using System.Diagnostics;
using Microsoft.Win32;

const string productName = "HebiKaio Pokedex";
var currentDirectory = Path.GetFullPath(AppContext.BaseDirectory);

try
{
    if (!File.Exists(Path.Combine(currentDirectory, ".hebikaio-install")) || !File.Exists(Path.Combine(currentDirectory, "HebiKaioPokedex.exe")) || string.Equals(currentDirectory.TrimEnd('\\'), Path.GetPathRoot(currentDirectory)?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("The uninstaller is not running from a valid HebiKaio Pokedex installation directory.");
    if (MessageBox.Show($"Remove {productName}?\n\nSaved profiles and custom modules will be kept.", "Uninstall " + productName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        return;

    DeleteShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), productName + ".lnk"));
    DeleteShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), productName + ".lnk"));
    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\HebiKaioPokedex", throwOnMissingSubKey: false);

    var script = Path.Combine(Path.GetTempPath(), "HebiKaio-uninstall-" + Guid.NewGuid().ToString("N") + ".cmd");
    File.WriteAllText(script, $"@echo off\r\ntimeout /t 2 /nobreak >nul\r\nrmdir /s /q \"{currentDirectory}\"\r\ndel /q \"%~f0\"\r\n");
    Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{script}\"") { CreateNoWindow = true, UseShellExecute = false });
    MessageBox.Show(productName + " was removed. Your saved profiles were kept.", "Uninstall complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
catch (Exception exception)
{
    MessageBox.Show(exception.Message, "Uninstall failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    Environment.ExitCode = 1;
}

static void DeleteShortcut(string path)
{
    if (File.Exists(path)) File.Delete(path);
}
