namespace GUI
{
    partial class Profile
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
            this.profileSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // profileSelect
            // 
            this.profileSelect.Location = new System.Drawing.Point(318, 78);
            this.profileSelect.Name = "profileSelect";
            this.profileSelect.Size = new System.Drawing.Size(150, 35);
            this.profileSelect.TabIndex = 0;
            this.profileSelect.Text = "Profile Select";
            this.profileSelect.UseVisualStyleBackColor = true;
            this.profileSelect.Click += new System.EventHandler(this.profileSelect_Click);
            // 
            // Profile
            // 
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.profileSelect);
            this.Name = "Profile";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button profileSelect;
    }
}
