namespace GUI
{
    partial class WindowTemplate
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
            this.loadProfButton = new System.Windows.Forms.Button();
            this.editProfButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // titleText
            // 
            this.titleText.Location = new System.Drawing.Point(490, 66);
            this.titleText.Name = "titleText";
            this.titleText.Size = new System.Drawing.Size(192, 22);
            this.titleText.TabIndex = 0;
            this.titleText.Text = "This is a test page.";
            this.titleText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.titleText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // createProfButton
            // 
            this.createProfButton.Location = new System.Drawing.Point(490, 143);
            this.createProfButton.Name = "createProfButton";
            this.createProfButton.Size = new System.Drawing.Size(192, 42);
            this.createProfButton.TabIndex = 1;
            this.createProfButton.Text = "Create Profile";
            this.createProfButton.UseVisualStyleBackColor = true;
            this.createProfButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // loadProfButton
            // 
            this.loadProfButton.Location = new System.Drawing.Point(490, 253);
            this.loadProfButton.Name = "loadProfButton";
            this.loadProfButton.Size = new System.Drawing.Size(192, 42);
            this.loadProfButton.TabIndex = 2;
            this.loadProfButton.Text = "Load Profile";
            this.loadProfButton.UseVisualStyleBackColor = true;
            this.loadProfButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // editProfButton
            // 
            this.editProfButton.Location = new System.Drawing.Point(490, 365);
            this.editProfButton.Name = "editProfButton";
            this.editProfButton.Size = new System.Drawing.Size(192, 42);
            this.editProfButton.TabIndex = 3;
            this.editProfButton.Text = "Edit Profile";
            this.editProfButton.UseVisualStyleBackColor = true;
            this.editProfButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // TestPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 633);
            this.Controls.Add(this.editProfButton);
            this.Controls.Add(this.loadProfButton);
            this.Controls.Add(this.createProfButton);
            this.Controls.Add(this.titleText);
            this.Name = "TestPage";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.TestPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox titleText;
        private System.Windows.Forms.Button createProfButton;
        private System.Windows.Forms.Button loadProfButton;
        private System.Windows.Forms.Button editProfButton;
    }
}

