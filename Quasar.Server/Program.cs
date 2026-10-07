// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Server.Build;
using Quasar.Server.Forms;
using System;
using System.Net;
using System.Windows.Forms;

namespace Quasar.Server
{
    internal static class Program
    {
        /// <summary>
        /// Laboratory baseline automation switch which opens the one click packaging console
        /// instead of the regular control console.
        /// </summary>
        private const string LabPackagerSwitch = "--lab-packager";

        [STAThread]
        private static int Main(string[] args)
        {
            // Laboratory baseline automation: headless certificate creation and client building.
            if (LabBuildCli.IsCliCommand(args))
                return LabBuildCli.Run(args);

            // Laboratory baseline automation: graphical one click packaging console.
            if (HasSwitch(args, LabPackagerSwitch))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FrmLabPackager());
                return 0;
            }

            // enable TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
            return 0;
        }

        private static bool HasSwitch(string[] args, string name)
        {
            if (args == null)
                return false;

            foreach (var arg in args)
            {
                if (string.Equals(arg, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
