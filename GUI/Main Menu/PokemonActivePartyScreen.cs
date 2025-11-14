using System;

namespace GUI
{
    public partial class PokemonActivePartyScreen : GUI.ResizableForm
    {
        public PokemonActivePartyScreen()
        {
            InitializeComponent();

            // Wire the radial button to close this screen (acts as the Back button
            // when the form is embedded inside MainMenu1).
            this.radialMenuButton.Click += RadialMenuButton_Click;
        }

        private void PokemonActivePartyScreen_Load(object sender, EventArgs e)
        {

        }

        private void RadialMenuButton_Click(object sender, EventArgs e)
        {
            // Closing the form triggers MainMenu1's FormClosed handler which restores the main view.
            this.Close();
        }
    }
}
