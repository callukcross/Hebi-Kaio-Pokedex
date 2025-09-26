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
        private Size originalFormSize;

        public ResizableForm()
        {
            this.Load += ResizableForm_Load;
            this.Resize += ResizableForm_Resize;
        }

        private void ResizableForm_Load(object sender, EventArgs e)
        {
            originalFormSize = this.ClientSize;
            foreach (Control ctrl in this.Controls)
            {
                originalLocations[ctrl] = ctrl.Location;
                originalSizes[ctrl] = ctrl.Size;
            }
        }

        private void resizeControl(Point originalLocation, Size originalSize, Control c)
        {
            float xRatio = (float)this.ClientSize.Width / originalFormSize.Width;
            float yRatio = (float)this.ClientSize.Height / originalFormSize.Height;

            int newX = (int)(originalLocation.X * xRatio);
            int newY = (int)(originalLocation.Y * yRatio);

            int newWidth = (int)(originalSize.Width * xRatio);
            int newHeight = (int)(originalSize.Height * yRatio);

            c.Location = new Point(newX, newY);
            c.Size = new Size(newWidth, newHeight);
        }

        private void ResizableForm_Resize(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (originalLocations.ContainsKey(ctrl) && originalSizes.ContainsKey(ctrl))
                {
                    resizeControl(originalLocations[ctrl], originalSizes[ctrl], ctrl);
                }
            }
        }
    }
}