using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;

namespace GUI
{
    public sealed class PokedexForm : Form
    {
        private readonly ProfileService _profiles;
        private readonly IReadOnlyList<PokemonSpecies> _allSpecies;
        private readonly TextBox _search = new TextBox();
        private readonly NumericUpDown _number = new NumericUpDown();
        private readonly ComboBox _type = new ComboBox();
        private readonly ComboBox _region = new ComboBox();
        private readonly ComboBox _stage = new ComboBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly PictureBox _image = new PictureBox();
        private readonly Label _name = new Label();
        private readonly Label _details = new Label();
        private readonly Label _resultCount = new Label();
        private PokemonSpecies _selected;

        public PokedexForm(ProfileService profiles, IPokemonCatalog catalog)
        {
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _allSpecies = catalog?.GetAll() ?? throw new ArgumentNullException(nameof(catalog));
            InitializeUi();
            PopulateFilters();
            ApplyFilters();
        }

        private void InitializeUi()
        {
            Text = "HebiKaio Pokédex";
            BackColor = Color.FromArgb(245, 247, 250);
            MinimumSize = new Size(900, 620);

            var header = new Panel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(12), BackColor = Color.FromArgb(187, 31, 46) };
            var back = new Button { Text = "← Menu", Width = 90, Height = 30, Location = new Point(12, 12) };
            back.Click += (sender, args) => Close();
            header.Controls.Add(back);

            _search.PlaceholderText = "Search species";
            _search.SetBounds(112, 12, 180, 30);
            _search.TextChanged += (sender, args) => ApplyFilters();
            header.Controls.Add(_search);

            _number.Minimum = 0;
            _number.Maximum = 809;
            _number.Width = 80;
            _number.Location = new Point(302, 12);
            _number.ValueChanged += (sender, args) => ApplyFilters();
            header.Controls.Add(_number);

            ConfigureFilter(_type, 392, header);
            ConfigureFilter(_region, 522, header);
            ConfigureFilter(_stage, 652, header);

            _resultCount.AutoSize = true;
            _resultCount.ForeColor = Color.White;
            _resultCount.Location = new Point(112, 50);
            header.Controls.Add(_resultCount);
            Controls.Add(header);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 570, FixedPanel = FixedPanel.Panel2 };
            Controls.Add(split);
            header.BringToFront();

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;
            _grid.BackgroundColor = Color.White;
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "Status", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Number", HeaderText = "#", Width = 55 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", Width = 145 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Types", HeaderText = "Types", Width = 130 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Region", HeaderText = "Region", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stage", HeaderText = "Evolution", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _grid.SelectionChanged += (sender, args) => ShowSelectedSpecies();
            split.Panel1.Controls.Add(_grid);

            var detail = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18), BackColor = Color.White };
            split.Panel2.Controls.Add(detail);
            _image.Size = new Size(220, 220);
            _image.Location = new Point(25, 20);
            _image.SizeMode = PictureBoxSizeMode.Zoom;
            detail.Controls.Add(_image);
            _name.SetBounds(18, 250, 260, 34);
            _name.Font = new Font(Font.FontFamily, 17, FontStyle.Bold);
            _name.TextAlign = ContentAlignment.MiddleCenter;
            detail.Controls.Add(_name);
            _details.SetBounds(25, 290, 240, 145);
            _details.Font = new Font(Font.FontFamily, 10);
            detail.Controls.Add(_details);

            var clear = CreateStateButton("Unknown", 25, PokedexEntryState.Unknown);
            var seen = CreateStateButton("Seen", 105, PokedexEntryState.Seen);
            var caught = CreateStateButton("Caught", 185, PokedexEntryState.Caught);
            detail.Controls.Add(clear);
            detail.Controls.Add(seen);
            detail.Controls.Add(caught);
        }

        private static void ConfigureFilter(ComboBox filter, int left, Control parent)
        {
            filter.DropDownStyle = ComboBoxStyle.DropDownList;
            filter.SetBounds(left, 12, 120, 30);
            parent.Controls.Add(filter);
        }

        private Button CreateStateButton(string text, int left, PokedexEntryState state)
        {
            var button = new Button { Text = text, Width = 75, Height = 34, Location = new Point(left, 445) };
            button.Click += (sender, args) => SetSelectedState(state);
            return button;
        }

        private void PopulateFilters()
        {
            _type.Items.Add("All types");
            _type.Items.AddRange(_allSpecies.SelectMany(item => item.Types).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).Cast<object>().ToArray());
            _region.Items.Add("All regions");
            _region.Items.AddRange(_allSpecies.Select(item => item.Region).Distinct().Cast<object>().ToArray());
            _stage.Items.AddRange(new object[] { "All stages", "Basic", "Stage 1", "Stage 2", "Standalone" });
            _type.SelectedIndex = _region.SelectedIndex = _stage.SelectedIndex = 0;
            _type.SelectedIndexChanged += (sender, args) => ApplyFilters();
            _region.SelectedIndexChanged += (sender, args) => ApplyFilters();
            _stage.SelectedIndexChanged += (sender, args) => ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_type.SelectedIndex < 0 || _region.SelectedIndex < 0 || _stage.SelectedIndex < 0)
                return;

            var filter = new PokedexFilter
            {
                SearchText = _search.Text,
                Number = _number.Value == 0 ? null : (int)_number.Value,
                Type = _type.SelectedIndex == 0 ? null : _type.SelectedItem.ToString(),
                Region = _region.SelectedIndex == 0 ? null : _region.SelectedItem.ToString(),
                EvolutionStage = _stage.SelectedIndex == 0 ? null : (EvolutionStage?)(_stage.SelectedIndex - 1)
            };
            var matches = filter.Apply(_allSpecies).ToList();
            var states = _profiles.GetPokedexStates();
            _grid.Rows.Clear();
            foreach (var species in matches)
            {
                var state = states.TryGetValue(species.Number, out var savedState)
                    ? savedState
                    : PokedexEntryState.Unknown;
                var index = _grid.Rows.Add(
                    FormatState(state),
                    species.Number.ToString("000"),
                    species.Name,
                    string.Join(" / ", species.Types),
                    species.Region,
                    FormatStage(species.EvolutionStage));
                _grid.Rows[index].Tag = species;
            }

            _resultCount.Text = $"{matches.Count} of {_allSpecies.Count} species";
            if (_grid.Rows.Count > 0)
                _grid.Rows[0].Selected = true;
            else
                ClearDetails();
        }

        private void ShowSelectedSpecies()
        {
            if (_grid.CurrentRow?.Tag is not PokemonSpecies species)
                return;

            _selected = species;
            _name.Text = $"#{species.Number:000} {species.Name}";
            var evolution = species.EvolvesInto.Count == 0 ? "—" : string.Join(", ", species.EvolvesInto);
            _details.Text = $"Types: {string.Join(" / ", species.Types)}\nRegion: {species.Region}\nEvolution: {FormatStage(species.EvolutionStage)}\nSpecies rating: {species.SpeciesRating:0.###}\nMinimum wild level: {species.MinimumWildLevel}\nEvolves into: {evolution}\nStatus: {FormatState(_profiles.GetPokedexState(species.Number))}";

            _image.Image?.Dispose();
            _image.Image = null;
            var imagePath = PokemonArtworkLocator.Find(species.Name);
            if (imagePath != null)
            {
                using var source = Image.FromFile(imagePath);
                _image.Image = new Bitmap(source);
            }
        }

        private void SetSelectedState(PokedexEntryState state)
        {
            if (_selected == null)
                return;

            _profiles.SetPokedexState(_selected.Number, state);
            if (_grid.CurrentRow != null)
                _grid.CurrentRow.Cells["State"].Value = FormatState(state);
            ShowSelectedSpecies();
        }

        private void ClearDetails()
        {
            _selected = null;
            _name.Text = "No matches";
            _details.Text = string.Empty;
            _image.Image?.Dispose();
            _image.Image = null;
        }

        private static string FormatState(PokedexEntryState state) => state switch
        {
            PokedexEntryState.Seen => "Seen",
            PokedexEntryState.Caught => "Caught",
            _ => "Unknown"
        };

        private static string FormatStage(EvolutionStage stage) => stage switch
        {
            EvolutionStage.Basic => "Basic",
            EvolutionStage.StageOne => "Stage 1",
            EvolutionStage.StageTwo => "Stage 2",
            _ => "Standalone"
        };

    }
}
