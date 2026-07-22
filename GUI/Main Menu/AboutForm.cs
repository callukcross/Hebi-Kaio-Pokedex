using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
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
        actions.Controls.Add(AppTheme.Button("Changelog", (_, _) => MessageBox.Show(this, "0.1.0\n\n• Restored the complete Pokedex5E rules catalog and desktop workflows.\n• Added HebiKaio trainer, inventory, content-module, transfer, and installer features.\n• Added battle tracking, encounter filters, evolution, clipboard sharing, and local-network transfer.", "HebiKaio Pokédex changelog")));
        actions.Controls.Add(AppTheme.Button("Copy Diagnostics", (_, _) => Clipboard.SetText(Diagnostics(version))));
        actions.Controls.Add(AppTheme.Button("Close", (_, _) => Close()));
        Controls.Add(text); Controls.Add(actions);
    }

    private static string Diagnostics(string version)
    {
        var text = new StringBuilder(); text.AppendLine($"HebiKaio Pokédex {version}"); text.AppendLine($"OS: {Environment.OSVersion}"); text.AppendLine($".NET: {Environment.Version}"); text.AppendLine($"64-bit process: {Environment.Is64BitProcess}"); return text.ToString();
    }
}
