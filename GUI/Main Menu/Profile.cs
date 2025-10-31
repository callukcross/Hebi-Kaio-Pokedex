using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GUI
{
    public partial class Profile : ResizableForm
    {
        public Profile()
        {
            InitializeComponent();
        }

        private void profileSelect_Click(object sender, EventArgs e)
        {
            // Use the existing "Profile Select" button as a Back action.
            // Closing the Profile form triggers MainMenu1's FormClosed handler
            // (or RestoreMainView when embedded) which will return the user to the main view.
            this.Close();
        }
    }
}
