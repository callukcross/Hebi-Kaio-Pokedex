using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

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

        public MainMenu1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void testProfile_Click(object sender, EventArgs e)
        {
            // If already showing embedded profile, do nothing.
            if (_embeddedProfile != null)
                return;

            // Snapshot existing top-level controls so we can restore them later.
            _mainControls = Controls.Cast<Control>().ToList();

            // Remove them from the form (do not Dispose; we'll re-add them).
            // Suspend layout while we clear and add to avoid repeated layout passes.
            this.SuspendLayout();
            try
            {
                Controls.Clear();

                // Create the PokemonActivePartyScreen form and embed it as a child control.
                var partyForm = new ActiveParty
                {
                    TopLevel = false,                       // make it a child control
                    FormBorderStyle = FormBorderStyle.None, // remove window chrome
                    Dock = DockStyle.Fill                    // fill the client area exactly
                };

                // When partyForm is closed (radial button will call Close()),
                // restore the original controls. Use BeginInvoke to avoid re-entrancy issues.
                partyForm.FormClosed += (s, args) =>
                {
                    BeginInvoke((Action)(() => RestoreMainView()));
                };

                // Add the embedded form to this form's Controls and show it.
                Controls.Add(partyForm);
                _embeddedProfile = partyForm;
                partyForm.Show();
            }
            finally
            {
                this.ResumeLayout();
                // Force immediate repaint to reduce visual artifacts when switching pages quickly.
                this.Invalidate(true);
                this.Update();
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
                    // TODO: Insert profile creation logic here.
                    MessageBox.Show(this, $"Profile '{trimmed}' created.", "Profile Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
