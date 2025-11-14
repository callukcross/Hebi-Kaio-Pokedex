using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    // Simple modal dialog used to request a profile name.
    public class NewProfileDialog : Form
    {
        private TextBox _textBox;
        private Button _createBtn;
        private Button _cancelBtn;

        public string ProfileName => _textBox.Text;

        // Event raised when the user successfully creates a profile (passes the entered name)
        public event EventHandler<string> ProfileCreated;

        // Win32 message constants used to block moving by dragging the title bar
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;

        public NewProfileDialog()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "New Profile";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(360, 130);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            // Remove the close button (the 'X') from the title bar
            this.ControlBox = false;

            _textBox = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(320, 24),
                ForeColor = SystemColors.GrayText,
                Text = "ENTER NAME"
            };
            _textBox.GotFocus += (s, e) =>
            {
                if (_textBox.Text == "ENTER NAME")
                {
                    _textBox.Text = string.Empty;
                    _textBox.ForeColor = SystemColors.WindowText;
                }
            };
            _textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_textBox.Text))
                {
                    _textBox.Text = "ENTER NAME";
                    _textBox.ForeColor = SystemColors.GrayText;
                }
            };

            _createBtn = new Button
            {
                Text = "Create",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 30),
                Location = new Point(60, 70)
            };
            _createBtn.Click += (s, e) =>
            {
                // If the user didn't change the placeholder, treat as empty
                var name = _textBox.Text == "ENTER NAME" ? string.Empty : _textBox.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show(this, "Please enter a profile name.", "Missing name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None; // prevent dialog from closing
                    return;
                }

                // Raise event for modeless usage
                ProfileCreated?.Invoke(this, name);

                // For modal usage, set DialogResult so caller can read it
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            _cancelBtn = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 30),
                Location = new Point(200, 70)
            };
            _cancelBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(_textBox);
            this.Controls.Add(_createBtn);
            this.Controls.Add(_cancelBtn);

            // Allow Enter to activate Create and Escape to cancel
            this.AcceptButton = _createBtn;
            this.CancelButton = _cancelBtn;
        }

        protected override void WndProc(ref Message m)
        {
            // Ignore attempts to start a non-client left-button down on the caption (title bar)
            // which would normally allow moving the window.
            if (m.Msg == WM_NCLBUTTONDOWN && m.WParam == (IntPtr)HTCAPTION)
            {
                return;
            }

            base.WndProc(ref m);
        }
    }
}
