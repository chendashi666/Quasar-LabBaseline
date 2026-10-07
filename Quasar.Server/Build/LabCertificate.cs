// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Server.Helper;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Quasar.Server.Build
{
    /// <summary>
    /// Laboratory certificate authority provisioning shared by the build CLI and the packaging
    /// console.
    /// <para>
    /// The authority is created on demand, so a prebuilt toolkit is usable out of the box on a
    /// machine which never ran the build pipeline and therefore has no quasar.p12 yet.
    /// </para>
    /// </summary>
    public static class LabCertificate
    {
        public const string DefaultSubject = "Detection Evaluation Lab CA";
        public const int DefaultKeyStrength = 4096;

        /// <summary>
        /// Returns the laboratory authority at <paramref name="path"/>, creating it first when absent.
        /// </summary>
        public static X509Certificate2 Ensure(string path, string subject, int keyStrength, out bool created)
        {
            created = false;

            if (File.Exists(path))
                return new X509Certificate2(path, string.Empty, X509KeyStorageFlags.Exportable);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var certificate = CertificateHelper.CreateCertificateAuthority(
                string.IsNullOrEmpty(subject) ? DefaultSubject : subject,
                keyStrength > 0 ? keyStrength : DefaultKeyStrength);

            File.WriteAllBytes(path, certificate.Export(X509ContentType.Pkcs12));
            created = true;
            return certificate;
        }

        /// <summary>
        /// Returns the laboratory authority stored next to the running executable.
        /// </summary>
        public static X509Certificate2 EnsureAtDefaultPath(out bool created)
        {
            return Ensure(Quasar.Server.Models.Settings.CertificatePath, DefaultSubject, DefaultKeyStrength, out created);
        }

        public static bool Exists()
        {
            return File.Exists(Quasar.Server.Models.Settings.CertificatePath);
        }
    }
}
