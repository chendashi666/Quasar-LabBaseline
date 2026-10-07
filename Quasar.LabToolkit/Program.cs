// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Server.Build;
using Quasar.Server.Forms;
using System;
using System.Windows.Forms;

namespace Quasar.LabToolkit
{
    /// <summary>
    /// Standalone laboratory control side toolkit.
    /// <para>
    /// Started without arguments it opens the one click packaging console. Started with a build
    /// switch it behaves exactly like the control console CLI, so the toolkit can drive the whole
    /// pipeline headlessly.
    /// </para>
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (LabBuildCli.IsCliCommand(args))
                return LabBuildCli.Run(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLabPackager());
            return 0;
        }
    }
}
