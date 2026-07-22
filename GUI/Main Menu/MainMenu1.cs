using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using HebiKaio.Core.Profiles;
using HebiKaio.Core.Pokedex;
using HebiKaio.Core.Trainer;
using HebiKaio.Core.Content;

namespace GUI
{
    public partial class MainMenu1 : ResizableForm
    {
        // Holds the original controls so we can restore them after embedding Profile.
        private List<Control> _mainControls;
        // The embedded Profile form instance (when shown inside this form).
        private Form _embeddedProfile;

        // Keep a reference to the new profile dialog when it's shown modelessly
        private NewProfileDialog _newProfileDialog;
        private readonly ProfileService _profileService;
        private readonly IProfileRepository _profileRepository;
        private readonly string _moduleDirectory;

        public MainMenu1()
        {
            InitializeComponent();

            var savePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HebiKaioPokedex",
                "profiles.json");
            _profileRepository = new JsonProfileRepository(savePath);
            _profileService = new ProfileService(_profileRepository);
            _moduleDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HebiKaioPokedex", "modules");
            RefreshActiveProfile();
        }

        private void RefreshActiveProfile()
        {
            try
            {
                var activeProfile = _profileService.GetActiveProfile();
                testProfile.Text = activeProfile == null ? "No Active Profile" : $"{activeProfile.Name}'s PC";
                testProfile.Enabled = activeProfile != null;
            }
            catch (Exception exception)
            {
                testProfile.Text = "Profile Save Error";
                testProfile.Enabled = false;
                MessageBox.Show(this, exception.Message, "Unable to load profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void testProfile_Click(object sender, EventArgs e)
        {
            try
            {
                var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "p5e");
                ShowEmbeddedForm(new PokemonPcForm(_profileService, JsonPokemonCatalog.Load(dataPath), TrainerRulesCatalog.Load(dataPath, _moduleDirectory)));
            }
            catch (Exception exception) when (exception is IOException || exception is InvalidDataException)
            {
                MessageBox.Show(this, exception.Message, "Unable to load Pokémon PC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pokedexButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_profileService.GetActiveProfile() == null)
                {
                    MessageBox.Show(this, "Create a trainer profile before opening the Pokédex.", "Profile required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "p5e");
                ShowEmbeddedForm(new PokedexForm(_profileService, JsonPokemonCatalog.Load(dataPath)));
            }
            catch (Exception exception) when (exception is IOException || exception is InvalidDataException)
            {
                MessageBox.Show(this, exception.Message, "Unable to load Pokédex", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trainerButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_profileService.GetActiveProfile() == null)
                {
                    MessageBox.Show(this, "Create a trainer profile before opening the character sheet.", "Profile required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "p5e");
                ShowEmbeddedForm(new TrainerForm(_profileService, TrainerRulesCatalog.Load(dataPath, _moduleDirectory)));
            }
            catch (Exception exception) when (exception is IOException || exception is InvalidDataException)
            {
                MessageBox.Show(this, exception.Message, "Unable to load trainer rules", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataButton_Click(object sender, EventArgs e)
        {
            ShowEmbeddedForm(new DataManagementForm(
                new ProfileTransferService(_profileRepository),
                new CustomContentService(_moduleDirectory),
                RefreshActiveProfile));
        }

        private void ShowEmbeddedForm(Form form)
        {
            if (_embeddedProfile != null)
                return;

            _mainControls = Controls.Cast<Control>().ToList();
            SuspendLayout();
            try
            {
                Controls.Clear();
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                form.FormClosed += (s, args) => BeginInvoke((Action)(RestoreMainView));
                Controls.Add(form);
                _embeddedProfile = form;
                form.Show();
            }
            finally
            {
                ResumeLayout();
                Invalidate(true);
                Update();
            }
        }

        private void RestoreMainView()
        {
            if (_embeddedProfile != null)
            {
                // Remove and dispose embedded form
                Controls.Remove(_embeddedProfile);
                try { _embeddedProfile.Close(); }
                catch { }
                _embeddedProfile.Dispose();
                _embeddedProfile = null;
            }

            // Restore the original controls
            if (_mainControls != null)
            {
                this.SuspendLayout();
                Controls.AddRange(_mainControls.ToArray());
                _mainControls = null;
                this.ResumeLayout();
                this.Invalidate(true);
                this.Update();
            }
        }

        private void ProfileForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void newProfile_Click(object sender, EventArgs e)
        {
            // If a dialog is already open, bring it to front instead of creating another.
            if (_newProfileDialog != null && !_newProfileDialog.IsDisposed)
            {
                _newProfileDialog.BringToFront();
                return;
            }

            // Show the NewProfileDialog modelessly so the main window remains movable.
            var dlg = new NewProfileDialog();
            _newProfileDialog = dlg;

            // Handle the ProfileCreated event raised by the dialog (modeless path).
            EventHandler<string> createdHandler = null;
            createdHandler = (s, name) =>
            {
                // Ensure handler runs on the main form thread
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((Action)(() => createdHandler(s, name)));
                    return;
                }

                var trimmed = name?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    MessageBox.Show(this, "Profile name cannot be empty.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    try
                    {
                        var profile = _profileService.CreateProfile(trimmed);
                        RefreshActiveProfile();
                        MessageBox.Show(this, $"Profile '{profile.Name}' created and saved.", "Profile Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException || exception is IOException)
                    {
                        MessageBox.Show(this, exception.Message, "Unable to create profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };

            dlg.ProfileCreated += createdHandler;

            // Keep dialog positioned relative to the main window: compute initial offset and update when main moves.
            Point offset = Point.Empty;
            EventHandler ownerMoved = null;
            ownerMoved = (s, args) =>
            {
                if (dlg == null || dlg.IsDisposed) return;
                try
                {
                    dlg.Location = new Point(this.Location.X + offset.X, this.Location.Y + offset.Y);
                }
                catch { }
            };

            // When dialog closes, remove the ownerMoved handler and clear reference.
            dlg.FormClosed += (s, args) =>
            {
                try { dlg.ProfileCreated -= createdHandler; } catch { }
                try { this.LocationChanged -= ownerMoved; } catch { }
                if (_newProfileDialog == dlg) _newProfileDialog = null;
            };

            // Show the dialog modelessly.
            dlg.Show(this);

            // Position dialog centered over the main form initially.
            try
            {
                var x = this.Left + (this.Width - dlg.Width) / 2;
                var y = this.Top + (this.Height - dlg.Height) / 2;
                dlg.Location = new Point(x, y);
            }
            catch { }

            // Compute how far the dialog is from the main window, then keep that offset.
            offset = new Point(dlg.Location.X - this.Location.X, dlg.Location.Y - this.Location.Y);

            // Subscribe to main window location changes to move the dialog accordingly.
            this.LocationChanged += ownerMoved;
        }
    }
}
