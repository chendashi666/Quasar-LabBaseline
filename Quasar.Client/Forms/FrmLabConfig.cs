// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Common.Config;
using Quasar.Common.Properties;
using System;
using System.Windows.Forms;

namespace Quasar.Client.Forms
{
    /// <summary>
    /// Laboratory baseline parameter form.
    /// Edits the five run parameters of the baseline sample and persists them to
    /// <c>config.json</c> next to the running executable. Certificate binding fields which were
    /// written by <c>build_release.ps1</c> are preserved unchanged.
    /// </summary>
    public partial class FrmLabConfig : Form
    {
        private LabConfig _current;

        public FrmLabConfig()
        {
            InitializeComponent();
            _current = LabConfig.Load() ?? new LabConfig();
            LoadIntoUi(_current);
            lblPathValue.Text = LabConfig.DefaultPath;
            lblStatus.Text = string.Empty;
        }

        private void LoadIntoUi(LabConfig config)
        {
            txtHosts.Text = config.Hosts ?? string.Empty;
            try
            {
                numPort.Value = config.Port > 0 ? config.Port : 4782;
            }
            catch (ArgumentOutOfRangeException)
            {
                numPort.Value = 4782;
            }
            txtMutex.Text = config.Mutex ?? string.Empty;
            txtTag.Text = config.Tag ?? string.Empty;
            txtSubnet.Text = config.Subnet ?? string.Empty;
            UpdateBindingStatus(config);
        }

        private void UpdateBindingStatus(LabConfig config)
        {
            if (config.HasCertificateBinding)
            {
                lblBindingValue.Text = "已绑定实验台证书（保存时原样保留）";
                lblBindingValue.ForeColor = System.Drawing.Color.FromArgb(21, 128, 61);
            }
            else
            {
                lblBindingValue.Text = "未绑定证书：仅覆盖运行参数，需搭配控制端生成物使用";
                lblBindingValue.ForeColor = System.Drawing.Color.FromArgb(180, 83, 9);
            }
        }

        private void btnGenerateMutex_Click(object sender, EventArgs e)
        {
            txtMutex.Text = Guid.NewGuid().ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHosts.Text))
            {
                MessageBox.Show(this, "HOSTS 不能为空。请填写控制端 IP 或主机名。", "实验室基线样本",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHosts.Focus();
                return;
            }

            var config = _current != null ? _current.Clone() : new LabConfig();
            config.Hosts = txtHosts.Text.Trim();
            config.Port = (int)numPort.Value;
            config.Mutex = txtMutex.Text.Trim();
            if (string.IsNullOrWhiteSpace(config.Mutex))
                config.Mutex = Guid.NewGuid().ToString();
            config.Tag = txtTag.Text.Trim();
            config.Subnet = txtSubnet.Text.Trim();
            if (string.IsNullOrWhiteSpace(config.LabName))
                config.LabName = LabStrings.LabName;

            var path = LabConfig.DefaultPath;
            try
            {
                config.SaveTo(path);
                _current = config;
                LoadIntoUi(config);
                lblStatus.Text = "已保存: " + path;
                MessageBox.Show(this, "配置已保存到:\n" + path, "实验室基线样本",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "保存失败: " + ex.Message;
                MessageBox.Show(this, "保存失败:\n" + ex.Message, "实验室基线样本",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
