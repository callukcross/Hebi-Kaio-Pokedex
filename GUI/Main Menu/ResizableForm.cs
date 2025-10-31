using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    public class ResizableForm : Form
    {
        private Dictionary<Control, Point> originalLocations = new Dictionary<Control, Point>();
        private Dictionary<Control, Size> originalSizes = new Dictionary<Control, Size>();
        private Dictionary<Control, float> originalFontSizes = new Dictionary<Control, float>();
        private Size originalFormSize;

        public ResizableForm()
        {
            // Wire events so derived forms don't need to.
            this.Load += ResizableForm_Load;
            this.Resize += ResizableForm_Resize;
        }

        private void ResizableForm_Load(object sender, EventArgs e)
        {
            // Capture the initial layout and font sizes once when the form first shows.
            originalFormSize = this.ClientSize;
            CaptureOriginalsRecursive(this);
        }

        private void CaptureOriginalsRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                // store only once (in case derived forms call Load multiple times)
                if (!originalLocations.ContainsKey(c))
                {
                    originalLocations[c] = c.Location;
                    originalSizes[c] = c.Size;
                    // store font size in points (fallback to form font if null)
                    originalFontSizes[c] = (c.Font != null) ? c.Font.Size : this.Font.Size;
                }

                if (c.HasChildren)
                    CaptureOriginalsRecursive(c);
            }
        }

        private void ResizableForm_Resize(object sender, EventArgs e)
        {
            if (originalFormSize.Width == 0 || originalFormSize.Height == 0)
                return;

            float xRatio = (float)this.ClientSize.Width / originalFormSize.Width;
            float yRatio = (float)this.ClientSize.Height / originalFormSize.Height;

            // Resize controls and fonts based on ratios
            foreach (var kv in originalLocations)
            {
                Control c = kv.Key;
                if (c == null || c.IsDisposed)
                    continue;

                var origLoc = kv.Value;
                var origSize = originalSizes.ContainsKey(c) ? originalSizes[c] : c.Size;

                // New size / location scaled independently for X/Y
                var newLocation = new Point((int)(origLoc.X * xRatio), (int)(origLoc.Y * yRatio));
                var newSize = new Size((int)(origSize.Width * xRatio), (int)(origSize.Height * yRatio));

                // Apply bounds safely
                c.SuspendLayout();
                c.Location = newLocation;
                c.Size = newSize;

                // Font scaling: use the smaller of x/y scale for readable aspect.
                // Respect Tag == "NoFontScale" to opt-out specific controls.
                if ((c.Tag == null || !string.Equals(c.Tag.ToString(), "NoFontScale", StringComparison.OrdinalIgnoreCase))
                    && originalFontSizes.ContainsKey(c)
                    && c.Font != null)
                {
                    float origFontSize = originalFontSizes[c];
                    float scale = Math.Min(xRatio, yRatio);
                    float newFontSize = Math.Max(6f, origFontSize * scale); // clamp to reasonable minimum
                    // Create a new Font with the same family/style but scaled size
                    c.Font = new Font(c.Font.FontFamily, newFontSize, c.Font.Style);
                }
                c.ResumeLayout();
            }
        }

        // Optional helper used previously in codebase; kept for compatibility.
        // Resize a single control using its stored original values.
        private void resizeControl(Point originalLocation, Size originalSize, Control c)
        {
            if (originalFormSize.Width == 0 || originalFormSize.Height == 0)
                return;

            float xRatio = (float)this.ClientSize.Width / originalFormSize.Width;
            float yRatio = (float)this.ClientSize.Height / originalFormSize.Height;

            c.Location = new Point((int)(originalLocation.X * xRatio), (int)(originalLocation.Y * yRatio));
            c.Size = new Size((int)(originalSize.Width * xRatio), (int)(originalSize.Height * yRatio));

            if ((c.Tag == null || !string.Equals(c.Tag.ToString(), "NoFontScale", StringComparison.OrdinalIgnoreCase))
                && originalFontSizes.ContainsKey(c)
                && c.Font != null)
            {
                float origFontSize = originalFontSizes[c];
                float scale = Math.Min(xRatio, yRatio);
                float newFontSize = Math.Max(6f, origFontSize * scale);
                c.Font = new Font(c.Font.FontFamily, newFontSize, c.Font.Style);
            }
        }
    }
}