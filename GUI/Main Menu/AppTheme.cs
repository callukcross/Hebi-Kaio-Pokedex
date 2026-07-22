using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI;

internal static class AppTheme
{
    public static readonly Color Background = Color.FromArgb(31, 34, 39);
    public static readonly Color Surface = Color.FromArgb(47, 50, 56);
    public static readonly Color Accent = Color.FromArgb(190, 38, 53);
    public static readonly Color Text = Color.WhiteSmoke;

    public static Button Button(string text, EventHandler click)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Surface, ForeColor = Text };
        button.FlatAppearance.BorderColor = Color.FromArgb(80, 84, 92);
        button.Click += click;
        return button;
    }

    public static void StyleTree(Control root)
    {
        root.BackColor = Background;
        root.ForeColor = Text;
        foreach (Control child in root.Controls) StyleTree(child);
    }
}

internal static class Prompt
{
    public static string Show(IWin32Window owner, string title, string label, string value = "")
    {
        using var dialog = new Form { Text = title, ClientSize = new Size(420, 140), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        var text = new TextBox { Left = 18, Top = 48, Width = 382, Text = value };
        var okay = new Button { Text = "OK", Left = 220, Top = 92, Width = 85, DialogResult = DialogResult.OK };
        dialog.Controls.AddRange([new Label { Text = label, Left = 18, Top = 18, Width = 382 }, text, okay, new Button { Text = "Cancel", Left = 315, Top = 92, Width = 85, DialogResult = DialogResult.Cancel }]);
        dialog.AcceptButton = okay;
        return dialog.ShowDialog(owner) == DialogResult.OK ? text.Text : null;
    }
}
