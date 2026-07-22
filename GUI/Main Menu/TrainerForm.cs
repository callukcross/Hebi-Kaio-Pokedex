using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Trainer;

namespace GUI
{
    public sealed class TrainerForm : Form
    {
        private readonly ProfileService _profiles;
        private readonly TrainerRulesCatalog _rules;
        private readonly ComboBox _class = new ComboBox();
        private readonly Dictionary<string, NumericUpDown> _abilities = new Dictionary<string, NumericUpDown>();
        private readonly CheckedListBox _feats = new CheckedListBox();
        private readonly TextBox _featDescription = new TextBox();
        private readonly Label _progression = new Label();
        private readonly Label _effects = new Label();
        private readonly DataGridView _inventory = new DataGridView();
        private readonly ComboBox _catalogItem = new ComboBox();
        private readonly NumericUpDown _quantity = new NumericUpDown();

        public TrainerForm(ProfileService profiles, TrainerRulesCatalog rules)
        {
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Text = "Trainer Character Sheet";
            BackColor = Color.FromArgb(244, 246, 249);
            MinimumSize = new Size(850, 650);
            InitializeUi();
            LoadTrainer();
        }

        private void InitializeUi()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(70, 48, 110) };
            var back = new Button { Text = "← Menu", Location = new Point(12, 12), Size = new Size(90, 32) };
            back.Click += (sender, args) => Close();
            header.Controls.Add(back);
            var title = new Label { Text = "TRAINER CHARACTER", ForeColor = Color.White, AutoSize = true, Location = new Point(125, 18), Font = new Font(Font.FontFamily, 12, FontStyle.Bold) };
            header.Controls.Add(title);
            Controls.Add(header);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateCharacterTab());
            tabs.TabPages.Add(CreateFeatsTab());
            tabs.TabPages.Add(CreateInventoryTab());
            Controls.Add(tabs);
            header.BringToFront();
        }

        private TabPage CreateCharacterTab()
        {
            var page = new TabPage("Character & Class") { Padding = new Padding(18) };
            page.Controls.Add(new Label { Text = "Trainer class", Location = new Point(20, 24), Size = new Size(120, 24) });
            _class.DropDownStyle = ComboBoxStyle.DropDownList;
            _class.SetBounds(145, 20, 220, 28);
            _class.Items.AddRange(_rules.Classes.Cast<object>().ToArray());
            page.Controls.Add(_class);

            _progression.SetBounds(400, 18, 380, 50);
            _progression.Font = new Font(Font.FontFamily, 10, FontStyle.Bold);
            page.Controls.Add(_progression);

            var labels = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
            for (var index = 0; index < labels.Length; index++)
            {
                var column = index % 2;
                var row = index / 2;
                var left = 20 + column * 220;
                var top = 90 + row * 48;
                page.Controls.Add(new Label { Text = labels[index], Location = new Point(left, top + 4), Size = new Size(110, 24) });
                var value = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 10, Location = new Point(left + 115, top), Width = 70 };
                _abilities[labels[index]] = value;
                page.Controls.Add(value);
            }

            var save = new Button { Text = "Save Character", Location = new Point(20, 255), Size = new Size(150, 36) };
            save.Click += (sender, args) => SaveTrainer();
            page.Controls.Add(save);
            _effects.SetBounds(20, 315, 740, 200);
            _effects.Font = new Font(FontFamily.GenericMonospace, 10);
            page.Controls.Add(_effects);
            return page;
        }

        private TabPage CreateFeatsTab()
        {
            var page = new TabPage("Feats") { Padding = new Padding(12) };
            _feats.Dock = DockStyle.Left;
            _feats.Width = 310;
            _feats.CheckOnClick = true;
            _feats.Items.AddRange(_rules.Feats.Keys.OrderBy(name => name).Cast<object>().ToArray());
            _feats.SelectedIndexChanged += (sender, args) =>
            {
                if (_feats.SelectedItem is string name && _rules.Feats.TryGetValue(name, out var description))
                    _featDescription.Text = description;
            };
            page.Controls.Add(_feats);
            _featDescription.Dock = DockStyle.Fill;
            _featDescription.Multiline = true;
            _featDescription.ReadOnly = true;
            _featDescription.ScrollBars = ScrollBars.Vertical;
            page.Controls.Add(_featDescription);
            _featDescription.BringToFront();
            var save = new Button { Text = "Save Selected Feats", Dock = DockStyle.Bottom, Height = 40 };
            save.Click += (sender, args) => SaveTrainer();
            page.Controls.Add(save);
            return page;
        }

        private TabPage CreateInventoryTab()
        {
            var page = new TabPage("Inventory") { Padding = new Padding(10) };
            var controls = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, FlowDirection = FlowDirection.LeftToRight };
            _catalogItem.Width = 330;
            _catalogItem.DropDownStyle = ComboBoxStyle.DropDownList;
            _catalogItem.Items.AddRange(_rules.Items.Keys.OrderBy(name => name).Cast<object>().ToArray());
            if (_catalogItem.Items.Count > 0)
                _catalogItem.SelectedIndex = 0;
            controls.Controls.Add(_catalogItem);
            _quantity.Minimum = 1;
            _quantity.Maximum = 999;
            _quantity.Value = 1;
            _quantity.Width = 70;
            controls.Controls.Add(_quantity);
            var add = new Button { Text = "Add catalog item", AutoSize = true };
            add.Click += AddCatalogItem;
            controls.Controls.Add(add);
            var custom = new Button { Text = "Add custom item", AutoSize = true };
            custom.Click += AddCustomItem;
            controls.Controls.Add(custom);
            page.Controls.Add(controls);

            _inventory.Dock = DockStyle.Fill;
            _inventory.ReadOnly = true;
            _inventory.AllowUserToAddRows = false;
            _inventory.AllowUserToDeleteRows = false;
            _inventory.AutoGenerateColumns = false;
            _inventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _inventory.MultiSelect = false;
            _inventory.RowHeadersVisible = false;
            _inventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Item", Width = 220 });
            _inventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Qty", Width = 55 });
            _inventory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Effect / Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            page.Controls.Add(_inventory);
            _inventory.BringToFront();
            var remove = new Button { Text = "Remove selected", Dock = DockStyle.Bottom, Height = 38 };
            remove.Click += RemoveInventoryItem;
            page.Controls.Add(remove);
            return page;
        }

        private void LoadTrainer()
        {
            var trainer = _profiles.GetTrainer();
            _class.SelectedItem = _rules.Classes.Contains(trainer.ClassName) ? trainer.ClassName : _rules.Classes[0];
            _abilities["Strength"].Value = trainer.Abilities.Strength;
            _abilities["Dexterity"].Value = trainer.Abilities.Dexterity;
            _abilities["Constitution"].Value = trainer.Abilities.Constitution;
            _abilities["Intelligence"].Value = trainer.Abilities.Intelligence;
            _abilities["Wisdom"].Value = trainer.Abilities.Wisdom;
            _abilities["Charisma"].Value = trainer.Abilities.Charisma;
            for (var index = 0; index < _feats.Items.Count; index++)
                _feats.SetItemChecked(index, trainer.Feats.Contains(_feats.Items[index].ToString(), StringComparer.OrdinalIgnoreCase));
            RefreshInventory(trainer);
            RefreshEffects();
        }

        private void SaveTrainer()
        {
            var selectedFeats = _feats.CheckedItems.Cast<string>().ToList();
            _profiles.UpdateTrainer(new TrainerUpdate
            {
                ClassName = _class.SelectedItem?.ToString() ?? _rules.Classes[0],
                Abilities = new AbilityScores
                {
                    Strength = (int)_abilities["Strength"].Value,
                    Dexterity = (int)_abilities["Dexterity"].Value,
                    Constitution = (int)_abilities["Constitution"].Value,
                    Intelligence = (int)_abilities["Intelligence"].Value,
                    Wisdom = (int)_abilities["Wisdom"].Value,
                    Charisma = (int)_abilities["Charisma"].Value
                },
                Feats = selectedFeats
            });
            RefreshEffects();
        }

        private void RefreshEffects()
        {
            var profile = _profiles.GetActiveProfile();
            var effects = _profiles.GetTrainerEffects(_rules);
            var caught = profile.Pokedex.Values.Count(state => state == PokedexEntryState.Caught);
            var next = profile.TrainerLevel >= 20 ? "Maximum level" : $"Next level at {profile.TrainerLevel * TrainerRulesService.CaughtEntriesPerLevel} caught entries";
            _progression.Text = $"Level {profile.TrainerLevel} • {caught} caught\n{next}";
            _effects.Text =
                $"Proficiency       +{effects.ProficiencyBonus}\n" +
                $"STAB              +{effects.StabBonus}\n" +
                $"Pokémon attacks   +{effects.PokemonAttackBonus}\n" +
                $"Pokémon damage    +{effects.PokemonDamageBonus}\n" +
                $"Pokémon AC        +{effects.PokemonArmorClassBonus}\n" +
                $"Initiative        +{effects.PokemonInitiativeBonus}\n" +
                $"HP per level      +{effects.PokemonHitPointsPerLevel}\n" +
                $"Move slots        +{effects.ExtraMoveSlots}\n" +
                $"Catch attempts    +{effects.CatchBonus}";
        }

        private void AddCatalogItem(object sender, EventArgs args)
        {
            if (_catalogItem.SelectedItem is not string name)
                return;
            _profiles.AddInventoryItem(name, _rules.Items[name], (int)_quantity.Value);
            RefreshInventory(_profiles.GetTrainer());
        }

        private void AddCustomItem(object sender, EventArgs args)
        {
            using var dialog = new CustomItemDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            _profiles.AddInventoryItem(dialog.ItemName, dialog.Description, dialog.Quantity, isCustom: true);
            RefreshInventory(_profiles.GetTrainer());
        }

        private void RemoveInventoryItem(object sender, EventArgs args)
        {
            if (_inventory.CurrentRow?.Tag is not InventoryEntry item)
                return;
            _profiles.SetInventoryQuantity(item.Id, 0);
            RefreshInventory(_profiles.GetTrainer());
        }

        private void RefreshInventory(TrainerCharacter trainer)
        {
            _inventory.Rows.Clear();
            foreach (var item in trainer.Inventory.OrderBy(item => item.Name))
            {
                var row = _inventory.Rows.Add(item.Name + (item.IsCustom ? " (Custom)" : string.Empty), item.Quantity, item.Description);
                _inventory.Rows[row].Tag = item;
            }
        }
    }

    internal sealed class CustomItemDialog : Form
    {
        private readonly TextBox _name = new TextBox();
        private readonly TextBox _description = new TextBox();
        private readonly NumericUpDown _quantity = new NumericUpDown();

        public string ItemName => _name.Text.Trim();
        public string Description => _description.Text.Trim();
        public int Quantity => (int)_quantity.Value;

        public CustomItemDialog()
        {
            Text = "Add Custom Item";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(440, 260);
            Controls.Add(new Label { Text = "Name", Location = new Point(18, 20), Size = new Size(80, 24) });
            _name.SetBounds(105, 18, 310, 28);
            Controls.Add(_name);
            Controls.Add(new Label { Text = "Notes", Location = new Point(18, 62), Size = new Size(80, 24) });
            _description.SetBounds(105, 60, 310, 100);
            _description.Multiline = true;
            Controls.Add(_description);
            Controls.Add(new Label { Text = "Quantity", Location = new Point(18, 177), Size = new Size(80, 24) });
            _quantity.SetBounds(105, 175, 80, 28);
            _quantity.Minimum = 1;
            _quantity.Maximum = 999;
            _quantity.Value = 1;
            Controls.Add(_quantity);
            var save = new Button { Text = "Add", Location = new Point(225, 214), Size = new Size(90, 32) };
            save.Click += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(_name.Text))
                {
                    MessageBox.Show(this, "Enter an item name.", "Name required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(save);
            Controls.Add(new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(325, 214), Size = new Size(90, 32) });
            AcceptButton = save;
        }
    }
}
