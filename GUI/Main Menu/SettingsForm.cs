using System;
using System.Windows.Forms;
using HebiKaio.Core.Content;

namespace GUI;

public sealed class SettingsForm : Form
{
    private readonly CustomContentService _modules;
    private readonly CheckBox _strictGender = new() { Text = "Enforce species gender rules", AutoSize = true };
    private readonly ListBox _moduleList = new() { Dock = DockStyle.Fill };

    public SettingsForm(string moduleDirectory)
    {
        _modules = new CustomContentService(moduleDirectory);
        Text = "Settings";
        var settings = DesktopSettingsService.Load(); _strictGender.Checked = settings.StrictGender;
        _strictGender.CheckedChanged += (_, _) => DesktopSettingsService.Save(new DesktopSettings { StrictGender = _strictGender.Checked });
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(12) }; top.Controls.Add(_strictGender);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
        actions.Controls.AddRange([AppTheme.Button("Install Content Module", (_, _) => Install()), AppTheme.Button("Remove Selected", (_, _) => Remove()), AppTheme.Button("Close", (_, _) => Close())]);
        Controls.Add(_moduleList); Controls.Add(top); Controls.Add(actions); RefreshModules();
    }

    private void RefreshModules() { _moduleList.Items.Clear(); foreach (var module in _modules.GetInstalledModules()) _moduleList.Items.Add(new ModuleItem(module)); if (_moduleList.Items.Count == 0) _moduleList.Items.Add("No custom modules installed."); }
    private void Install() { using var dialog = new OpenFileDialog { Filter = "HebiKaio module (*.json)|*.json" }; if (dialog.ShowDialog(this) != DialogResult.OK) return; try { _modules.Install(dialog.FileName); RefreshModules(); } catch (Exception exception) { MessageBox.Show(this, exception.Message, "Invalid module", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void Remove() { if (_moduleList.SelectedItem is not ModuleItem item) return; _modules.Remove(item.Module.Id); RefreshModules(); }
    private sealed record ModuleItem(ContentModuleSummary Module) { public override string ToString() => $"{Module.Name} — {Module.FeatCount} feats, {Module.ItemCount} items"; }
}
