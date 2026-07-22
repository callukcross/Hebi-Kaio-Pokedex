using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using HebiKaio.Core.Content;

namespace GUI;

public sealed class SettingsForm : Form
{
    private readonly string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HebiKaioPokedex", "settings.json");
    private readonly CustomContentService _modules;
    private readonly CheckBox _strictGender = new() { Text = "Enforce species gender rules", AutoSize = true };
    private readonly ListBox _moduleList = new() { Dock = DockStyle.Fill };

    public SettingsForm(string moduleDirectory)
    {
        _modules = new CustomContentService(moduleDirectory);
        Text = "Settings";
        var settings = LoadSettings(); _strictGender.Checked = settings.StrictGender;
        _strictGender.CheckedChanged += (_, _) => Save(new DesktopSettings { StrictGender = _strictGender.Checked });
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(12) }; top.Controls.Add(_strictGender);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
        actions.Controls.AddRange([AppTheme.Button("Install Content Module", (_, _) => Install()), AppTheme.Button("Remove Selected", (_, _) => Remove()), AppTheme.Button("Close", (_, _) => Close())]);
        Controls.Add(_moduleList); Controls.Add(top); Controls.Add(actions); RefreshModules();
    }

    private void RefreshModules() { _moduleList.Items.Clear(); foreach (var module in _modules.GetInstalledModules()) _moduleList.Items.Add(new ModuleItem(module)); if (_moduleList.Items.Count == 0) _moduleList.Items.Add("No custom modules installed."); }
    private void Install() { using var dialog = new OpenFileDialog { Filter = "HebiKaio module (*.json)|*.json" }; if (dialog.ShowDialog(this) != DialogResult.OK) return; try { _modules.Install(dialog.FileName); RefreshModules(); } catch (Exception exception) { MessageBox.Show(this, exception.Message, "Invalid module", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void Remove() { if (_moduleList.SelectedItem is not ModuleItem item) return; _modules.Remove(item.Module.Id); RefreshModules(); }
    private DesktopSettings LoadSettings() { try { return File.Exists(_settingsPath) ? JsonSerializer.Deserialize<DesktopSettings>(File.ReadAllText(_settingsPath)) ?? new() : new(); } catch { return new(); } }
    private void Save(DesktopSettings settings) { Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)); File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true })); }
    private sealed record ModuleItem(ContentModuleSummary Module) { public override string ToString() => $"{Module.Name} — {Module.FeatCount} feats, {Module.ItemCount} items"; }
    private sealed class DesktopSettings { public bool StrictGender { get; set; } }
}
