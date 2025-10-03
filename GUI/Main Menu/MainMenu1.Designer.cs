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
            this.createProfButton = new System.Windows.Forms.Button();
            this.testProfile1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // titleText
            // 
            this.titleText.Location = new System.Drawing.Point(318, 11);
            this.titleText.Margin = new System.Windows.Forms.Padding(2);
            this.titleText.Name = "titleText";
            this.titleText.Size = new System.Drawing.Size(150, 20);
            this.titleText.TabIndex = 0;
            this.titleText.Text = "HebiKaio Dex";
            this.titleText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.titleText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // createProfButton
            // 
            this.createProfButton.Location = new System.Drawing.Point(318, 400);
            this.createProfButton.Margin = new System.Windows.Forms.Padding(2);
            this.createProfButton.Name = "createProfButton";
            this.createProfButton.Size = new System.Drawing.Size(150, 35);
            this.createProfButton.TabIndex = 1;
            this.createProfButton.Text = "New Profile";
            this.createProfButton.UseVisualStyleBackColor = true;
            this.createProfButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // testProfile1
            // 
            this.testProfile1.Location = new System.Drawing.Point(318, 76);
            this.testProfile1.Name = "testProfile1";
            this.testProfile1.Size = new System.Drawing.Size(150, 35);
            this.testProfile1.TabIndex = 2;
            this.testProfile1.Text = "Test Profile";
            this.testProfile1.UseVisualStyleBackColor = true;
            // 
            // MainMenu1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.testProfile1);
            this.Controls.Add(this.createProfButton);
            this.Controls.Add(this.titleText);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainMenu1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox titleText;
        private System.Windows.Forms.Button createProfButton;
        private System.Windows.Forms.Button testProfile1;
    }
}

