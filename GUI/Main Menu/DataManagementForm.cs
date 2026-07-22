using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using HebiKaio.Core.Content;
using HebiKaio.Core.Profiles;

namespace GUI;

public sealed class DataManagementForm : Form
{
    private readonly ProfileTransferService _transfers;
    private readonly CustomContentService _modules;
    private readonly Action _profileChanged;
    private readonly ListBox _moduleList = new() { Dock = DockStyle.Fill };

    public DataManagementForm(ProfileTransferService transfers, CustomContentService modules, Action profileChanged)
    {
        _transfers = transfers;
        _modules = modules;
        _profileChanged = profileChanged;
        Text = "Import, Export & Custom Content";
        Width = 700;
        Height = 500;

        var profileButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8) };
        profileButtons.Controls.Add(Button("Export Active Profile", ExportProfile));
        profileButtons.Controls.Add(Button("Import Profile", ImportProfile));
        profileButtons.Controls.Add(Button("Export Pokémon", ExportPokemon));
        profileButtons.Controls.Add(Button("Import Pokémon", ImportPokemon));

        var moduleButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(8) };
        moduleButtons.Controls.Add(Button("Install Module", InstallModule));
        moduleButtons.Controls.Add(Button("Remove Selected", RemoveModule));
        moduleButtons.Controls.Add(Button("Close", (_, _) => Close()));

        Controls.Add(_moduleList);
        Controls.Add(new Label { Dock = DockStyle.Top, Height = 30, Padding = new Padding(8, 6, 0, 0), Text = "Installed custom content modules" });
        Controls.Add(profileButtons);
        Controls.Add(moduleButtons);
        RefreshModules();
    }

    private static Button Button(string text, EventHandler handler)
    {
        var button = new Button { AutoSize = true, Text = text };
        button.Click += handler;
        return button;
    }

    private void ExportProfile(object sender, EventArgs args)
    {
        using var dialog = new SaveFileDialog { Filter = "HebiKaio profile (*.hkp)|*.hkp", DefaultExt = "hkp", AddExtension = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run(() => _transfers.ExportActiveProfile(dialog.FileName), "Profile exported successfully.");
    }

    private void ImportProfile(object sender, EventArgs args)
    {
        using var dialog = new OpenFileDialog { Filter = "HebiKaio profile (*.hkp)|*.hkp|JSON files (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run(() =>
        {
            var profile = _transfers.ImportProfile(dialog.FileName);
            _profileChanged();
            MessageBox.Show(this, $"Imported '{profile.Name}' and made it active.", "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private void InstallModule(object sender, EventArgs args)
    {
        using var dialog = new OpenFileDialog { Filter = "HebiKaio module (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run(() =>
        {
            var module = _modules.Install(dialog.FileName);
            RefreshModules();
            MessageBox.Show(this, $"Installed '{module.Name}'.", "Module installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private void ExportPokemon(object sender, EventArgs args)
    {
        var pokemon = _transfers.GetExportablePokemon();
        if (pokemon.Count == 0) { MessageBox.Show(this, "The active profile has no Pokémon to export."); return; }
        using var picker = new Form { Text = "Choose Pokémon", ClientSize = new Size(420, 150), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
        var choice = new ComboBox { Left = 20, Top = 30, Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var item in pokemon) choice.Items.Add(new PokemonItem(item)); choice.SelectedIndex = 0;
        picker.Controls.AddRange([choice, new Button { Text = "Export", Left = 220, Top = 90, Width = 85, DialogResult = DialogResult.OK }, new Button { Text = "Cancel", Left = 315, Top = 90, Width = 85, DialogResult = DialogResult.Cancel }]);
        if (picker.ShowDialog(this) != DialogResult.OK || choice.SelectedItem is not PokemonItem selected) return;
        using var dialog = new SaveFileDialog { Filter = "HebiKaio Pokémon (*.hkpokemon)|*.hkpokemon", AddExtension = true, DefaultExt = "hkpokemon", FileName = selected.Pokemon.Nickname ?? selected.Pokemon.SpeciesName };
        if (dialog.ShowDialog(this) == DialogResult.OK) Run(() => _transfers.ExportPokemon(selected.Pokemon.Id, dialog.FileName), "Pokémon exported successfully.");
    }

    private void ImportPokemon(object sender, EventArgs args)
    {
        using var dialog = new OpenFileDialog { Filter = "HebiKaio Pokémon (*.hkpokemon)|*.hkpokemon|JSON files (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        Run(() => { var pokemon = _transfers.ImportPokemon(dialog.FileName); MessageBox.Show(this, $"Imported {pokemon.Nickname ?? pokemon.SpeciesName} into storage.", "Import complete"); });
    }

    private void RemoveModule(object sender, EventArgs args)
    {
        if (_moduleList.SelectedItem is not ModuleListItem selected) return;
        if (MessageBox.Show(this, $"Remove '{selected.Summary.Name}'?", "Remove module", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Run(() => { _modules.Remove(selected.Summary.Id); RefreshModules(); });
    }

    private void RefreshModules()
    {
        _moduleList.Items.Clear();
        foreach (var module in _modules.GetInstalledModules())
            _moduleList.Items.Add(new ModuleListItem(module));
        if (_moduleList.Items.Count == 0)
            _moduleList.Items.Add("No custom modules installed.");
    }

    private void Run(Action action, string success = null)
    {
        try
        {
            action();
            if (success is not null)
                MessageBox.Show(this, success, "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or InvalidOperationException or ArgumentException)
        {
            MessageBox.Show(this, exception.Message, "Unable to complete operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed record ModuleListItem(ContentModuleSummary Summary)
    {
        public override string ToString() => $"{Summary.Name} ({Summary.Id}) — {Summary.FeatCount} feats, {Summary.ItemCount} items";
    }

    private sealed record PokemonItem(OwnedPokemon Pokemon)
    {
        public override string ToString() => $"{Pokemon.Nickname ?? Pokemon.SpeciesName} — {Pokemon.SpeciesName} Lv. {Pokemon.Level}";
    }
}
