using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Win32;

namespace HebiKaio.Installer;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length == 2 && args[0] == "--install-test")
        {
            InstallerForm.Install(Path.GetFullPath(args[1]), false, false, new Progress<(int Percent, string Status)>(), registerWithWindows: false); return;
        }
        Application.Run(new InstallerForm());
    }
}

internal sealed class InstallerForm : Form
{
    private const string AppName = "HebiKaio Pokedex";
    private readonly string _defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", AppName);
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(28) };
    private readonly Button _back = new() { Text = "< Back", Width = 90 };
    private readonly Button _next = new() { Text = "Next >", Width = 90 };
    private readonly Button _cancel = new() { Text = "Cancel", Width = 90 };
    private readonly TextBox _installPath = new() { Dock = DockStyle.Top };
    private readonly CheckBox _desktopShortcut = new() { Text = "Create a Desktop shortcut", AutoSize = true, Checked = true };
    private readonly CheckBox _startMenuShortcut = new() { Text = "Create a Start Menu shortcut", AutoSize = true, Checked = true };
    private readonly ProgressBar _progress = new() { Dock = DockStyle.Top, Height = 25 };
    private readonly Label _progressLabel = new() { Dock = DockStyle.Top, Height = 35, Text = "Preparing installation..." };
    private int _page;
    private bool _installing;

    public InstallerForm()
    {
        Text = AppName + " Setup";
        ClientSize = new Size(650, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        _installPath.Text = _defaultDirectory;

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
        footer.Controls.AddRange([_cancel, _next, _back]);
        Controls.Add(_content);
        Controls.Add(footer);
        _back.Click += (_, _) => { _page--; ShowPage(); };
        _next.Click += NextClicked;
        _cancel.Click += (_, _) => Close();
        FormClosing += (_, args) => { if (_installing) args.Cancel = true; };
        ShowPage();
    }

    private void ShowPage()
    {
        _content.Controls.Clear();
        _back.Enabled = _page > 0 && _page < 3;
        _cancel.Enabled = _page < 3;
        _next.Enabled = true;
        _next.Text = _page switch { 2 => "Install", 3 => "Finish", _ => "Next >" };

        switch (_page)
        {
            case 0:
                AddText("This wizard installs HebiKaio Pokedex for your Windows account. Administrator privileges are not required.\n\nClose the application before continuing.");
                AddHeading("Welcome to HebiKaio Pokedex Setup");
                break;
            case 1:
                ShowLocationPage();
                break;
            case 2:
                AddOptions();
                AddText($"The application will be installed to:\n{_installPath.Text}\n\nClick Install to begin.");
                AddHeading("Ready to install");
                break;
            case 3:
                ShowProgressPage();
                break;
            case 4:
                ShowCompletionPage();
                break;
        }
    }

    private void ShowLocationPage()
    {
        var browse = new Button { Text = "Browse...", Dock = DockStyle.Top, Height = 30 };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog { Description = "Choose a writable folder for HebiKaio Pokedex", SelectedPath = _installPath.Text, UseDescriptionForTitle = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) _installPath.Text = dialog.SelectedPath;
        };
        _content.Controls.Add(browse);
        _content.Controls.Add(_installPath);
        AddText("Setup runs without administrator privileges. Choose a folder your Windows account can write to. Protected locations such as Program Files are not supported by this installer.");
        AddHeading("Choose an installation folder");
    }

    private void AddOptions()
    {
        var options = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 65, FlowDirection = FlowDirection.TopDown };
        options.Controls.Add(_desktopShortcut);
        options.Controls.Add(_startMenuShortcut);
        _content.Controls.Add(options);
    }

    private void ShowProgressPage()
    {
        _back.Enabled = false;
        _next.Enabled = false;
        _cancel.Enabled = false;
        _content.Controls.Add(_progress);
        _content.Controls.Add(_progressLabel);
        AddHeading("Installing HebiKaio Pokedex");
    }

    private void ShowCompletionPage()
    {
        _back.Enabled = false;
        _cancel.Enabled = false;
        _next.Text = "Finish";
        var launch = new CheckBox { Name = "launch", Text = "Launch HebiKaio Pokedex", AutoSize = true, Checked = true, Dock = DockStyle.Top };
        _content.Controls.Add(launch);
        AddText("HebiKaio Pokedex has been installed successfully.");
        AddHeading("Installation complete");
    }

    private async void NextClicked(object? sender, EventArgs args)
    {
        if (_page == 1 && !TryValidateInstallPath(out var error))
        {
            MessageBox.Show(this, error, "Choose another folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_page == 2)
        {
            _page = 3;
            _installing = true;
            ShowPage();
            try
            {
                var progress = new Progress<(int Percent, string Status)>(update => { _progress.Value = update.Percent; _progressLabel.Text = update.Status; });
                var destination = Path.GetFullPath(_installPath.Text);
                var desktopShortcut = _desktopShortcut.Checked;
                var startMenuShortcut = _startMenuShortcut.Checked;
                await Task.Run(() => Install(destination, desktopShortcut, startMenuShortcut, progress));
                _installing = false;
                _page = 4;
                ShowPage();
            }
            catch (Exception exception)
            {
                _installing = false;
                MessageBox.Show(this, exception.Message, "Installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _page = 2;
                ShowPage();
            }
            return;
        }
        if (_page == 4)
        {
            var launch = _content.Controls.Find("launch", true).OfType<CheckBox>().FirstOrDefault()?.Checked == true;
            if (launch)
                Process.Start(new ProcessStartInfo(Path.Combine(Path.GetFullPath(_installPath.Text), "HebiKaioPokedex.exe")) { UseShellExecute = true });
            Close();
            return;
        }
        _page++;
        ShowPage();
    }

    internal static void Install(string destination, bool desktopShortcut, bool startMenuShortcut, IProgress<(int Percent, string Status)> progress, bool registerWithWindows = true)
    {
        Directory.CreateDirectory(destination);
        using var payload = typeof(Program).Assembly.GetManifestResourceStream("HebiKaio.Payload.zip")
            ?? throw new InvalidOperationException("The installer payload is missing.");
        using var archive = new ZipArchive(payload, ZipArchiveMode.Read);
        var files = archive.Entries.Where(entry => !string.IsNullOrEmpty(entry.Name)).ToList();
        for (var index = 0; index < files.Count; index++)
        {
            var entry = files[index];
            var target = Path.GetFullPath(Path.Combine(destination, entry.FullName));
            if (!target.StartsWith(destination.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The installer payload contains an unsafe path.");
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
            progress.Report(((index + 1) * 85 / Math.Max(files.Count, 1), "Installing " + entry.Name));
        }
        File.WriteAllText(Path.Combine(destination, ".hebikaio-install"), "HebiKaio Pokedex 0.1.0");
        progress.Report((90, "Creating shortcuts..."));
        if (startMenuShortcut) CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName + ".lnk"), destination);
        if (desktopShortcut) CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), AppName + ".lnk"), destination);
        if (registerWithWindows) RegisterUninstaller(destination);
        progress.Report((100, "Installation complete."));
    }

    private bool TryValidateInstallPath(out string error)
    {
        try
        {
            var path = Path.GetFullPath(_installPath.Text.Trim());
            var root = Path.GetPathRoot(path);
            var protectedPaths = new[] { Environment.GetFolderPath(Environment.SpecialFolder.Windows), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) };
            if (string.Equals(path.TrimEnd('\\'), root?.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) || protectedPaths.Any(item => !string.IsNullOrEmpty(item) && (path.Equals(item, StringComparison.OrdinalIgnoreCase) || path.StartsWith(item.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase))))
                throw new InvalidOperationException("Choose a per-user folder, such as the default location under Local AppData.");
            Directory.CreateDirectory(path);
            var probe = Path.Combine(path, ".hebikaio-write-test-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(probe, "test");
            File.Delete(probe);
            _installPath.Text = path;
            error = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or InvalidOperationException)
        {
            error = "Setup cannot write to that folder without administrator privileges. " + exception.Message;
            return false;
        }
    }

    private static void RegisterUninstaller(string directory)
    {
        var app = Path.Combine(directory, "HebiKaioPokedex.exe");
        var uninstall = Path.Combine(directory, "Uninstall-HebiKaio-Pokedex.exe");
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\HebiKaioPokedex");
        key.SetValue("DisplayName", AppName);
        key.SetValue("DisplayVersion", "0.1.0");
        key.SetValue("Publisher", "HebiKaio");
        key.SetValue("InstallLocation", directory);
        key.SetValue("DisplayIcon", app);
        key.SetValue("UninstallString", $"\"{uninstall}\"");
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    private static void CreateShortcut(string shortcutPath, string directory)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("Windows shortcut support is unavailable.");
        dynamic shell = Activator.CreateInstance(shellType)!;
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = Path.Combine(directory, "HebiKaioPokedex.exe");
        shortcut.WorkingDirectory = directory;
        shortcut.Description = "Launch " + AppName;
        shortcut.Save();
    }

    private void AddHeading(string text) => _content.Controls.Add(new Label { Text = text, Dock = DockStyle.Top, Height = 55, Font = new Font(Font, FontStyle.Bold) });
    private void AddText(string text) => _content.Controls.Add(new Label { Text = text, Dock = DockStyle.Top, Height = 110 });
}
