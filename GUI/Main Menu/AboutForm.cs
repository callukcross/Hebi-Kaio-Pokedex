using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace GUI;

public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About HebiKaio Pokédex 5E";
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "development";
        var text = new Label { Dock = DockStyle.Fill, Padding = new Padding(30), TextAlign = ContentAlignment.MiddleCenter, Font = new Font(SystemFonts.DefaultFont.FontFamily, 12), Text = $"HEBIKAIO POKÉDEX 5E\nVersion {version}\n\nBased on Jerakin/Pokedex5E and the Pokémon 5e rules data.\n\nPokémon and related properties belong to their respective rights holders.\nThis fan application claims no ownership of Pokémon or Dungeons & Dragons." };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(8) };
        actions.Controls.Add(AppTheme.Button("Original Project", (_, _) => Process.Start(new ProcessStartInfo("https://github.com/Jerakin/Pokedex5E") { UseShellExecute = true })));
        actions.Controls.Add(AppTheme.Button("HebiKaio Repository", (_, _) => Process.Start(new ProcessStartInfo("https://github.com/callukcross/Hebi-Kaio-Pokedex") { UseShellExecute = true })));
        actions.Controls.Add(AppTheme.Button("Close", (_, _) => Close()));
        Controls.Add(text); Controls.Add(actions);
    }
}
