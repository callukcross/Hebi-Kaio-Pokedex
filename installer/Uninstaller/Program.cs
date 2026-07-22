using System.Diagnostics;
using Microsoft.Win32;

internal static class Program
{
    private const string ProductName = "HebiKaio Pokedex";
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\HebiKaioPokedex";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var installationDirectory = FindInstallationDirectory();
            if (args.Contains("--validate", StringComparer.OrdinalIgnoreCase)) { Environment.ExitCode = installationDirectory is null ? 1 : 0; return; }
            if (installationDirectory is null) throw new InvalidOperationException("HebiKaio Pokedex is not currently installed for this Windows account.");
            if (MessageBox.Show($"Remove {ProductName}?\n\nSaved profiles and custom modules will be kept.", "Uninstall " + ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            DeleteShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), ProductName + ".lnk"));
            DeleteShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), ProductName + ".lnk"));
            Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, throwOnMissingSubKey: false);
            ScheduleRemoval(installationDirectory);
            MessageBox.Show(ProductName + " will finish removing itself in the background. Your saved profiles were kept.", "Uninstall complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Uninstall failed", MessageBoxButtons.OK, MessageBoxIcon.Error); Environment.ExitCode = 1;
        }
    }

    private static string? FindInstallationDirectory()
    {
        var executableDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        if (IsInstallationDirectory(executableDirectory)) return executableDirectory;
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
        var registered = key?.GetValue("InstallLocation") as string;
        return !string.IsNullOrWhiteSpace(registered) && IsInstallationDirectory(registered) ? Path.GetFullPath(registered) : null;
    }

    private static bool IsInstallationDirectory(string directory)
    {
        var full = Path.GetFullPath(directory); var root = Path.GetPathRoot(full);
        return !string.Equals(full.TrimEnd('\\'), root?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(full, ".hebikaio-install")) && File.Exists(Path.Combine(full, "HebiKaioPokedex.exe"));
    }

    private static void ScheduleRemoval(string directory)
    {
        var script = Path.Combine(Path.GetTempPath(), "HebiKaio-uninstall-" + Guid.NewGuid().ToString("N") + ".cmd");
        File.WriteAllText(script, $"@echo off\r\ncd /d \"%TEMP%\"\r\nfor /L %%i in (1,1,10) do (\r\n  rmdir /s /q \"{directory.TrimEnd('\\')}\" 2>nul\r\n  if not exist \"{directory.TrimEnd('\\')}\" goto done\r\n  timeout /t 1 /nobreak >nul\r\n)\r\n:done\r\ndel /q \"%~f0\"\r\n");
        Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{script}\"") { CreateNoWindow = true, UseShellExecute = false, WorkingDirectory = Path.GetTempPath() });
    }

    private static void DeleteShortcut(string path) { if (File.Exists(path)) File.Delete(path); }
}
