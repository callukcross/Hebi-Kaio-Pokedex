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

public sealed class PartyBattleForm : Form
{
    private readonly ProfileService _profiles;
    private readonly IPokemonCatalog _catalog;
    private readonly ReferenceRulesCatalog _rules;
    private readonly TrainerRulesCatalog _trainerRules;
    private readonly ListBox _party = new() { Dock = DockStyle.Left, Width = 220 };
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private OwnedPokemon _selected;

    public PartyBattleForm(ProfileService profiles, IPokemonCatalog catalog, ReferenceRulesCatalog rules, TrainerRulesCatalog trainerRules)
    {
        _profiles = profiles;
        _catalog = catalog;
        _rules = rules;
        _trainerRules = trainerRules;
        Text = "Active Party";
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
        toolbar.Controls.AddRange([AppTheme.Button("Edit Pokémon", (_, _) => Edit()), AppTheme.Button("Pokémon Center", (_, _) => Heal()), AppTheme.Button("Storage", (_, _) => Close()), AppTheme.Button("Close", (_, _) => Close())]);
        Controls.Add(_tabs);
        Controls.Add(_party);
        Controls.Add(toolbar);
        _party.SelectedIndexChanged += (_, _) => ShowPokemon();
        RefreshParty();
    }

    private void RefreshParty(Guid? selectedId = null)
    {
        _party.Items.Clear();
        foreach (var pokemon in _profiles.GetPartyPokemon()) _party.Items.Add(new PartyItem(pokemon));
        if (_party.Items.Count > 0)
            _party.SelectedIndex = selectedId is null ? 0 : Math.Max(0, _party.Items.Cast<PartyItem>().ToList().FindIndex(item => item.Pokemon.Id == selectedId));
        else
            _tabs.TabPages.Clear();
    }

    private void ShowPokemon()
    {
        if (_party.SelectedItem is not PartyItem item) return;
        _selected = item.Pokemon;
        var species = _rules.FindPokemon(_selected.SpeciesName);
        if (species is null) return;
        var effects = _profiles.GetTrainerEffects(_trainerRules);
        var maximumHp = PokemonRulesService.GetMaximumHp(_selected, species, _rules, effects);
        var attributes = PokemonRulesService.GetAttributes(_selected, species, _rules);
        _tabs.TabPages.Clear();
        _tabs.TabPages.Add(BuildInfoTab(species, effects, attributes, maximumHp));
        _tabs.TabPages.Add(BuildMovesTab());
        _tabs.TabPages.Add(BuildFeaturesTab());
    }

    private TabPage BuildInfoTab(PokemonRule species, TrainerEffects effects, AbilityScores attributes, int maximumHp)
    {
        var page = new TabPage("Information") { AutoScroll = true };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(18) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var name = string.IsNullOrWhiteSpace(_selected.Nickname) ? _selected.SpeciesName : _selected.Nickname;
        AddWide(layout, $"{name}  •  #{_selected.SpeciesNumber:000} {_selected.SpeciesName}  •  Level {_selected.Level}", 16, true);
        AddWide(layout, $"HP {_selected.CurrentHp}/{maximumHp}{(_selected.TemporaryHp > 0 ? $" +{_selected.TemporaryHp} temp" : "")}", 13, true);
        AddWide(layout, new ProgressBar { Width = 520, Height = 18, Maximum = Math.Max(1, maximumHp), Value = Math.Clamp(_selected.CurrentHp, 0, Math.Max(1, maximumHp)) });
        var hpControls = new FlowLayoutPanel { AutoSize = true };
        hpControls.Controls.AddRange([AppTheme.Button("− HP", (_, _) => ChangeHp(-1)), AppTheme.Button("+ HP", (_, _) => ChangeHp(1)), AppTheme.Button("Set HP", (_, _) => SetHp()), AppTheme.Button("Temp HP", (_, _) => SetTempHp())]);
        AddWide(layout, hpControls);
        var nextExperience = ExperienceForLevel(_selected.Level);
        AddRow(layout, "Experience", _selected.Level >= 20 ? $"{_selected.Experience} (maximum level)" : $"{_selected.Experience}/{nextExperience}");
        AddWide(layout, new ProgressBar { Width = 520, Height = 14, Maximum = Math.Max(1, nextExperience), Value = Math.Clamp(_selected.Experience, 0, Math.Max(1, nextExperience)) });
        AddWide(layout, AppTheme.Button("Set Experience", (_, _) => SetExperience()));
        AddRow(layout, "Armor Class", PokemonRulesService.GetArmorClass(_selected, species, effects).ToString());
        AddRow(layout, "Type", string.Join(" / ", species.Types));
        AddRow(layout, "Species Rating", species.SpeciesRating.ToString("0.#"));
        AddRow(layout, "Size / Hit Die", $"{species.Size} / d{species.HitDie}");
        AddRow(layout, "Nature", _selected.Nature);
        AddRow(layout, "Gender", _selected.Gender.ToString());
        AddRow(layout, "Saving Throws", species.SavingThrows.Count == 0 ? "None" : string.Join(", ", species.SavingThrows));
        AddRow(layout, "Catch DC", (10 + _selected.Level + (int)Math.Floor(species.SpeciesRating) + Math.Max(0, _selected.CurrentHp) / 10).ToString());
        AddRow(layout, "STAB / Proficiency", $"+{effects.StabBonus} / +{effects.ProficiencyBonus}");
        AddRow(layout, "Loyalty", _selected.Loyalty.ToString("+0;-0;0"));
        var loyalty = new FlowLayoutPanel { AutoSize = true };
        loyalty.Controls.AddRange([AppTheme.Button("− Loyalty", (_, _) => ChangeLoyalty(-1)), AppTheme.Button("+ Loyalty", (_, _) => ChangeLoyalty(1))]);
        AddWide(layout, loyalty);
        AddRow(layout, "STR / DEX / CON", $"{attributes.Strength} / {attributes.Dexterity} / {attributes.Constitution}");
        AddRow(layout, "INT / WIS / CHA", $"{attributes.Intelligence} / {attributes.Wisdom} / {attributes.Charisma}");
        AddRow(layout, "Speeds", $"Walk {species.WalkingSpeed} • Fly {species.FlyingSpeed} • Swim {species.SwimmingSpeed} • Burrow {species.BurrowingSpeed} • Climb {species.ClimbingSpeed}");
        AddRow(layout, "Senses", species.Senses.Count == 0 ? "None" : string.Join(", ", species.Senses));
        var defenses = PokemonTypeService.Calculate(species.Types);
        AddRow(layout, "Vulnerabilities", defenses.Vulnerabilities.Count == 0 ? "None" : string.Join(", ", defenses.Vulnerabilities));
        AddRow(layout, "Resistances", defenses.Resistances.Count == 0 ? "None" : string.Join(", ", defenses.Resistances));
        AddRow(layout, "Immunities", defenses.Immunities.Count == 0 ? "None" : string.Join(", ", defenses.Immunities));
        AddRow(layout, "Skills", string.Join(", ", _selected.Skills));
        AddRow(layout, "Held item", _selected.HeldItem ?? "None");
        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildMovesTab()
    {
        var page = new TabPage("Moves") { AutoScroll = true };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(12) };
        foreach (var ownedMove in _selected.Moves.OrderBy(move => move.Name))
        {
            var move = _rules.FindMove(ownedMove.Name);
            var card = new GroupBox { Text = $"{ownedMove.Name} — PP {ownedMove.CurrentPowerPoints}/{move?.PowerPoints ?? 0}", Width = 700, Height = 125 };
            card.Controls.Add(new Label { Dock = DockStyle.Fill, Padding = new Padding(8), Text = $"{move?.Type} • {move?.MoveTime} • {move?.Range} • {move?.Duration}\n{move?.Description}" });
            var minus = AppTheme.Button("−", (_, _) => ChangePp(ownedMove.Name, -1)); minus.Dock = DockStyle.Right;
            var plus = AppTheme.Button("+", (_, _) => ChangePp(ownedMove.Name, 1)); plus.Dock = DockStyle.Right;
            var reset = AppTheme.Button("Reset", (_, _) => ResetPp(ownedMove.Name)); reset.Dock = DockStyle.Right;
            card.Controls.Add(minus); card.Controls.Add(plus); card.Controls.Add(reset);
            flow.Controls.Add(card);
        }
        page.Controls.Add(flow);
        return page;
    }

    private TabPage BuildFeaturesTab()
    {
        var page = new TabPage("Abilities, Feats & Status") { AutoScroll = true };
        var status = new CheckedListBox { Dock = DockStyle.Top, Height = 145 };
        foreach (var value in Enum.GetValues<PokemonStatus>()) status.Items.Add(value, _selected.Statuses.Contains(value));
        status.ItemCheck += (_, _) => BeginInvoke((Action)(() => SaveStatuses(status)));
        page.Controls.Add(status);
        page.Controls.Add(new Label { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10), Text = "FEATS\n" + (string.Join(", ", _selected.Feats) is { Length: > 0 } feats ? feats : "None") });
        page.Controls.Add(new Label { Dock = DockStyle.Top, Height = 160, Padding = new Padding(10), Text = "ABILITIES\n" + string.Join("\n", _selected.Abilities.Select(name => $"• {name}: {_rules.Abilities.GetValueOrDefault(name)}")) });
        return page;
    }

    private void ChangeHp(int amount) => SaveBattle(new PokemonBattleUpdate { CurrentHp = _selected.CurrentHp + amount });
    private void SetHp() { var value = Prompt.Show(this, "Set HP", "Current HP:", _selected.CurrentHp.ToString()); if (int.TryParse(value, out var hp)) SaveBattle(new PokemonBattleUpdate { CurrentHp = hp }); }
    private void SetTempHp() { var value = Prompt.Show(this, "Temporary HP", "Temporary HP:", _selected.TemporaryHp.ToString()); if (int.TryParse(value, out var hp)) SaveBattle(new PokemonBattleUpdate { TemporaryHp = hp }); }
    private void SetExperience() { var value = Prompt.Show(this, "Experience", "Current experience:", _selected.Experience.ToString()); if (int.TryParse(value, out var experience)) SaveBattle(new PokemonBattleUpdate { Experience = experience }); }
    private void ChangeLoyalty(int amount) => SaveBattle(new PokemonBattleUpdate { Loyalty = _selected.Loyalty + amount });
    private void ChangePp(string move, int amount) => SaveBattle(new PokemonBattleUpdate { MovePowerPoints = new Dictionary<string, int> { [move] = _selected.Moves.Single(item => item.Name == move).CurrentPowerPoints + amount } });
    private void ResetPp(string move) => SaveBattle(new PokemonBattleUpdate { MovePowerPoints = new Dictionary<string, int> { [move] = _rules.FindMove(move)?.PowerPoints ?? 0 } });
    private void SaveStatuses(CheckedListBox list) => SaveBattle(new PokemonBattleUpdate { Statuses = list.CheckedItems.Cast<PokemonStatus>().ToList() });
    private void SaveBattle(PokemonBattleUpdate update) { _profiles.UpdateBattleState(_selected.Id, update, _rules); RefreshParty(_selected.Id); }
    private void Heal() { if (MessageBox.Show(this, "Heal the entire active party to perfect health?", "Pokémon Center", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) { _profiles.HealParty(_rules); RefreshParty(_selected?.Id); } }
    private void Edit() { if (_selected is null) return; using var dialog = new AdvancedPokemonEditorDialog(_catalog, _rules, _trainerRules, _selected); if (dialog.ShowDialog(this) == DialogResult.OK) { _profiles.UpdatePokemon(_selected.Id, dialog.Result, _rules); RefreshParty(_selected.Id); } }

    private static void AddRow(TableLayoutPanel layout, string label, string value) { layout.Controls.Add(new Label { Text = label, AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Margin = new Padding(3, 8, 3, 8) }); layout.Controls.Add(new Label { Text = value, AutoSize = true, MaximumSize = new Size(520, 0), Margin = new Padding(3, 8, 3, 8) }); }
    private static void AddWide(TableLayoutPanel layout, string text, float size, bool bold) => AddWide(layout, new Label { Text = text, AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, size, bold ? FontStyle.Bold : FontStyle.Regular), Margin = new Padding(3, 8, 3, 8) });
    private static void AddWide(TableLayoutPanel layout, Control control) { layout.Controls.Add(control); layout.SetColumnSpan(control, 2); }
    private static int ExperienceForLevel(int level) => level switch { <= 0 => 0, 1 => 200, 2 => 800, 3 => 2000, 4 => 6000, 5 => 12000, 6 => 20000, 7 => 30000, 8 => 44000, 9 => 62000, 10 => 82000, 11 => 104000, 12 => 128000, 13 => 158000, 14 => 194000, 15 => 234000, 16 => 278000, 17 => 326000, 18 => 382000, 19 => 450000, _ => 450000 };
    private sealed record PartyItem(OwnedPokemon Pokemon) { public override string ToString() => $"{Pokemon.Nickname ?? Pokemon.SpeciesName}  Lv.{Pokemon.Level}  HP {Pokemon.CurrentHp}{(Pokemon.TemporaryHp > 0 ? $" +{Pokemon.TemporaryHp}" : "")}"; }
}
