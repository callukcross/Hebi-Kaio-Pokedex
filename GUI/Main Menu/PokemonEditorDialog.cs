using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;

namespace GUI
{
    public sealed class PokemonEditorDialog : Form
    {
        private readonly ComboBox _species = new ComboBox();
        private readonly TextBox _nickname = new TextBox();
        private readonly NumericUpDown _level = new NumericUpDown();
        private readonly TextBox _form = new TextBox();
        private readonly TextBox _imagePath = new TextBox();
        private readonly IReadOnlyList<PokemonSpecies> _catalog;

        public PokemonDraft Result { get; private set; }

        public PokemonEditorDialog(IPokemonCatalog catalog, OwnedPokemon existing = null)
        {
            _catalog = catalog?.GetAll() ?? throw new ArgumentNullException(nameof(catalog));
            Text = existing == null ? "Create Pokémon" : "Edit Pokémon";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(500, 330);
            MaximizeBox = false;
            MinimizeBox = false;
            InitializeFields(existing);
        }

        private void InitializeFields(OwnedPokemon existing)
        {
            AddLabel("Species", 22);
            _species.SetBounds(130, 18, 335, 28);
            _species.DropDownStyle = ComboBoxStyle.DropDown;
            _species.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _species.AutoCompleteSource = AutoCompleteSource.ListItems;
            foreach (var pokemon in _catalog)
                _species.Items.Add(new SpeciesOption(pokemon));
            Controls.Add(_species);

            AddLabel("Nickname", 68);
            _nickname.SetBounds(130, 64, 335, 28);
            _nickname.MaxLength = 40;
            Controls.Add(_nickname);

            AddLabel("Level", 114);
            _level.SetBounds(130, 110, 90, 28);
            _level.Minimum = 1;
            _level.Maximum = 20;
            _level.Value = 1;
            Controls.Add(_level);

            AddLabel("Form", 160);
            _form.SetBounds(130, 156, 335, 28);
            _form.MaxLength = 40;
            Controls.Add(_form);

            AddLabel("Custom image", 206);
            _imagePath.SetBounds(130, 202, 250, 28);
            _imagePath.ReadOnly = true;
            Controls.Add(_imagePath);
            var browse = new Button { Text = "Browse…", Location = new Point(388, 200), Size = new Size(77, 30) };
            browse.Click += BrowseForImage;
            Controls.Add(browse);

            var save = new Button { Text = "Save", Location = new Point(264, 270), Size = new Size(95, 34) };
            save.Click += Save;
            Controls.Add(save);
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(370, 270), Size = new Size(95, 34) };
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;

            if (existing == null)
            {
                _species.SelectedIndex = 0;
                return;
            }

            _species.SelectedItem = _species.Items.Cast<SpeciesOption>().FirstOrDefault(option => option.Species.Number == existing.SpeciesNumber);
            _nickname.Text = existing.Nickname ?? string.Empty;
            _level.Value = Math.Clamp(existing.Level, 1, 20);
            _form.Text = existing.Form ?? string.Empty;
            _imagePath.Text = existing.CustomImagePath ?? string.Empty;
        }

        private void AddLabel(string text, int top)
        {
            Controls.Add(new Label { Text = text, Location = new Point(20, top), Size = new Size(100, 24), TextAlign = ContentAlignment.MiddleLeft });
        }

        private void BrowseForImage(object sender, EventArgs args)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose Pokémon artwork",
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All files|*.*",
                CheckFileExists = true
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                _imagePath.Text = dialog.FileName;
        }

        private void Save(object sender, EventArgs args)
        {
            if (_species.SelectedItem is not SpeciesOption option)
            {
                MessageBox.Show(this, "Choose a species from the list.", "Species required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = new PokemonDraft
            {
                SpeciesNumber = option.Species.Number,
                SpeciesName = option.Species.Name,
                Nickname = _nickname.Text,
                Level = (int)_level.Value,
                Form = _form.Text,
                CustomImagePath = _imagePath.Text
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private sealed class SpeciesOption
        {
            public PokemonSpecies Species { get; }

            public SpeciesOption(PokemonSpecies species) => Species = species;

            public override string ToString() => $"#{Species.Number:000} {Species.Name}";
        }
    }
}
