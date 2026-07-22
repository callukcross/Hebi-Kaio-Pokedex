using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI;

internal sealed class SplashForm : Form
{
    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None; StartPosition = FormStartPosition.CenterScreen; ClientSize = new Size(560, 300); BackColor = AppTheme.Surface;
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "development";
        Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font(SystemFonts.DefaultFont.FontFamily, 24, FontStyle.Bold), Text = $"HEBIKAIO\nPOKÉDEX 5E\n\nVersion {version}" });
        Shown += async (_, _) => await LaunchAsync();
    }

    private async Task LaunchAsync()
    {
        await Task.Delay(900); var main = new MainShellForm(); main.FormClosed += (_, _) => Close(); main.Show(); Hide();
    }
}
