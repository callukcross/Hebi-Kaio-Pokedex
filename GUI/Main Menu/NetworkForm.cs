using System.Drawing;
using System.Windows.Forms;

namespace GUI;

public sealed class NetworkForm : Form
{
    private readonly TextBox _address = new() { Text = "127.0.0.1", Width = 180 };
    private readonly NumericUpDown _port = new() { Minimum = 1024, Maximum = 65535, Value = 45835, Width = 100 };
    private readonly Label _status = new() { Text = "Not connected", AutoSize = true };

    public NetworkForm()
    {
        Text = "Local Network";
        var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(30) };
        content.Controls.Add(new Label { Text = "CONNECT WITH ANOTHER TRAINER", AutoSize = true, Font = new Font(SystemFonts.DefaultFont.FontFamily, 15, FontStyle.Bold) });
        content.Controls.Add(new Label { Text = "Direct local-network Pokémon transfer is being rebuilt for the desktop protocol. Profile file export/import remains available from Receive / Import.", AutoSize = true, MaximumSize = new Size(650, 0), Margin = new Padding(0, 20, 0, 20) });
        content.Controls.Add(new Label { Text = "Host address", AutoSize = true }); content.Controls.Add(_address);
        content.Controls.Add(new Label { Text = "Port", AutoSize = true }); content.Controls.Add(_port);
        content.Controls.Add(_status);
        content.Controls.Add(AppTheme.Button("Close", (_, _) => Close()));
        Controls.Add(content);
    }
}
