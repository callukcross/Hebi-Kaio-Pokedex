using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32;

const string productName = "HebiKaio Pokedex";
var installDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", productName);

try
{
    var answer = MessageBox.Show($"Install {productName} for your Windows account?\n\nLocation: {installDirectory}", productName + " Setup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
    if (answer != DialogResult.OK) return;

    Directory.CreateDirectory(installDirectory);
    var assembly = typeof(Program).Assembly;
    using var payload = assembly.GetManifestResourceStream("HebiKaio.Payload.zip")
        ?? throw new InvalidOperationException("The installer payload is missing.");
    using var archive = new ZipArchive(payload, ZipArchiveMode.Read);
    archive.ExtractToDirectory(installDirectory, overwriteFiles: true);

    var applicationPath = Path.Combine(installDirectory, "HebiKaioPokedex.exe");
    var uninstallerPath = Path.Combine(installDirectory, "Uninstall-HebiKaio-Pokedex.exe");
    if (!File.Exists(applicationPath) || !File.Exists(uninstallerPath))
        throw new InvalidDataException("The installed application files are incomplete.");

    var startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), productName + ".lnk");
    CreateShortcut(startMenu, applicationPath, installDirectory);
    if (MessageBox.Show("Create a Desktop shortcut?", productName + " Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
    {
        var desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), productName + ".lnk");
        CreateShortcut(desktop, applicationPath, installDirectory);
    }

    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\HebiKaioPokedex"))
    {
        key.SetValue("DisplayName", productName);
        key.SetValue("DisplayVersion", "0.1.0");
        key.SetValue("Publisher", "HebiKaio");
        key.SetValue("InstallLocation", installDirectory);
        key.SetValue("DisplayIcon", applicationPath);
        key.SetValue("UninstallString", $"\"{uninstallerPath}\"");
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    if (MessageBox.Show("Installation completed. Launch HebiKaio Pokedex now?", productName + " Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
        Process.Start(new ProcessStartInfo(applicationPath) { WorkingDirectory = installDirectory, UseShellExecute = true });
}
catch (Exception exception)
{
    MessageBox.Show(exception.Message, productName + " Setup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    Environment.ExitCode = 1;
}

static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory)
{
    var shellType = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("Windows shortcut support is unavailable.");
    dynamic shell = Activator.CreateInstance(shellType)!;
    dynamic shortcut = shell.CreateShortcut(shortcutPath);
    shortcut.TargetPath = targetPath;
    shortcut.WorkingDirectory = workingDirectory;
    shortcut.Description = "Launch HebiKaio Pokedex";
    shortcut.Save();
}
