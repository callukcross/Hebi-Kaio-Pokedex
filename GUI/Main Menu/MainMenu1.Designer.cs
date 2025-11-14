namespace GUI
{
    partial class MainMenu1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.titleText = new System.Windows.Forms.TextBox();
            this.newProfile = new System.Windows.Forms.Button();
            this.testProfile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // titleText
            // 
            this.titleText.Location = new System.Drawing.Point(222, 11);
            this.titleText.Margin = new System.Windows.Forms.Padding(2);
            this.titleText.Name = "titleText";
            this.titleText.Size = new System.Drawing.Size(150, 20);
            this.titleText.TabIndex = 0;
            this.titleText.Text = "HebiKaio Dex";
            this.titleText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.titleText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // newProfile
            // 
            this.newProfile.Location = new System.Drawing.Point(238, 327);
            this.newProfile.Margin = new System.Windows.Forms.Padding(2);
            this.newProfile.Name = "newProfile";
            this.newProfile.Size = new System.Drawing.Size(112, 28);
            this.newProfile.TabIndex = 3;
            this.newProfile.Text = "New Profile";
            this.newProfile.UseVisualStyleBackColor = true;
            this.newProfile.Click += new System.EventHandler(this.newProfile_Click);
            // 
            // testProfile
            // 
            this.testProfile.Location = new System.Drawing.Point(238, 63);
            this.testProfile.Margin = new System.Windows.Forms.Padding(2);
            this.testProfile.Name = "testProfile";
            this.testProfile.Size = new System.Drawing.Size(112, 28);
            this.testProfile.TabIndex = 4;
            this.testProfile.Text = "Test Profile";
            this.testProfile.UseVisualStyleBackColor = true;
            this.testProfile.Click += new System.EventHandler(this.testProfile_Click);
            // 
            // MainMenu1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 912);
            this.Controls.Add(this.testProfile);
            this.Controls.Add(this.newProfile);
            this.Controls.Add(this.titleText);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainMenu1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox titleText;
        private System.Windows.Forms.Button newProfile;
        private System.Windows.Forms.Button testProfile;
    }
}

