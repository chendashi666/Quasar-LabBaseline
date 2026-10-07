// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

namespace Quasar.Client.Forms
{
    partial class FrmLabConfig
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Label lblPathValue;
        private System.Windows.Forms.GroupBox grpParams;
        private System.Windows.Forms.Label lblHosts;
        private System.Windows.Forms.TextBox txtHosts;
        private System.Windows.Forms.Label lblHostsHint;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label lblPortHint;
        private System.Windows.Forms.Label lblMutex;
        private System.Windows.Forms.TextBox txtMutex;
        private System.Windows.Forms.Button btnGenerateMutex;
        private System.Windows.Forms.Label lblMutexHint;
        private System.Windows.Forms.Label lblTag;
        private System.Windows.Forms.TextBox txtTag;
        private System.Windows.Forms.Label lblTagHint;
        private System.Windows.Forms.Label lblSubnet;
        private System.Windows.Forms.TextBox txtSubnet;
        private System.Windows.Forms.Label lblSubnetHint;
        private System.Windows.Forms.GroupBox grpBinding;
        private System.Windows.Forms.Label lblBinding;
        private System.Windows.Forms.Label lblBindingValue;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblPathValue = new System.Windows.Forms.Label();
            this.grpParams = new System.Windows.Forms.GroupBox();
            this.lblHosts = new System.Windows.Forms.Label();
            this.txtHosts = new System.Windows.Forms.TextBox();
            this.lblHostsHint = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblPortHint = new System.Windows.Forms.Label();
            this.lblMutex = new System.Windows.Forms.Label();
            this.txtMutex = new System.Windows.Forms.TextBox();
            this.btnGenerateMutex = new System.Windows.Forms.Button();
            this.lblMutexHint = new System.Windows.Forms.Label();
            this.lblTag = new System.Windows.Forms.Label();
            this.txtTag = new System.Windows.Forms.TextBox();
            this.lblTagHint = new System.Windows.Forms.Label();
            this.lblSubnet = new System.Windows.Forms.Label();
            this.txtSubnet = new System.Windows.Forms.TextBox();
            this.lblSubnetHint = new System.Windows.Forms.Label();
            this.grpBinding = new System.Windows.Forms.GroupBox();
            this.lblBinding = new System.Windows.Forms.Label();
            this.lblBindingValue = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grpParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpBinding.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(18, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 22);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "实验室基线样本 · 运行参数外置";
            //
            // lblPath
            //
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(20, 44);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(0, 13);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "配置文件:";
            //
            // lblPathValue
            //
            this.lblPathValue.AutoSize = true;
            this.lblPathValue.ForeColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.lblPathValue.Location = new System.Drawing.Point(88, 44);
            this.lblPathValue.Name = "lblPathValue";
            this.lblPathValue.Size = new System.Drawing.Size(0, 13);
            this.lblPathValue.TabIndex = 2;
            this.lblPathValue.Text = "-";
            //
            // grpParams
            //
            this.grpParams.Controls.Add(this.lblHosts);
            this.grpParams.Controls.Add(this.txtHosts);
            this.grpParams.Controls.Add(this.lblHostsHint);
            this.grpParams.Controls.Add(this.lblPort);
            this.grpParams.Controls.Add(this.numPort);
            this.grpParams.Controls.Add(this.lblPortHint);
            this.grpParams.Controls.Add(this.lblMutex);
            this.grpParams.Controls.Add(this.txtMutex);
            this.grpParams.Controls.Add(this.btnGenerateMutex);
            this.grpParams.Controls.Add(this.lblMutexHint);
            this.grpParams.Controls.Add(this.lblTag);
            this.grpParams.Controls.Add(this.txtTag);
            this.grpParams.Controls.Add(this.lblTagHint);
            this.grpParams.Controls.Add(this.lblSubnet);
            this.grpParams.Controls.Add(this.txtSubnet);
            this.grpParams.Controls.Add(this.lblSubnetHint);
            this.grpParams.Location = new System.Drawing.Point(22, 70);
            this.grpParams.Name = "grpParams";
            this.grpParams.Size = new System.Drawing.Size(560, 300);
            this.grpParams.TabIndex = 3;
            this.grpParams.TabStop = false;
            this.grpParams.Text = "五项实验参数（保存后立即生效，无需重新编译）";
            //
            // lblHosts
            //
            this.lblHosts.AutoSize = true;
            this.lblHosts.Location = new System.Drawing.Point(16, 30);
            this.lblHosts.Name = "lblHosts";
            this.lblHosts.Size = new System.Drawing.Size(0, 13);
            this.lblHosts.TabIndex = 0;
            this.lblHosts.Text = "HOSTS（控制端地址）";
            //
            // txtHosts
            //
            this.txtHosts.Location = new System.Drawing.Point(190, 27);
            this.txtHosts.Name = "txtHosts";
            this.txtHosts.Size = new System.Drawing.Size(350, 21);
            this.txtHosts.TabIndex = 1;
            //
            // lblHostsHint
            //
            this.lblHostsHint.AutoSize = true;
            this.lblHostsHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblHostsHint.Location = new System.Drawing.Point(190, 51);
            this.lblHostsHint.Name = "lblHostsHint";
            this.lblHostsHint.Size = new System.Drawing.Size(0, 13);
            this.lblHostsHint.TabIndex = 2;
            this.lblHostsHint.Text = "默认 10.10.10.10；多个用 ; 分隔；不带端口时自动补 PORT";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(16, 82);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(0, 13);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "PORT（监听端口）";
            //
            // numPort
            //
            this.numPort.Location = new System.Drawing.Point(190, 79);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 21);
            this.numPort.TabIndex = 4;
            this.numPort.Value = new decimal(new int[] { 4782, 0, 0, 0 });
            //
            // lblPortHint
            //
            this.lblPortHint.AutoSize = true;
            this.lblPortHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblPortHint.Location = new System.Drawing.Point(320, 82);
            this.lblPortHint.Name = "lblPortHint";
            this.lblPortHint.Size = new System.Drawing.Size(0, 13);
            this.lblPortHint.TabIndex = 5;
            this.lblPortHint.Text = "默认 4782，与控制端 ListenPort 一致";
            //
            // lblMutex
            //
            this.lblMutex.AutoSize = true;
            this.lblMutex.Location = new System.Drawing.Point(16, 134);
            this.lblMutex.Name = "lblMutex";
            this.lblMutex.Size = new System.Drawing.Size(0, 13);
            this.lblMutex.TabIndex = 6;
            this.lblMutex.Text = "MUTEX（单实例互斥）";
            //
            // txtMutex
            //
            this.txtMutex.Location = new System.Drawing.Point(190, 131);
            this.txtMutex.Name = "txtMutex";
            this.txtMutex.Size = new System.Drawing.Size(250, 21);
            this.txtMutex.TabIndex = 7;
            //
            // btnGenerateMutex
            //
            this.btnGenerateMutex.Location = new System.Drawing.Point(448, 129);
            this.btnGenerateMutex.Name = "btnGenerateMutex";
            this.btnGenerateMutex.Size = new System.Drawing.Size(92, 25);
            this.btnGenerateMutex.TabIndex = 8;
            this.btnGenerateMutex.Text = "生成 GUID";
            this.btnGenerateMutex.UseVisualStyleBackColor = true;
            this.btnGenerateMutex.Click += new System.EventHandler(this.btnGenerateMutex_Click);
            //
            // lblMutexHint
            //
            this.lblMutexHint.AutoSize = true;
            this.lblMutexHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblMutexHint.Location = new System.Drawing.Point(190, 155);
            this.lblMutexHint.Name = "lblMutexHint";
            this.lblMutexHint.Size = new System.Drawing.Size(0, 13);
            this.lblMutexHint.TabIndex = 9;
            this.lblMutexHint.Text = "留空则自动生成 GUID；同一实验条件下保持固定";
            //
            // lblTag
            //
            this.lblTag.AutoSize = true;
            this.lblTag.Location = new System.Drawing.Point(16, 186);
            this.lblTag.Name = "lblTag";
            this.lblTag.Size = new System.Drawing.Size(0, 13);
            this.lblTag.TabIndex = 10;
            this.lblTag.Text = "TAG（实验条件标签）";
            //
            // txtTag
            //
            this.txtTag.Location = new System.Drawing.Point(190, 183);
            this.txtTag.Name = "txtTag";
            this.txtTag.Size = new System.Drawing.Size(350, 21);
            this.txtTag.TabIndex = 11;
            //
            // lblTagHint
            //
            this.lblTagHint.AutoSize = true;
            this.lblTagHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblTagHint.Location = new System.Drawing.Point(190, 207);
            this.lblTagHint.Name = "lblTagHint";
            this.lblTagHint.Size = new System.Drawing.Size(0, 13);
            this.lblTagHint.TabIndex = 12;
            this.lblTagHint.Text = "例如 COND-A / COND-B，用于区分对照组";
            //
            // lblSubnet
            //
            this.lblSubnet.AutoSize = true;
            this.lblSubnet.Location = new System.Drawing.Point(16, 238);
            this.lblSubnet.Name = "lblSubnet";
            this.lblSubnet.Size = new System.Drawing.Size(0, 13);
            this.lblSubnet.TabIndex = 13;
            this.lblSubnet.Text = "SUBNET（内网网段）";
            //
            // txtSubnet
            //
            this.txtSubnet.Location = new System.Drawing.Point(190, 235);
            this.txtSubnet.Name = "txtSubnet";
            this.txtSubnet.Size = new System.Drawing.Size(350, 21);
            this.txtSubnet.TabIndex = 14;
            //
            // lblSubnetHint
            //
            this.lblSubnetHint.AutoSize = true;
            this.lblSubnetHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblSubnetHint.Location = new System.Drawing.Point(190, 259);
            this.lblSubnetHint.Name = "lblSubnetHint";
            this.lblSubnetHint.Size = new System.Drawing.Size(0, 13);
            this.lblSubnetHint.TabIndex = 15;
            this.lblSubnetHint.Text = "默认 10.10.10.0/24；实验记录用元数据（上游 1.4.1 无消费点）";
            //
            // grpBinding
            //
            this.grpBinding.Controls.Add(this.lblBinding);
            this.grpBinding.Controls.Add(this.lblBindingValue);
            this.grpBinding.Location = new System.Drawing.Point(22, 380);
            this.grpBinding.Name = "grpBinding";
            this.grpBinding.Size = new System.Drawing.Size(560, 62);
            this.grpBinding.TabIndex = 4;
            this.grpBinding.TabStop = false;
            this.grpBinding.Text = "证书绑定（由 build_release.ps1 写入）";
            //
            // lblBinding
            //
            this.lblBinding.AutoSize = true;
            this.lblBinding.Location = new System.Drawing.Point(16, 28);
            this.lblBinding.Name = "lblBinding";
            this.lblBinding.Size = new System.Drawing.Size(0, 13);
            this.lblBinding.TabIndex = 0;
            this.lblBinding.Text = "状态:";
            //
            // lblBindingValue
            //
            this.lblBindingValue.AutoSize = true;
            this.lblBindingValue.Location = new System.Drawing.Point(58, 28);
            this.lblBindingValue.Name = "lblBindingValue";
            this.lblBindingValue.Size = new System.Drawing.Size(0, 13);
            this.lblBindingValue.TabIndex = 1;
            this.lblBindingValue.Text = "-";
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(392, 456);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(490, 456);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 30);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(21, 128, 61);
            this.lblStatus.Location = new System.Drawing.Point(24, 464);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "";
            //
            // FrmLabConfig
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.lblPathValue);
            this.Controls.Add(this.grpParams);
            this.Controls.Add(this.grpBinding);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLabConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quasar 实验室基线样本 - 参数配置";
            this.grpParams.ResumeLayout(false);
            this.grpParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpBinding.ResumeLayout(false);
            this.grpBinding.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
