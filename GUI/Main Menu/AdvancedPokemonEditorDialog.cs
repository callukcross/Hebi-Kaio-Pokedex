using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Rules;
using HebiKaio.Core.Trainer;

namespace GUI;

public sealed class AdvancedPokemonEditorDialog : Form
{
    private readonly IReadOnlyList<PokemonSpecies> _speciesData;
    private readonly ReferenceRulesCatalog _rules;
    private readonly TrainerRulesCatalog _trainerRules;
    private readonly OwnedPokemon _existing;
    private readonly ComboBox _species = new() { DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
    private readonly TextBox _nickname = new();
    private readonly NumericUpDown _level = new() { Minimum = 1, Maximum = 20, Value = 1 };
    private readonly ComboBox _gender = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _shiny = new() { Text = "Shiny Pokémon", AutoSize = true };
    private readonly ComboBox _nature = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _item = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly TextBox _form = new();
    private readonly TextBox _image = new() { ReadOnly = true };
    private readonly CheckedListBox _abilities = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly CheckedListBox _moves = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly CheckedListBox _feats = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly CheckedListBox _skills = new() { Dock = DockStyle.Fill, CheckOnClick = true };
    private readonly Dictionary<string, NumericUpDown> _attributes = new(StringComparer.OrdinalIgnoreCase);

    public PokemonDraft Result { get; private set; }

    public AdvancedPokemonEditorDialog(IPokemonCatalog catalog, ReferenceRulesCatalog rules, TrainerRulesCatalog trainerRules, OwnedPokemon existing = null)
    {
        _speciesData = catalog.GetAll(); _rules = rules; _trainerRules = trainerRules; _existing = existing;
        Text = existing is null ? "Add Pokémon" : "Edit Pokémon";
        ClientSize = new Size(860, 720); MinimumSize = new Size(760, 620); StartPosition = FormStartPosition.CenterParent;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildGeneral()); tabs.TabPages.Add(BuildAttributes());
        tabs.TabPages.Add(BuildSelection("Moves", _moves)); tabs.TabPages.Add(BuildSelection("Abilities", _abilities));
        tabs.TabPages.Add(BuildSelection("Feats", _feats)); tabs.TabPages.Add(BuildSelection("Skills", _skills));
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
        var cancel = AppTheme.Button("Cancel", (_, _) => { DialogResult = DialogResult.Cancel; Close(); });
        var save = AppTheme.Button("Save Pokémon", (_, _) => Save()); footer.Controls.AddRange([cancel, save]);
        Controls.Add(tabs); Controls.Add(footer);
        foreach (var value in Enum.GetValues<PokemonGender>()) _gender.Items.Add(value);
        foreach (var value in _rules.Natures.Keys.OrderBy(value => value)) _nature.Items.Add(value);
        _item.Items.Add("None"); foreach (var value in _trainerRules.Items.Keys.OrderBy(value => value)) _item.Items.Add(value);
        foreach (var value in _trainerRules.Feats.Keys.OrderBy(value => value)) _feats.Items.Add(value);
        var allSkills = _rules.Pokemon.SelectMany(value => value.Skills).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value); foreach (var value in allSkills) _skills.Items.Add(value);
        foreach (var value in _speciesData) _species.Items.Add(new SpeciesOption(value));
        _species.SelectedIndexChanged += (_, _) => RefreshSpeciesChoices(); _level.ValueChanged += (_, _) => RefreshSpeciesChoices();
        PopulateExisting();
    }

    private TabPage BuildGeneral()
    {
        var page = new TabPage("Identity");
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20), AutoScroll = true };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddField(table, "Species", _species); AddField(table, "Nickname", _nickname); AddField(table, "Level", _level); AddField(table, "Gender", _gender); AddField(table, "Nature", _nature); AddField(table, "Held item", _item); AddField(table, "Variant / form", _form); AddField(table, "", _shiny);
        var imageRow = new FlowLayoutPanel { Dock = DockStyle.Fill }; _image.Width = 450; imageRow.Controls.Add(_image); imageRow.Controls.Add(AppTheme.Button("Browse...", (_, _) => BrowseImage())); AddField(table, "Custom artwork", imageRow);
        page.Controls.Add(table); return page;
    }

    private TabPage BuildAttributes()
    {
        var page = new TabPage("Ability Scores / ASI");
        var table = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, Padding = new Padding(24), AutoSize = true };
        table.Controls.Add(new Label { Text = "Ability", AutoSize = true }); table.Controls.Add(new Label { Text = "ASI increase", AutoSize = true }); table.Controls.Add(new Label { Text = "Custom modifier", AutoSize = true });
        foreach (var name in new[] { "STR", "DEX", "CON", "INT", "WIS", "CHA" })
        {
            table.RowCount++; table.Controls.Add(new Label { Text = name, AutoSize = true, Margin = new Padding(3, 10, 20, 3) });
            var increase = new NumericUpDown { Minimum = -20, Maximum = 20, Width = 90 }; var custom = new NumericUpDown { Minimum = -20, Maximum = 20, Width = 90 };
            _attributes[name + ":increase"] = increase; _attributes[name + ":custom"] = custom; table.Controls.Add(increase); table.Controls.Add(custom);
        }
        page.Controls.Add(table); return page;
    }

    private static TabPage BuildSelection(string title, CheckedListBox list)
    {
        var page = new TabPage(title); page.Controls.Add(list); page.Controls.Add(new Label { Dock = DockStyle.Top, Height = 36, Padding = new Padding(8), Text = $"Search and selection parity: choose {title.ToLowerInvariant()} by checking entries. Move choices update with species and level." }); return page;
    }

    private void PopulateExisting()
    {
        _species.SelectedItem = _species.Items.Cast<SpeciesOption>().FirstOrDefault(item => item.Species.Number == _existing?.SpeciesNumber) ?? _species.Items.Cast<SpeciesOption>().First();
        if (_existing is null) { _gender.SelectedItem = PokemonGender.Unspecified; _nature.SelectedItem = "Hardy"; _item.SelectedIndex = 0; return; }
        _nickname.Text = _existing.Nickname ?? ""; _level.Value = _existing.Level; _gender.SelectedItem = _existing.Gender; _shiny.Checked = _existing.IsShiny; _nature.SelectedItem = _existing.Nature; _item.Text = _existing.HeldItem ?? "None"; _form.Text = _existing.Form ?? ""; _image.Text = _existing.CustomImagePath ?? "";
        SetScores(_existing.AttributeIncreases, "increase"); SetScores(_existing.CustomAttributes, "custom");
        CheckValues(_feats, _existing.Feats); CheckValues(_skills, _existing.Skills); RefreshSpeciesChoices(); CheckValues(_abilities, _existing.Abilities); CheckValues(_moves, _existing.Moves.Select(move => move.Name));
    }

    private void RefreshSpeciesChoices()
    {
        if (_species.SelectedItem is not SpeciesOption option) return;
        var pokemon = _rules.FindPokemon(option.Species.Name); if (pokemon is null) return;
        var selectedAbilities = _abilities.CheckedItems.Cast<string>().Concat(_existing?.Abilities ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedMoves = _moves.CheckedItems.Cast<string>().Concat(_existing?.Moves.Select(move => move.Name) ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _abilities.Items.Clear(); foreach (var value in pokemon.Abilities.Concat(pokemon.HiddenAbility is null ? [] : [pokemon.HiddenAbility]).Distinct()) _abilities.Items.Add(value, selectedAbilities.Contains(value));
        _moves.Items.Clear(); foreach (var value in PokemonRulesService.AvailableMoves(pokemon, (int)_level.Value).Concat(pokemon.EggMoves).Distinct().OrderBy(value => value)) _moves.Items.Add(value, selectedMoves.Contains(value));
    }

    private void Save()
    {
        if (_species.SelectedItem is not SpeciesOption option) { MessageBox.Show(this, "Choose a species."); return; }
        Result = new PokemonDraft { SpeciesNumber = option.Species.Number, SpeciesName = option.Species.Name, Nickname = _nickname.Text, Level = (int)_level.Value, Gender = (PokemonGender)_gender.SelectedItem, IsShiny = _shiny.Checked, Nature = _nature.Text, HeldItem = _item.Text == "None" ? "" : _item.Text, Form = _form.Text, CustomImagePath = _image.Text, AttributeIncreases = Scores("increase"), CustomAttributes = Scores("custom"), Abilities = _abilities.CheckedItems.Cast<string>().ToList(), Moves = _moves.CheckedItems.Cast<string>().ToList(), Feats = _feats.CheckedItems.Cast<string>().ToList(), Skills = _skills.CheckedItems.Cast<string>().ToList() };
        DialogResult = DialogResult.OK; Close();
    }

    private AbilityScores Scores(string suffix) => new() { Strength = Value("STR", suffix), Dexterity = Value("DEX", suffix), Constitution = Value("CON", suffix), Intelligence = Value("INT", suffix), Wisdom = Value("WIS", suffix), Charisma = Value("CHA", suffix) };
    private int Value(string name, string suffix) => (int)_attributes[name + ":" + suffix].Value;
    private void SetScores(AbilityScores scores, string suffix) { _attributes["STR:" + suffix].Value = scores.Strength; _attributes["DEX:" + suffix].Value = scores.Dexterity; _attributes["CON:" + suffix].Value = scores.Constitution; _attributes["INT:" + suffix].Value = scores.Intelligence; _attributes["WIS:" + suffix].Value = scores.Wisdom; _attributes["CHA:" + suffix].Value = scores.Charisma; }
    private static void CheckValues(CheckedListBox list, IEnumerable<string> values) { var set = values.ToHashSet(StringComparer.OrdinalIgnoreCase); for (var i = 0; i < list.Items.Count; i++) list.SetItemChecked(i, set.Contains(list.Items[i].ToString())); }
    private void BrowseImage() { using var dialog = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.webp;*.bmp" }; if (dialog.ShowDialog(this) == DialogResult.OK) _image.Text = dialog.FileName; }
    private static void AddField(TableLayoutPanel table, string label, Control control) { table.RowCount++; table.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 8, 3, 8) }); control.Dock = DockStyle.Top; table.Controls.Add(control); }
    private sealed record SpeciesOption(PokemonSpecies Species) { public override string ToString() => $"#{Species.Number:000} {Species.Name}"; }
}
