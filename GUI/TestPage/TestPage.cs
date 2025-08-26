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
    public partial class TestPage : Form
    {
        private Dictionary<Control, Point> originalLocations = new Dictionary<Control, Point>();
        private Dictionary<Control, Size> originalSizes = new Dictionary<Control, Size>();
        private Size originalFormSize;

        public TestPage()
        {
            InitializeComponent();
            this.Load += TestPage_Load;
        }

        private void TestPage_Load(object sender, EventArgs e)
        {
            originalFormSize = this.ClientSize;
            foreach (Control ctrl in this.Controls)
            {
                originalLocations[ctrl] = ctrl.Location;
                originalSizes[ctrl] = ctrl.Size;
            }
        }

        private void InitializeComponent()
        {
            this.testButton1 = new System.Windows.Forms.Button();
            this.testButton2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // testButton1
            // 
            this.testButton1.AutoSize = false;
            this.testButton1.Location = new System.Drawing.Point(116, 195);
            this.testButton1.Name = "testButton1";
            this.testButton1.Size = new System.Drawing.Size(145, 85);
            this.testButton1.TabIndex = 0;
            this.testButton1.Text = "button1";
            this.testButton1.UseVisualStyleBackColor = true;
            // 
            // testButton2
            // 
            this.testButton2.AutoSize = false;
            this.testButton2.Location = new System.Drawing.Point(155, 80);
            this.testButton2.Name = "testButton2";
            this.testButton2.Size = new System.Drawing.Size(75, 25);
            this.testButton2.TabIndex = 1;
            this.testButton2.Text = "button1";
            this.testButton2.UseVisualStyleBackColor = true;
            // 
            // TestPage
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(405, 503);
            this.Controls.Add(this.testButton2);
            this.Controls.Add(this.testButton1);
            this.Name = "TestPage";
            this.Resize += new System.EventHandler(this.TestPage_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

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

        private void TestPage_Resize(object sender, EventArgs e)
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
