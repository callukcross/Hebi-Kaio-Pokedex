using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Rules;

namespace GUI;

public sealed class EncounterGeneratorForm : Form
{
    private readonly ProfileService _profiles;
    private readonly IPokemonCatalog _catalog;
    private readonly ReferenceRulesCatalog _rules;
    private readonly NumericUpDown _level = new() { Minimum = 1, Maximum = 20, Value = 1 };
    private readonly NumericUpDown _minimumSr = new() { Minimum = 0, Maximum = 30, DecimalPlaces = 1, Increment = .5M };
    private readonly NumericUpDown _maximumSr = new() { Minimum = 0, Maximum = 30, DecimalPlaces = 1, Increment = .5M, Value = 30 };
    private readonly ComboBox _generation = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _habitat = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _trainerClass = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly PictureBox _art = new() { SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill };
    private readonly Label _result = new() { Dock = DockStyle.Bottom, Height = 90, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(SystemFonts.DefaultFont.FontFamily, 16, FontStyle.Bold) };
    private PokemonSpecies _selected;

    public EncounterGeneratorForm(ProfileService profiles, IPokemonCatalog catalog, ReferenceRulesCatalog rules)
    {
        _profiles = profiles; _catalog = catalog; _rules = rules;
        Text = "Generate Random Pokémon";
        var filters = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 280, FlowDirection = FlowDirection.TopDown, Padding = new Padding(18) };
        filters.Controls.AddRange([Label("Pokémon level"), _level, Label("Minimum species rating"), _minimumSr, Label("Maximum species rating"), _maximumSr, Label("Generation"), _generation, Label("Habitat"), _habitat, Label("Type"), _type, Label("Trainer class"), _trainerClass]);
        _generation.Items.Add("Any"); for (var i = 1; i <= 7; i++) _generation.Items.Add($"Generation {i}"); _generation.SelectedIndex = 0;
        _type.Items.Add("Any"); foreach (var type in catalog.GetAll().SelectMany(item => item.Types).Distinct().OrderBy(item => item)) _type.Items.Add(type); _type.SelectedIndex = 0;
        _habitat.Items.Add("Any"); foreach (var habitat in rules.Habitats.Keys.OrderBy(item => item)) _habitat.Items.Add(habitat); _habitat.SelectedIndex = 0;
        _trainerClass.Items.Add("Any"); foreach (var trainerClass in rules.TrainerClasses.Keys.OrderBy(item => item)) _trainerClass.Items.Add(trainerClass); _trainerClass.SelectedIndex = 0;
        filters.Controls.Add(AppTheme.Button("Randomize", (_, _) => Randomize()));
        filters.Controls.Add(AppTheme.Button("Add to Storage", (_, _) => Add()));
        filters.Controls.Add(AppTheme.Button("Reset", (_, _) => Reset()));
        filters.Controls.Add(AppTheme.Button("Close", (_, _) => Close()));
        var preview = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) }; preview.Controls.Add(_art); preview.Controls.Add(_result);
        Controls.Add(preview); Controls.Add(filters);
        Randomize();
    }

    private void Randomize()
    {
        var matches = _catalog.GetAll().Where(item => item.SpeciesRating >= (double)_minimumSr.Value && item.SpeciesRating <= (double)_maximumSr.Value && item.MinimumWildLevel <= _level.Value);
        if (_generation.SelectedIndex > 0) matches = matches.Where(item => Generation(item.Number) == _generation.SelectedIndex);
        if (_type.SelectedIndex > 0) matches = matches.Where(item => item.Types.Contains(_type.Text, StringComparer.OrdinalIgnoreCase));
        if (_habitat.SelectedIndex > 0) { var names = _rules.Habitats[_habitat.Text].ToHashSet(StringComparer.OrdinalIgnoreCase); matches = matches.Where(item => names.Contains(item.Name)); }
        if (_trainerClass.SelectedIndex > 0) { var names = _rules.TrainerClasses[_trainerClass.Text].ToHashSet(StringComparer.OrdinalIgnoreCase); matches = matches.Where(item => names.Contains(item.Name)); }
        var list = matches.ToList();
        if (list.Count == 0) { _selected = null; _result.Text = "No Pokémon match these filters."; _art.Image = null; return; }
        _selected = list[Random.Shared.Next(list.Count)];
        _result.Text = $"#{_selected.Number:000} {_selected.Name}\nLevel {(int)_level.Value} • SR {_selected.SpeciesRating:0.#}";
        _art.Image?.Dispose(); _art.Image = null;
        var path = PokemonArtworkLocator.Find(_selected.Name); if (path is not null) { using var source = Image.FromFile(path); _art.Image = new Bitmap(source); }
    }

    private void Add() { if (_selected is null) return; _profiles.CreatePokemon(new PokemonDraft { SpeciesNumber = _selected.Number, SpeciesName = _selected.Name, Level = (int)_level.Value }, _rules); MessageBox.Show(this, $"{_selected.Name} was added to storage.", "Pokémon added"); }
    private void Reset() { _level.Value = 1; _minimumSr.Value = 0; _maximumSr.Value = 30; _generation.SelectedIndex = 0; _habitat.SelectedIndex = 0; _type.SelectedIndex = 0; _trainerClass.SelectedIndex = 0; Randomize(); }
    private static Label Label(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(0, 10, 0, 2) };
    private static int Generation(int number) => number switch { <= 151 => 1, <= 251 => 2, <= 386 => 3, <= 493 => 4, <= 649 => 5, <= 721 => 6, _ => 7 };
}
