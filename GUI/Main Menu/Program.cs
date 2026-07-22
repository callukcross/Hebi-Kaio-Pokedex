using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace GUI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
            {
                RunSelfTest(); return;
            }
            Application.ThreadException += (_, eventArgs) => ReportCrash(eventArgs.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => ReportCrash(eventArgs.ExceptionObject as Exception ?? new Exception(eventArgs.ExceptionObject?.ToString()));
            Application.Run(new SplashForm());
        }

        private static void RunSelfTest()
        {
            var directory = Path.Combine(Path.GetTempPath(), "HebiKaio.UiSelfTest", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
            var profiles = new HebiKaio.Core.Profiles.ProfileService(new HebiKaio.Core.Profiles.JsonProfileRepository(Path.Combine(directory, "profiles.json"))); profiles.CreateProfile("UI Test");
            using var shell = new MainShellForm(directory) { ShowInTaskbar = false, Opacity = 0 };
            shell.Shown += (_, _) => shell.BeginInvoke((Action)(() =>
            {
                try { File.WriteAllText(Path.Combine(directory, "result.txt"), shell.RunNavigationSelfTest()); Environment.ExitCode = 0; }
                catch (Exception exception) { File.WriteAllText(Path.Combine(directory, "result.txt"), exception.ToString()); Environment.ExitCode = 1; }
                finally { shell.Close(); }
            }));
            Application.Run(shell);
        }

        private static void ReportCrash(Exception exception)
        {
            try { var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HebiKaioPokedex"); Directory.CreateDirectory(directory); File.AppendAllText(Path.Combine(directory, "error.log"), $"[{DateTimeOffset.Now:O}] {exception}\r\n\r\n"); }
            catch { }
            MessageBox.Show(exception.Message + "\n\nDetails were written to the HebiKaio error log.", "HebiKaio Pokédex error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
