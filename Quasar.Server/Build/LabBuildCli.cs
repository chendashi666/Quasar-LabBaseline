// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Common.Config;
using Quasar.Common.Properties;
using Quasar.Server.Helper;
using Quasar.Server.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Quasar.Server.Build
{
    /// <summary>
    /// Headless laboratory automation entry point of the control console.
    /// It exposes the project's own certificate authority and client builder to the build script
    /// so the baseline sample can be produced without the GUI and without third party tooling.
    /// </summary>
    public static class LabBuildCli
    {
        public static bool IsCliCommand(string[] args)
        {
            if (args == null || args.Length == 0)
                return false;

            return string.Equals(args[0], "--build-client", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(args[0], "--make-cert", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(args[0], "--write-config", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(args[0], "--help-lab", StringComparison.OrdinalIgnoreCase);
        }

        public static int Run(string[] args)
        {
            try
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "--make-cert":
                        return MakeCertificate(args);
                    case "--build-client":
                        return BuildClient(args);
                    case "--write-config":
                        return WriteConfiguration(args);
                    default:
                        PrintUsage();
                        return 0;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static int MakeCertificate(string[] args)
        {
            var options = Parse(args);
            var path = Path.GetFullPath(Get(options, "out", Quasar.Server.Models.Settings.CertificatePath));
            var name = Get(options, "name", "Detection Evaluation Lab CA");
            var strength = ParseInt(Get(options, "key", "4096"), 4096);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var certificate = CertificateHelper.CreateCertificateAuthority(name, strength);
            File.WriteAllBytes(path, certificate.Export(X509ContentType.Pkcs12));

            Console.WriteLine("certificate=" + path);
            Console.WriteLine("subject=" + certificate.Subject);
            Console.WriteLine("thumbprint=" + certificate.Thumbprint);
            Console.WriteLine("notafter=" + certificate.NotAfter.ToString("o", CultureInfo.InvariantCulture));
            return 0;
        }

        private static int BuildClient(string[] args)
        {
            var options = Parse(args);
            var outPath = Get(options, "out", null);
            if (string.IsNullOrEmpty(outPath))
                throw new ArgumentException("--out is required, e.g. --out dist/LabBaseline/LabBaseline.exe");

            outPath = Path.GetFullPath(outPath);
            var outDirectory = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDirectory) && !Directory.Exists(outDirectory))
                Directory.CreateDirectory(outDirectory);

            var hosts = Get(options, "hosts", null);
            if (string.IsNullOrEmpty(hosts))
                throw new ArgumentException("--hosts is required, e.g. --hosts \"10.10.10.10:4782;\"");
            hosts = hosts.Trim();
            if (!hosts.EndsWith(";", StringComparison.Ordinal))
                hosts += ";";

            var clientBin = Path.GetFullPath(Get(options, "client-bin",
                Path.Combine(Application.StartupPath, LabStrings.ClientBinaryName)));
            if (!File.Exists(clientBin))
                throw new FileNotFoundException("client.bin was not found. Build Quasar.sln first.", clientBin);
            bool certificateCreated;
            var laboratoryCertificate = LabCertificate.EnsureAtDefaultPath(out certificateCreated);
            if (certificateCreated)
            {
                Console.WriteLine("certificate_generated=" + Quasar.Server.Models.Settings.CertificatePath);
                Console.WriteLine("certificate_thumbprint=" + laboratoryCertificate.Thumbprint);
            }

            var buildOptions = new BuildOptions
            {
                OutputPath = outPath,
                Tag = Get(options, "tag", "LAB"),
                Mutex = Get(options, "mutex", Guid.NewGuid().ToString()),
                UnattendedMode = ParseBool(Get(options, "unattended", "true")),
                RawHosts = hosts,
                Delay = ParseInt(Get(options, "delay", "5000"), 5000),
                Version = Application.ProductVersion,
                IconPath = Get(options, "icon", string.Empty),
                InstallPath = 1,
                InstallSub = Get(options, "install-sub", LabStrings.DefaultInstallSubDirectory),
                InstallName = Get(options, "install-name", Path.GetFileName(outPath)),
                StartupName = Get(options, "startup-name", "LabBaseline"),
                Install = ParseBool(Get(options, "install", "false")),
                Startup = ParseBool(Get(options, "startup", "false")),
                HideFile = false,
                HideInstallSubdirectory = false,
                Keylogger = ParseBool(Get(options, "keylogger", "false")),
                LogDirectoryName = Get(options, "log-dir", LabStrings.DefaultLogDirectory),
                HideLogDirectory = false
            };

            var subnet = Get(options, "subnet", string.Empty);
            var builder = new ClientBuilder(buildOptions, clientBin);
            builder.LabSubnet = subnet;
            builder.Build();

            // External run parameters of the produced baseline sample (override layer).
            var config = LabConfigFactory.FromBuildOptions(buildOptions);
            config.Subnet = subnet;
            var configPath = Path.Combine(outDirectory ?? Application.StartupPath, LabConfig.FileName);
            config.SaveTo(configPath);

            Console.WriteLine("client=" + outPath);
            Console.WriteLine("config=" + configPath);
            Console.WriteLine("hosts=" + hosts);
            Console.WriteLine("tag=" + buildOptions.Tag);
            Console.WriteLine("mutex=" + buildOptions.Mutex);
            Console.WriteLine("certificate=" + Quasar.Server.Models.Settings.CertificatePath);
            return 0;
        }

        private static int WriteConfiguration(string[] args)
        {
            var options = Parse(args);
            var outDirectory = Path.GetFullPath(Get(options, "out", Application.StartupPath));
            if (!Directory.Exists(outDirectory))
                Directory.CreateDirectory(outDirectory);

            var hosts = Get(options, "hosts", null);
            if (string.IsNullOrEmpty(hosts))
                throw new ArgumentException("--hosts is required");
            hosts = hosts.Trim();
            if (!hosts.EndsWith(";", StringComparison.Ordinal))
                hosts += ";";

            var config = new LabConfig
            {
                Hosts = hosts,
                Port = LabConfigFactory.ExtractFirstPort(hosts),
                Mutex = Get(options, "mutex", Guid.NewGuid().ToString()),
                Tag = Get(options, "tag", "LAB"),
                Subnet = Get(options, "subnet", string.Empty),
                ReconnectDelay = ParseInt(Get(options, "delay", "5000"), 5000),
                LogDirectoryName = Get(options, "log-dir", LabStrings.DefaultLogDirectory),
                InstallSubDirectory = Get(options, "install-sub", LabStrings.DefaultInstallSubDirectory),
                UnattendedMode = ParseBool(Get(options, "unattended", "true"))
            };

            LabConfigFactory.WithCertificateBinding(config);

            var configPath = Path.Combine(outDirectory, LabConfig.FileName);
            config.SaveTo(configPath);
            Console.WriteLine("config=" + configPath);
            return 0;
        }

        private static Dictionary<string, string> Parse(string[] args)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrEmpty(arg) || !arg.StartsWith("--", StringComparison.Ordinal))
                    continue;

                var key = arg.Substring(2);
                var value = "true";
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    value = args[++i];
                map[key] = value;
            }
            return map;
        }

        private static string Get(Dictionary<string, string> map, string key, string fallback)
        {
            string value;
            return map.TryGetValue(key, out value) && !string.IsNullOrEmpty(value) ? value : fallback;
        }

        private static int ParseInt(string value, int fallback)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : fallback;
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : string.Equals(value, "1", StringComparison.Ordinal);
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Quasar laboratory baseline toolchain");
            Console.WriteLine("作者：陈森（Chen Sen） - https://github.com/chendashi666");
            Console.WriteLine("禁止盗卖，禁止商业用途 / No resale, no commercial use");
            Console.WriteLine("上游 Quasar (c) MaxXor and contributors, MIT License");
            Console.WriteLine();
            Console.WriteLine("  Quasar.exe | QuasarLabToolkit.exe --make-cert [--out <p12>] [--name <CN>] [--key 4096]");
            Console.WriteLine("      Creates the laboratory certificate authority (quasar.p12) with the");
            Console.WriteLine("      project's own certificate helper.");
            Console.WriteLine();
            Console.WriteLine("  ... --build-client --out <exe> --hosts \"<ip:port;>\" [options]");
            Console.WriteLine("      Builds a functionality complete baseline client and writes config.json next to it.");
            Console.WriteLine("      Options: --tag --mutex --subnet --delay --install-sub --install-name");
            Console.WriteLine("               --startup-name --log-dir --icon --unattended --install --startup --keylogger");
            Console.WriteLine();
            Console.WriteLine("  ... --write-config --out <dir> --hosts \"<ip:port;>\" [--tag --mutex --subnet]");
            Console.WriteLine("      Only (re)writes config.json so one build can be re-pointed between conditions.");
        }
    }
}
