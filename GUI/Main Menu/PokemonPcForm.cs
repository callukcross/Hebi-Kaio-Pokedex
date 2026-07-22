using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Trainer;
using HebiKaio.Core.Rules;

namespace GUI
{
    public sealed class PokemonPcForm : Form
    {
        private const string PokemonDragFormat = "HebiKaio.OwnedPokemonId";
        private readonly ProfileService _profiles;
        private readonly IPokemonCatalog _catalog;
        private readonly TrainerRulesCatalog _trainerRules;
        private readonly ReferenceRulesCatalog _referenceRules;
        private readonly DataGridView _party = CreateGrid();
        private readonly DataGridView _storage = CreateGrid();
        private readonly Label _summary = new Label();
        private readonly PictureBox _preview = new PictureBox();
        private readonly Label _previewName = new Label();

        public PokemonPcForm(ProfileService profiles, IPokemonCatalog catalog, TrainerRulesCatalog trainerRules, ReferenceRulesCatalog referenceRules = null)
        {
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _trainerRules = trainerRules ?? throw new ArgumentNullException(nameof(trainerRules));
            _referenceRules = referenceRules;
            InitializeUi();
            RefreshPokemon();
        }

        private void InitializeUi()
        {
            Text = "Pokémon PC & Party";
            BackColor = Color.FromArgb(242, 245, 248);
            MinimumSize = new Size(880, 620);

            var header = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(12), BackColor = Color.FromArgb(40, 79, 120) };
            var back = new Button { Text = "← Menu", Location = new Point(12, 12), Size = new Size(90, 32) };
            back.Click += (sender, args) => Close();
            header.Controls.Add(back);
            _summary.AutoSize = true;
            _summary.ForeColor = Color.White;
            _summary.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            _summary.Location = new Point(125, 19);
            header.Controls.Add(_summary);
            Controls.Add(header);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(10), FlowDirection = FlowDirection.LeftToRight };
            actions.Controls.Add(CreateButton("Create Pokémon", (sender, args) => CreatePokemon()));
            actions.Controls.Add(CreateButton("Edit Selected", (sender, args) => EditSelected()));
            actions.Controls.Add(CreateButton("Add to Party", (sender, args) => AddSelectedToParty()));
            actions.Controls.Add(CreateButton("Remove from Party", (sender, args) => RemoveSelectedFromParty()));
            actions.Controls.Add(CreateButton("Delete Selected", (sender, args) => DeleteSelected()));
            Controls.Add(actions);

            var content = new Panel { Dock = DockStyle.Fill };
            Controls.Add(content);
            var previewPanel = new Panel { Dock = DockStyle.Right, Width = 220, Padding = new Padding(12), BackColor = Color.White };
            content.Controls.Add(previewPanel);
            _preview.Dock = DockStyle.Top;
            _preview.Height = 190;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            previewPanel.Controls.Add(_preview);
            _previewName.Dock = DockStyle.Top;
            _previewName.Height = 60;
            _previewName.TextAlign = ContentAlignment.MiddleCenter;
            _previewName.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            previewPanel.Controls.Add(_previewName);
            _preview.BringToFront();
            var trainerEffects = new Label { Dock = DockStyle.Bottom, Height = 160, Font = new Font(FontFamily.GenericMonospace, 8.5f) };
            var effects = _profiles.GetTrainerEffects(_trainerRules);
            trainerEffects.Text =
                $"TRAINER EFFECTS\n" +
                $"Attack   +{effects.PokemonAttackBonus}\n" +
                $"Damage   +{effects.PokemonDamageBonus}\n" +
                $"AC       +{effects.PokemonArmorClassBonus}\n" +
                $"Init.    +{effects.PokemonInitiativeBonus}\n" +
                $"HP/lvl   +{effects.PokemonHitPointsPerLevel}\n" +
                $"STAB     +{effects.StabBonus}\n" +
                $"Moves    +{effects.ExtraMoveSlots}";
            previewPanel.Controls.Add(trainerEffects);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 230 };
            content.Controls.Add(split);
            previewPanel.BringToFront();
            header.BringToFront();
            actions.BringToFront();

            split.Panel1.Padding = new Padding(10, 34, 10, 8);
            split.Panel2.Padding = new Padding(10, 34, 10, 8);
            split.Panel1.Controls.Add(new Label { Text = "ACTIVE PARTY — drag to reorder, or drop a stored Pokémon here", Dock = DockStyle.Top, Height = 28, Font = new Font(Font.FontFamily, 10, FontStyle.Bold) });
            split.Panel2.Controls.Add(new Label { Text = "POKÉMON STORAGE — drag a row into the party", Dock = DockStyle.Top, Height = 28, Font = new Font(Font.FontFamily, 10, FontStyle.Bold) });
            split.Panel1.Controls.Add(_party);
            split.Panel2.Controls.Add(_storage);
            _party.BringToFront();
            _storage.BringToFront();

            ConfigureDragSource(_party);
            ConfigureDragSource(_storage);
            _party.AllowDrop = true;
            _party.DragEnter += PartyDragEnter;
            _party.DragDrop += PartyDragDrop;
            _party.SelectionChanged += (sender, args) => ShowPreview(GetSelectedPokemon(_party));
            _storage.SelectionChanged += (sender, args) => ShowPreview(GetSelectedPokemon(_storage));
        }

        private static DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Slot", HeaderText = "Slot", Width = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", Width = 180 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Level", HeaderText = "Level", Width = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Form", HeaderText = "Form", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            return grid;
        }

        private static Button CreateButton(string text, EventHandler handler)
        {
            var button = new Button { Text = text, AutoSize = true, Height = 32 };
            button.Click += handler;
            return button;
        }

        private void RefreshPokemon(Guid? selectId = null)
        {
            var party = _profiles.GetPartyPokemon();
            var storage = _profiles.GetOwnedPokemon();
            FillGrid(_party, party.ToArray(), includeSlots: true);
            FillGrid(_storage, storage.ToArray(), includeSlots: false);
            _summary.Text = $"Party {party.Count}/{ProfileService.MaximumPartySize}     Storage {storage.Count}";
            if (selectId is { } id)
                SelectPokemon(_storage, id);
        }

        private static void FillGrid(DataGridView grid, OwnedPokemon[] pokemon, bool includeSlots)
        {
            grid.Rows.Clear();
            for (var index = 0; index < pokemon.Length; index++)
            {
                var item = pokemon[index];
                var row = grid.Rows.Add(
                    includeSlots ? (index + 1).ToString() : "—",
                    string.IsNullOrWhiteSpace(item.Nickname) ? item.SpeciesName : item.Nickname,
                    $"#{item.SpeciesNumber:000} {item.SpeciesName}",
                    item.Level,
                    item.Form ?? "—");
                grid.Rows[row].Tag = item;
            }
        }

        private void CreatePokemon()
        {
            using Form dialog = _referenceRules is null ? (Form)new PokemonEditorDialog(_catalog) : new AdvancedPokemonEditorDialog(_catalog, _referenceRules, _trainerRules);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            var draft = dialog is AdvancedPokemonEditorDialog advanced ? advanced.Result : ((PokemonEditorDialog)dialog).Result;
            var created = _referenceRules is null ? _profiles.CreatePokemon(draft) : _profiles.CreatePokemon(draft, _referenceRules);
            RefreshPokemon(created.Id);
        }

        private void EditSelected()
        {
            var selected = GetSelectedPokemon(_storage) ?? GetSelectedPokemon(_party);
            if (selected == null)
                return;

            using Form dialog = _referenceRules is null ? (Form)new PokemonEditorDialog(_catalog, selected) : new AdvancedPokemonEditorDialog(_catalog, _referenceRules, _trainerRules, selected);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            var draft = dialog is AdvancedPokemonEditorDialog advanced ? advanced.Result : ((PokemonEditorDialog)dialog).Result;
            if (_referenceRules is null) _profiles.UpdatePokemon(selected.Id, draft); else _profiles.UpdatePokemon(selected.Id, draft, _referenceRules);
            RefreshPokemon(selected.Id);
        }

        private void AddSelectedToParty()
        {
            var selected = GetSelectedPokemon(_storage);
            if (selected == null)
                return;
            TryPartyChange(() => _profiles.AddToParty(selected.Id));
        }

        private void RemoveSelectedFromParty()
        {
            var selected = GetSelectedPokemon(_party);
            if (selected == null)
                return;
            _profiles.RemoveFromParty(selected.Id);
            RefreshPokemon(selected.Id);
        }

        private void DeleteSelected()
        {
            var selected = GetSelectedPokemon(_storage) ?? GetSelectedPokemon(_party);
            if (selected == null)
                return;
            var displayName = string.IsNullOrWhiteSpace(selected.Nickname) ? selected.SpeciesName : selected.Nickname;
            if (MessageBox.Show(this, $"Permanently delete {displayName}?", "Delete Pokémon", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            _profiles.DeletePokemon(selected.Id);
            RefreshPokemon();
        }

        private static OwnedPokemon GetSelectedPokemon(DataGridView grid) => grid.CurrentRow?.Tag as OwnedPokemon;

        private static void SelectPokemon(DataGridView grid, Guid id)
        {
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Tag is OwnedPokemon pokemon && pokemon.Id == id)
                {
                    row.Selected = true;
                    grid.CurrentCell = row.Cells["Name"];
                    return;
                }
            }
        }

        private void ShowPreview(OwnedPokemon pokemon)
        {
            if (pokemon == null)
                return;

            _previewName.Text = string.IsNullOrWhiteSpace(pokemon.Nickname)
                ? pokemon.SpeciesName
                : $"{pokemon.Nickname}\n({pokemon.SpeciesName})";
            _preview.Image?.Dispose();
            _preview.Image = null;
            var imagePath = PokemonArtworkLocator.Find(pokemon.SpeciesName, pokemon.CustomImagePath);
            if (imagePath == null)
                return;

            try
            {
                using var source = Image.FromFile(imagePath);
                _preview.Image = new Bitmap(source);
            }
            catch (ArgumentException)
            {
                _previewName.Text += "\nImage unavailable";
            }
        }

        private static void ConfigureDragSource(DataGridView grid)
        {
            grid.MouseDown += (sender, args) =>
            {
                var hit = grid.HitTest(args.X, args.Y);
                if (hit.RowIndex < 0 || grid.Rows[hit.RowIndex].Tag is not OwnedPokemon pokemon)
                    return;
                grid.DoDragDrop(new DataObject(PokemonDragFormat, pokemon.Id), DragDropEffects.Move);
            };
        }

        private void PartyDragEnter(object sender, DragEventArgs args)
        {
            args.Effect = args.Data.GetDataPresent(PokemonDragFormat) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void PartyDragDrop(object sender, DragEventArgs args)
        {
            if (args.Data.GetData(PokemonDragFormat) is not Guid pokemonId)
                return;
            var clientPoint = _party.PointToClient(new Point(args.X, args.Y));
            var hit = _party.HitTest(clientPoint.X, clientPoint.Y);
            var targetIndex = hit.RowIndex < 0 ? _party.Rows.Count : hit.RowIndex;
            var alreadyInParty = _profiles.GetPartyPokemon().Any(pokemon => pokemon.Id == pokemonId);
            TryPartyChange(() =>
            {
                if (alreadyInParty)
                    _profiles.ReorderPartyPokemon(pokemonId, targetIndex);
                else
                    _profiles.AddToParty(pokemonId, targetIndex);
            });
        }

        private void TryPartyChange(Action change)
        {
            try
            {
                change();
                RefreshPokemon();
            }
            catch (InvalidOperationException exception)
            {
                MessageBox.Show(this, exception.Message, "Party unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
