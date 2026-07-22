using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HebiKaio.Core.Profiles;

namespace GUI;

public sealed class ProfilesForm : Form
{
    private readonly ProfileService _profiles;
    private readonly Action _activeChanged;
    private readonly TextBox _search = new() { Dock = DockStyle.Top, PlaceholderText = "Search trainer profiles..." };
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };

    public ProfilesForm(ProfileService profiles, Action activeChanged)
    {
        _profiles = profiles;
        _activeChanged = activeChanged;
        Text = "Trainer Profiles";
        BackColor = AppTheme.Background;
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
        actions.Controls.AddRange([
            AppTheme.Button("New Profile", (_, _) => Create()),
            AppTheme.Button("Use Selected", (_, _) => ActivateSelected()),
            AppTheme.Button("Rename", (_, _) => Rename()),
            AppTheme.Button("Delete", (_, _) => Delete()),
            AppTheme.Button("Close", (_, _) => Close())]);
        Controls.Add(_list);
        Controls.Add(_search);
        Controls.Add(actions);
        _search.TextChanged += (_, _) => RefreshList();
        _list.DoubleClick += (_, _) => ActivateSelected();
        RefreshList();
    }

    private ProfileItem Selected => _list.SelectedItem as ProfileItem;

    private void RefreshList()
    {
        var active = _profiles.GetActiveProfile()?.Id;
        var query = _search.Text.Trim();
        _list.Items.Clear();
        foreach (var profile in _profiles.GetProfiles().Where(item => query.Length == 0 || item.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
            _list.Items.Add(new ProfileItem(profile, profile.Id == active));
    }

    private void Create()
    {
        var name = Prompt.Show(this, "New Profile", "Trainer name:");
        if (name is null) return;
        Run(() => { _profiles.CreateProfile(name); _activeChanged(); RefreshList(); });
    }

    private void ActivateSelected()
    {
        if (Selected is not { } selected) return;
        Run(() => { _profiles.SetActiveProfile(selected.Profile.Id); _activeChanged(); RefreshList(); });
    }

    private void Rename()
    {
        if (Selected is not { } selected) return;
        var name = Prompt.Show(this, "Rename Profile", "Trainer name:", selected.Profile.Name);
        if (name is null) return;
        Run(() => { _profiles.RenameProfile(selected.Profile.Id, name); _activeChanged(); RefreshList(); });
    }

    private void Delete()
    {
        if (Selected is not { } selected || MessageBox.Show(this, $"Delete '{selected.Profile.Name}' and all Pokémon in that profile?", "Delete profile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Run(() => { _profiles.DeleteProfile(selected.Profile.Id); _activeChanged(); RefreshList(); });
    }

    private void Run(Action action)
    {
        try { action(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or IOException or KeyNotFoundException)
        { MessageBox.Show(this, exception.Message, "Profile operation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private sealed record ProfileItem(TrainerProfile Profile, bool Active)
    {
        public override string ToString() => $"{(Active ? "▶ " : "   ")}{Profile.Name} — {Profile.Pokemon.Count} Pokémon — Lv. {Profile.TrainerLevel}";
    }
}
