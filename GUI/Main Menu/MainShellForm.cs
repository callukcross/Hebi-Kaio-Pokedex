using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Content;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Rules;
using HebiKaio.Core.Trainer;

namespace GUI;

public sealed class MainShellForm : Form
{
    private readonly IProfileRepository _repository;
    private readonly ProfileService _profiles;
    private readonly JsonPokemonCatalog _catalog;
    private readonly ReferenceRulesCatalog _referenceRules;
    private readonly TrainerRulesCatalog _trainerRules;
    private readonly string _moduleDirectory;
    private readonly Panel _navigation = new() { Dock = DockStyle.Left, Width = 210, AutoScroll = true };
    private readonly Panel _content = new() { Dock = DockStyle.Fill };
    private readonly Label _activeProfile = new() { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(12), TextAlign = ContentAlignment.MiddleLeft };
    private Form _currentPage;

    public MainShellForm()
    {
        Text = "HebiKaio Pokédex 5E";
        MinimumSize = new Size(1000, 720);
        ClientSize = new Size(1180, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.Text;

        var appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HebiKaioPokedex");
        _moduleDirectory = Path.Combine(appDirectory, "modules");
        _repository = new JsonProfileRepository(Path.Combine(appDirectory, "profiles.json"));
        _profiles = new ProfileService(_repository);
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data", "p5e");
        _catalog = JsonPokemonCatalog.Load(dataDirectory);
        _referenceRules = ReferenceRulesCatalog.Load(dataDirectory);
        _trainerRules = TrainerRulesCatalog.Load(dataDirectory, _moduleDirectory);

        BuildNavigation();
        Controls.Add(_content);
        Controls.Add(_navigation);
        ShowDashboard();
    }

    private void BuildNavigation()
    {
        _navigation.BackColor = Color.FromArgb(24, 26, 30);
        var title = new Label { Text = "HEBIKAIO\nPOKÉDEX 5E", Dock = DockStyle.Top, Height = 82, Font = new Font(Font.FontFamily, 15, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White };
        var links = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(10), BackColor = _navigation.BackColor };
        AddLink(links, "Party", () => RequireProfile(() => new PartyBattleForm(_profiles, _catalog, _referenceRules, _trainerRules)));
        AddLink(links, "Pokémon Storage", () => RequireProfile(() => new PokemonPcForm(_profiles, _catalog, _trainerRules, _referenceRules)));
        AddActionLink(links, "Add Pokémon", AddPokemon);
        AddLink(links, "Generate Encounter", () => RequireProfile(() => new EncounterGeneratorForm(_profiles, _catalog, _referenceRules)));
        AddLink(links, "Pokédex", () => RequireProfile(() => new PokedexForm(_profiles, _catalog, _referenceRules)));
        AddLink(links, "Trainer", () => RequireProfile(() => new TrainerForm(_profiles, _trainerRules)));
        AddLink(links, "Profiles", () => new ProfilesForm(_profiles, RefreshProfile));
        AddLink(links, "Receive / Import", () => new DataManagementForm(new ProfileTransferService(_repository), new CustomContentService(_moduleDirectory), RefreshProfile));
        AddLink(links, "Connect", () => new NetworkForm());
        AddLink(links, "Settings", () => new SettingsForm(_moduleDirectory));
        AddLink(links, "About", () => new AboutForm());
        _navigation.Controls.Add(links);
        _navigation.Controls.Add(_activeProfile);
        _navigation.Controls.Add(title);
        RefreshProfile();
    }

    private static void AddLink(Control parent, string text, Func<Form> page)
    {
        var button = new Button { Text = text, Width = 170, Height = 40, Margin = new Padding(0, 0, 0, 7), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), BackColor = AppTheme.Surface, ForeColor = AppTheme.Text };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => ((MainShellForm)button.FindForm()).ShowPage(page());
        parent.Controls.Add(button);
    }

    private static void AddActionLink(Control parent, string text, Action action)
    {
        var button = new Button { Text = text, Width = 170, Height = 40, Margin = new Padding(0, 0, 0, 7), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), BackColor = AppTheme.Surface, ForeColor = AppTheme.Text };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        parent.Controls.Add(button);
    }

    private void ShowDashboard()
    {
        _content.Controls.Clear();
        var active = _profiles.GetActiveProfile();
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(45), BackColor = AppTheme.Background };
        panel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 150, Text = active is null ? "Open Profiles from the navigation menu to begin." : $"Trainer level {active.TrainerLevel}\n{active.PartyPokemonIds.Count} active Pokémon • {active.Pokemon.Count} owned\n{active.Pokedex.Count(item => item.Value == PokedexEntryState.Caught)} caught • {active.Pokedex.Count(item => item.Value >= PokedexEntryState.Seen)} seen", Font = new Font(Font.FontFamily, 15), ForeColor = Color.Gainsboro });
        panel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 100, Text = active is null ? "Choose or create a trainer profile" : $"Welcome back, {active.Name}", Font = new Font(Font.FontFamily, 24, FontStyle.Bold), ForeColor = AppTheme.Text });
        _content.Controls.Add(panel);
    }

    private Form RequireProfile(Func<Form> factory)
    {
        if (_profiles.GetActiveProfile() is not null) return factory();
        MessageBox.Show(this, "Create or choose a trainer profile first.", "Profile required", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return new ProfilesForm(_profiles, RefreshProfile);
    }

    private void AddPokemon()
    {
        if (_profiles.GetActiveProfile() is null) { ShowPage(new ProfilesForm(_profiles, RefreshProfile)); return; }
        using var dialog = new AdvancedPokemonEditorDialog(_catalog, _referenceRules, _trainerRules);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _profiles.CreatePokemon(dialog.Result, _referenceRules);
            ShowPage(new PokemonPcForm(_profiles, _catalog, _trainerRules, _referenceRules));
        }
    }

    private void ShowPage(Form page)
    {
        if (_currentPage is not null) { _content.Controls.Remove(_currentPage); _currentPage.Dispose(); }
        _currentPage = page;
        page.TopLevel = false;
        page.FormBorderStyle = FormBorderStyle.None;
        page.Dock = DockStyle.Fill;
        page.FormClosed += (_, _) => { _currentPage = null; RefreshProfile(); ShowDashboard(); };
        _content.Controls.Add(page);
        page.Show();
    }

    private void RefreshProfile()
    {
        var active = _profiles.GetActiveProfile();
        _activeProfile.Text = active is null ? "No active profile" : $"{active.Name}\nTrainer Lv. {active.TrainerLevel}";
    }
}
