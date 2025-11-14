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
        private Dictionary<Control, float> lastAppliedFontSizes = new Dictionary<Control, float>();
        private Size originalFormSize;

        // Debounce timer to avoid doing expensive layout continuously while the user is actively resizing
        private Timer resizeTimer;
        private const int ResizeDebounceMs =60; // adjust as needed (60ms gives ~16fps updates)

        public ResizableForm()
        {
            // Wire events so derived forms don't need to.
            this.Load += ResizableForm_Load;

            // Use Resize event only to schedule a deferred resize apply
            this.Resize += ResizableForm_Resize;

            // Enable double buffering & optimized painting to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            // Prepare debounce timer
            resizeTimer = new Timer();
            resizeTimer.Interval = ResizeDebounceMs;
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        // Reduce flicker for complex forms by enabling WS_EX_COMPOSITED when supported.
        // Note: WS_EX_COMPOSITED can introduce a small input latency; keep it scoped to forms that need it.
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // WS_EX_COMPOSITED =0x02000000
                cp.ExStyle |=0x02000000;
                return cp;
            }
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
                    lastAppliedFontSizes[c] = originalFontSizes[c];
                }

                if (c.HasChildren)
                    CaptureOriginalsRecursive(c);
            }
        }

        private void ResizableForm_Resize(object sender, EventArgs e)
        {
            // Reset the debounce timer each time Resize fires so we only do the expensive work
            // when the user pauses resizing.
            if (originalFormSize.Width ==0 || originalFormSize.Height ==0)
                return;

            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            ApplyResize();
        }

        // The actual resize work moved into its own method so it can be debounced.
        private void ApplyResize()
        {
            if (originalFormSize.Width ==0 || originalFormSize.Height ==0)
                return;

            float xRatio = (float)this.ClientSize.Width / originalFormSize.Width;
            float yRatio = (float)this.ClientSize.Height / originalFormSize.Height;

            // Suspend layout of the form while we update many child controls.
            this.SuspendLayout();
            try
            {
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

                        // Only set a new Font if the size has meaningfully changed to avoid churn.
                        float lastSize = lastAppliedFontSizes.ContainsKey(c) ? lastAppliedFontSizes[c] :0f;
                        if (Math.Abs(newFontSize - lastSize) >0.2f)
                        {
                            try
                            {
                                c.Font = new Font(c.Font.FontFamily, newFontSize, c.Font.Style);
                                lastAppliedFontSizes[c] = newFontSize;
                            }
                            catch
                            {
                                // ignore font creation failures and continue
                            }
                        }
                    }
                    c.ResumeLayout();
                }
            }
            finally
            {
                this.ResumeLayout();
                // Force an immediate repaint so the UI updates without leaving black voids
                this.Invalidate(true);
                this.Update();
            }
        }

        // Optional helper used previously in codebase; kept for compatibility.
        // Resize a single control using its stored original values.
        private void resizeControl(Point originalLocation, Size originalSize, Control c)
        {
            if (originalFormSize.Width ==0 || originalFormSize.Height ==0)
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

                float lastSize = lastAppliedFontSizes.ContainsKey(c) ? lastAppliedFontSizes[c] :0f;
                if (Math.Abs(newFontSize - lastSize) >0.2f)
                {
                    try
                    {
                        c.Font = new Font(c.Font.FontFamily, newFontSize, c.Font.Style);
                        lastAppliedFontSizes[c] = newFontSize;
                    }
                    catch { }
                }
            }
        }
    }
}