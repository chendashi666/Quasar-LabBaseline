// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Server.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Quasar.Server.Build
{
    /// <summary>
    /// Laboratory certificate authority provisioning shared by the build CLI and the packaging
    /// console.
    /// <para>
    /// A baseline sample is only accepted by a control console which runs the very same authority:
    /// the release build compares the server certificate with the one embedded in the sample. The
    /// control side therefore resolves one single shared authority instead of letting every tool
    /// create its own. An authority which already exists in a sibling laboratory folder is
    /// adopted, a newly created one is propagated to the sibling folders.
    /// </para>
    /// </summary>
    public static class LabCertificate
    {
        public const string DefaultSubject = "Detection Evaluation Lab CA";
        public const int DefaultKeyStrength = 4096;

        /// <summary>
        /// Folder names of the control side tools which must share one laboratory authority.
        /// </summary>
        private static readonly string[] SiblingFolders = { "ControlConsole", "LabToolkit" };

        /// <summary>
        /// Returns the shared laboratory authority at <paramref name="path"/>.
        /// An authority found in a sibling laboratory folder is adopted first, a newly created one
        /// is propagated to the sibling folders so both control side tools stay in sync.
        /// </summary>
        public static X509Certificate2 Ensure(string path, string subject, int keyStrength, out bool created)
        {
            created = false;

            if (File.Exists(path))
                return Open(path);

            var adopted = TryAdoptFromSiblings(path);
            if (adopted != null)
                return adopted;

            var certificate = CertificateHelper.CreateCertificateAuthority(
                string.IsNullOrEmpty(subject) ? DefaultSubject : subject,
                keyStrength > 0 ? keyStrength : DefaultKeyStrength);

            var payload = certificate.Export(X509ContentType.Pkcs12);
            WriteAllBytes(path, payload);
            PropagateToSiblingFolders(path, payload);
            created = true;
            return certificate;
        }

        /// <summary>
        /// Returns the shared authority stored next to the running executable.
        /// </summary>
        public static X509Certificate2 EnsureAtDefaultPath(out bool created)
        {
            return Ensure(Quasar.Server.Models.Settings.CertificatePath, DefaultSubject, DefaultKeyStrength, out created);
        }

        /// <summary>
        /// Adopts an authority which already exists in a sibling laboratory folder of the running
        /// executable. Returns null when the laboratory has none yet.
        /// </summary>
        public static X509Certificate2 TryAdoptAtDefaultPath(out string sourcePath)
        {
            return TryAdoptFromSiblings(Quasar.Server.Models.Settings.CertificatePath, out sourcePath);
        }

        public static X509Certificate2 TryAdoptFromSiblings(string path)
        {
            string ignored;
            return TryAdoptFromSiblings(path, out ignored);
        }

        private static X509Certificate2 TryAdoptFromSiblings(string path, out string sourcePath)
        {
            sourcePath = null;

            foreach (var candidate in GetCandidatePaths(path))
            {
                if (!File.Exists(candidate))
                    continue;

                try
                {
                    WriteAllBytes(path, File.ReadAllBytes(candidate));
                    sourcePath = candidate;
                    return Open(path);
                }
                catch (Exception)
                {
                    // an unreadable candidate must not stop the search
                }
            }

            return null;
        }

        public static bool Exists()
        {
            return File.Exists(Quasar.Server.Models.Settings.CertificatePath);
        }

        private static X509Certificate2 Open(string path)
        {
            return new X509Certificate2(path, string.Empty, X509KeyStorageFlags.Exportable);
        }

        private static void WriteAllBytes(string path, byte[] payload)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(path, payload);
        }

        /// <summary>
        /// Gives every existing sibling laboratory tool folder the same authority file so the
        /// packaging toolkit and the control console can never drift apart. Existing authority
        /// files are never overwritten.
        /// </summary>
        public static void PropagateToSiblingFolders(string path, byte[] payload)
        {
            foreach (var target in GetSiblingPaths(path))
            {
                try
                {
                    var folder = Path.GetDirectoryName(target);
                    // only fill folders which already exist, never create a new layout
                    if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                        continue;
                    // never overwrite an authority which already exists
                    if (File.Exists(target))
                        continue;

                    WriteAllBytes(target, payload);
                }
                catch (Exception)
                {
                }
            }
        }

        private static IEnumerable<string> GetCandidatePaths(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
                yield break;

            var own = Path.GetFullPath(directory);
            var parent = Path.GetFullPath(Path.Combine(own, ".."));
            var fileName = Path.GetFileName(path);

            // 1) sibling laboratory tool folders, e.g. prebuilt/ControlConsole
            foreach (var sibling in GetSiblingPaths(path))
                yield return sibling;

            // 2) the parent folder, e.g. dist/quasar.p12 shared by both tools
            yield return Path.Combine(parent, fileName);
        }

        private static IEnumerable<string> GetSiblingPaths(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
                yield break;

            var own = Path.GetFullPath(directory);
            var parent = Path.GetFullPath(Path.Combine(own, ".."));
            var fileName = Path.GetFileName(path);

            foreach (var sibling in SiblingFolders)
            {
                var folder = Path.GetFullPath(Path.Combine(parent, sibling));
                if (string.Equals(folder, own, StringComparison.OrdinalIgnoreCase))
                    continue;

                yield return Path.Combine(folder, fileName);
            }
        }
    }
}
