using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainMenu1 : ResizableForm
    {
        // Holds the original controls so we can restore them after embedding Profile.
        private List<Control> _mainControls;
        // The embedded Profile form instance (when shown inside this form).
        private Form _embeddedProfile;

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
            // TODO: Add logic to create a new profile here.
            // For example, show a new form or prompt for profile details.
        }
    }
}
