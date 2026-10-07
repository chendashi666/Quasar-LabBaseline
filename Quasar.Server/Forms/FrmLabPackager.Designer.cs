// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

namespace Quasar.Server.Forms
{
    partial class FrmLabPackager
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.GroupBox grpInput;
        private System.Windows.Forms.Label lblHosts;
        private System.Windows.Forms.TextBox txtHosts;
        private System.Windows.Forms.Label lblHostsHint;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label lblTag;
        private System.Windows.Forms.TextBox txtTag;
        private System.Windows.Forms.Label lblSubnet;
        private System.Windows.Forms.TextBox txtSubnet;
        private System.Windows.Forms.Label lblMutex;
        private System.Windows.Forms.TextBox txtMutex;
        private System.Windows.Forms.Button btnGenerateMutex;
        private System.Windows.Forms.Label lblMutexHint;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.Label lblDir;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.CheckBox chkWriteConfig;
        private System.Windows.Forms.Label lblOutputHint;
        private System.Windows.Forms.Button btnPackage;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblCopyright;

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
            this.lblHint = new System.Windows.Forms.Label();
            this.grpInput = new System.Windows.Forms.GroupBox();
            this.lblHosts = new System.Windows.Forms.Label();
            this.txtHosts = new System.Windows.Forms.TextBox();
            this.lblHostsHint = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblTag = new System.Windows.Forms.Label();
            this.txtTag = new System.Windows.Forms.TextBox();
            this.lblSubnet = new System.Windows.Forms.Label();
            this.txtSubnet = new System.Windows.Forms.TextBox();
            this.lblMutex = new System.Windows.Forms.Label();
            this.txtMutex = new System.Windows.Forms.TextBox();
            this.btnGenerateMutex = new System.Windows.Forms.Button();
            this.lblMutexHint = new System.Windows.Forms.Label();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.lblDir = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.chkWriteConfig = new System.Windows.Forms.CheckBox();
            this.lblOutputHint = new System.Windows.Forms.Label();
            this.btnPackage = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.grpInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpOutput.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 24);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "实验室一键出包控制台";
            //
            // lblHint
            //
            this.lblHint.AutoSize = true;
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(75, 85, 99);
            this.lblHint.Location = new System.Drawing.Point(18, 42);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(0, 13);
            this.lblHint.TabIndex = 1;
            this.lblHint.Text = "填入控制端内网 IP，点一次按钮 → 直接得到可投递的被控端 exe（运行参数已内嵌）。";
            //
            // grpInput
            //
            this.grpInput.Controls.Add(this.lblHosts);
            this.grpInput.Controls.Add(this.txtHosts);
            this.grpInput.Controls.Add(this.lblHostsHint);
            this.grpInput.Controls.Add(this.lblPort);
            this.grpInput.Controls.Add(this.numPort);
            this.grpInput.Controls.Add(this.lblTag);
            this.grpInput.Controls.Add(this.txtTag);
            this.grpInput.Controls.Add(this.lblSubnet);
            this.grpInput.Controls.Add(this.txtSubnet);
            this.grpInput.Controls.Add(this.lblMutex);
            this.grpInput.Controls.Add(this.txtMutex);
            this.grpInput.Controls.Add(this.btnGenerateMutex);
            this.grpInput.Controls.Add(this.lblMutexHint);
            this.grpInput.Location = new System.Drawing.Point(18, 66);
            this.grpInput.Name = "grpInput";
            this.grpInput.Size = new System.Drawing.Size(724, 236);
            this.grpInput.TabIndex = 2;
            this.grpInput.TabStop = false;
            this.grpInput.Text = "实验条件参数";
            //
            // lblHosts
            //
            this.lblHosts.AutoSize = true;
            this.lblHosts.Location = new System.Drawing.Point(16, 34);
            this.lblHosts.Name = "lblHosts";
            this.lblHosts.Size = new System.Drawing.Size(0, 13);
            this.lblHosts.TabIndex = 0;
            this.lblHosts.Text = "控制端内网 IP";
            //
            // txtHosts
            //
            this.txtHosts.AcceptsReturn = true;
            this.txtHosts.Location = new System.Drawing.Point(150, 28);
            this.txtHosts.Multiline = true;
            this.txtHosts.Name = "txtHosts";
            this.txtHosts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHosts.Size = new System.Drawing.Size(556, 48);
            this.txtHosts.TabIndex = 1;
            //
            // lblHostsHint
            //
            this.lblHostsHint.AutoSize = true;
            this.lblHostsHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblHostsHint.Location = new System.Drawing.Point(150, 80);
            this.lblHostsHint.Name = "lblHostsHint";
            this.lblHostsHint.Size = new System.Drawing.Size(0, 13);
            this.lblHostsHint.TabIndex = 2;
            this.lblHostsHint.Text = "示例 10.10.10.10；多个用 ; 分隔；不带端口自动补下面的端口";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(16, 112);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(0, 13);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "端口";
            //
            // numPort
            //
            this.numPort.Location = new System.Drawing.Point(150, 109);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(90, 21);
            this.numPort.TabIndex = 4;
            this.numPort.Value = new decimal(new int[] { 4782, 0, 0, 0 });
            //
            // lblTag
            //
            this.lblTag.AutoSize = true;
            this.lblTag.Location = new System.Drawing.Point(262, 112);
            this.lblTag.Name = "lblTag";
            this.lblTag.Size = new System.Drawing.Size(0, 13);
            this.lblTag.TabIndex = 5;
            this.lblTag.Text = "TAG";
            //
            // txtTag
            //
            this.txtTag.Location = new System.Drawing.Point(316, 109);
            this.txtTag.Name = "txtTag";
            this.txtTag.Size = new System.Drawing.Size(160, 21);
            this.txtTag.TabIndex = 6;
            this.txtTag.Text = "COND-A";
            //
            // lblSubnet
            //
            this.lblSubnet.AutoSize = true;
            this.lblSubnet.Location = new System.Drawing.Point(492, 112);
            this.lblSubnet.Name = "lblSubnet";
            this.lblSubnet.Size = new System.Drawing.Size(0, 13);
            this.lblSubnet.TabIndex = 7;
            this.lblSubnet.Text = "SUBNET";
            //
            // txtSubnet
            //
            this.txtSubnet.Location = new System.Drawing.Point(558, 109);
            this.txtSubnet.Name = "txtSubnet";
            this.txtSubnet.Size = new System.Drawing.Size(148, 21);
            this.txtSubnet.TabIndex = 8;
            this.txtSubnet.Text = "10.10.10.0/24";
            //
            // lblMutex
            //
            this.lblMutex.AutoSize = true;
            this.lblMutex.Location = new System.Drawing.Point(16, 156);
            this.lblMutex.Name = "lblMutex";
            this.lblMutex.Size = new System.Drawing.Size(0, 13);
            this.lblMutex.TabIndex = 9;
            this.lblMutex.Text = "MUTEX";
            //
            // txtMutex
            //
            this.txtMutex.Location = new System.Drawing.Point(150, 153);
            this.txtMutex.Name = "txtMutex";
            this.txtMutex.Size = new System.Drawing.Size(430, 21);
            this.txtMutex.TabIndex = 10;
            //
            // btnGenerateMutex
            //
            this.btnGenerateMutex.Location = new System.Drawing.Point(590, 151);
            this.btnGenerateMutex.Name = "btnGenerateMutex";
            this.btnGenerateMutex.Size = new System.Drawing.Size(116, 25);
            this.btnGenerateMutex.TabIndex = 11;
            this.btnGenerateMutex.Text = "生成 GUID";
            this.btnGenerateMutex.UseVisualStyleBackColor = true;
            this.btnGenerateMutex.Click += new System.EventHandler(this.btnGenerateMutex_Click);
            //
            // lblMutexHint
            //
            this.lblMutexHint.AutoSize = true;
            this.lblMutexHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblMutexHint.Location = new System.Drawing.Point(150, 180);
            this.lblMutexHint.Name = "lblMutexHint";
            this.lblMutexHint.Size = new System.Drawing.Size(0, 13);
            this.lblMutexHint.TabIndex = 12;
            this.lblMutexHint.Text = "单实例互斥名；留空自动生成；同一实验条件下保持固定";
            //
            // grpOutput
            //
            this.grpOutput.Controls.Add(this.lblDir);
            this.grpOutput.Controls.Add(this.txtOutputDir);
            this.grpOutput.Controls.Add(this.btnBrowse);
            this.grpOutput.Controls.Add(this.lblFileName);
            this.grpOutput.Controls.Add(this.txtFileName);
            this.grpOutput.Controls.Add(this.chkWriteConfig);
            this.grpOutput.Controls.Add(this.lblOutputHint);
            this.grpOutput.Location = new System.Drawing.Point(18, 310);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(724, 128);
            this.grpOutput.TabIndex = 3;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "输出";
            //
            // lblDir
            //
            this.lblDir.AutoSize = true;
            this.lblDir.Location = new System.Drawing.Point(16, 32);
            this.lblDir.Name = "lblDir";
            this.lblDir.Size = new System.Drawing.Size(0, 13);
            this.lblDir.TabIndex = 0;
            this.lblDir.Text = "输出目录";
            //
            // txtOutputDir
            //
            this.txtOutputDir.Location = new System.Drawing.Point(120, 29);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(474, 21);
            this.txtOutputDir.TabIndex = 1;
            //
            // btnBrowse
            //
            this.btnBrowse.Location = new System.Drawing.Point(602, 27);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(104, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            //
            // lblFileName
            //
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(16, 68);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(0, 13);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "文件名";
            //
            // txtFileName
            //
            this.txtFileName.Location = new System.Drawing.Point(120, 65);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(180, 21);
            this.txtFileName.TabIndex = 4;
            this.txtFileName.Text = "LabBaseline.exe";
            //
            // chkWriteConfig
            //
            this.chkWriteConfig.AutoSize = true;
            this.chkWriteConfig.Checked = true;
            this.chkWriteConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWriteConfig.Location = new System.Drawing.Point(320, 67);
            this.chkWriteConfig.Name = "chkWriteConfig";
            this.chkWriteConfig.Size = new System.Drawing.Size(0, 17);
            this.chkWriteConfig.TabIndex = 5;
            this.chkWriteConfig.Text = "同时输出 config.json（切换实验条件时只改它，不用重新出包）";
            this.chkWriteConfig.UseVisualStyleBackColor = true;
            //
            // lblOutputHint
            //
            this.lblOutputHint.AutoSize = true;
            this.lblOutputHint.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.lblOutputHint.Location = new System.Drawing.Point(120, 94);
            this.lblOutputHint.Name = "lblOutputHint";
            this.lblOutputHint.Size = new System.Drawing.Size(0, 13);
            this.lblOutputHint.TabIndex = 6;
            this.lblOutputHint.Text = "出包后可只投递 exe（参数已内嵌）；config.json 存在时优先覆盖内嵌值。";
            //
            // btnPackage
            //
            this.btnPackage.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPackage.Location = new System.Drawing.Point(18, 450);
            this.btnPackage.Name = "btnPackage";
            this.btnPackage.Size = new System.Drawing.Size(190, 42);
            this.btnPackage.TabIndex = 4;
            this.btnPackage.Text = "一键出包";
            this.btnPackage.UseVisualStyleBackColor = true;
            this.btnPackage.Click += new System.EventHandler(this.btnPackage_Click);
            //
            // btnOpenFolder
            //
            this.btnOpenFolder.Location = new System.Drawing.Point(220, 450);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(150, 42);
            this.btnOpenFolder.TabIndex = 5;
            this.btnOpenFolder.Text = "打开输出目录";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(620, 450);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(122, 42);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(21, 128, 61);
            this.lblStatus.Location = new System.Drawing.Point(18, 502);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "就绪";
            //
            // txtLog
            //
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(17, 24, 39);
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txtLog.Location = new System.Drawing.Point(18, 524);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(724, 146);
            this.txtLog.TabIndex = 8;
            this.txtLog.WordWrap = false;
            //
            // lblCopyright
            //
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.ForeColor = System.Drawing.Color.FromArgb(120, 53, 15);
            this.lblCopyright.Location = new System.Drawing.Point(18, 680);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(0, 13);
            this.lblCopyright.TabIndex = 9;
            this.lblCopyright.Text = "作者：陈森（Chen Sen）· https://github.com/chendashi666　|　禁止盗卖 · 禁止商业用途 | 上游 Quasar (c) MaxXor (MIT)";
            //
            // FrmLabPackager
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 706);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.grpInput);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.btnPackage);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblCopyright);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLabPackager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quasar 实验室一键出包控制台";
            this.grpInput.ResumeLayout(false);
            this.grpInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpOutput.ResumeLayout(false);
            this.grpOutput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
