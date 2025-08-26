using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainMenu1 : Form
    {
        private Dictionary<Control, Point> originalLocations = new Dictionary<Control, Point>();
        private Dictionary<Control, Size> originalSizes = new Dictionary<Control, Size>();
        private Size originalFormSize;

        public MainMenu1()
        {
            InitializeComponent();
            this.Load += MainMenu1_Load;
            this.Resize += MainMenu1_Resize;
        }

        private void MainMenu1_Load(object sender, EventArgs e)
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

        private void MainMenu1_Resize(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (originalLocations.ContainsKey(ctrl) && originalSizes.ContainsKey(ctrl))
                {
                    resizeControl(originalLocations[ctrl], originalSizes[ctrl], ctrl);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e) // Create Profile button click event
        {

        }
        private void button2_Click(object sender, EventArgs e) // Load Profile button click event
        {

        }
        private void button3_Click(object sender, EventArgs e) // Edit Profile button click event
        {

        }
    }
}
