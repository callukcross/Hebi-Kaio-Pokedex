namespace GUI
{
    partial class PokemonActivePartyScreen
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
            this.pokemonSpeciesNameText = new System.Windows.Forms.Label();
            this.pokemonNicknameTextbox = new System.Windows.Forms.TextBox();
            this.typeIcon1 = new System.Windows.Forms.PictureBox();
            this.typeIcon2 = new System.Windows.Forms.PictureBox();
            this.editAttributesButton = new System.Windows.Forms.Button();
            this.radialMenuButton = new System.Windows.Forms.Button();
            this.levelText = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.xpTotalText = new System.Windows.Forms.Label();
            this.pokemonPortraitImage = new System.Windows.Forms.PictureBox();
            this.expProgressBar = new System.Windows.Forms.ProgressBar();
            this.favoriteAttributes = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.typeIcon1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeIcon2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pokemonPortraitImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.favoriteAttributes)).BeginInit();
            this.SuspendLayout();
            // 
            // pokemonSpeciesNameText
            // 
            this.pokemonSpeciesNameText.AutoSize = true;
            this.pokemonSpeciesNameText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pokemonSpeciesNameText.Location = new System.Drawing.Point(177, 16);
            this.pokemonSpeciesNameText.Name = "pokemonSpeciesNameText";
            this.pokemonSpeciesNameText.Size = new System.Drawing.Size(126, 24);
            this.pokemonSpeciesNameText.TabIndex = 0;
            this.pokemonSpeciesNameText.Text = "speciesName";
            this.pokemonSpeciesNameText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pokemonNicknameTextbox
            // 
            this.pokemonNicknameTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pokemonNicknameTextbox.Location = new System.Drawing.Point(45, 13);
            this.pokemonNicknameTextbox.Name = "pokemonNicknameTextbox";
            this.pokemonNicknameTextbox.Size = new System.Drawing.Size(126, 29);
            this.pokemonNicknameTextbox.TabIndex = 1;
            this.pokemonNicknameTextbox.Text = "nickname";
            this.pokemonNicknameTextbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // typeIcon1
            // 
            this.typeIcon1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.typeIcon1.Location = new System.Drawing.Point(305, 8);
            this.typeIcon1.Name = "typeIcon1";
            this.typeIcon1.Size = new System.Drawing.Size(40, 40);
            this.typeIcon1.TabIndex = 2;
            this.typeIcon1.TabStop = false;
            // 
            // typeIcon2
            // 
            this.typeIcon2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.typeIcon2.Location = new System.Drawing.Point(351, 8);
            this.typeIcon2.Name = "typeIcon2";
            this.typeIcon2.Size = new System.Drawing.Size(40, 40);
            this.typeIcon2.TabIndex = 3;
            this.typeIcon2.TabStop = false;
            // 
            // editAttributesButton
            // 
            this.editAttributesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editAttributesButton.Location = new System.Drawing.Point(397, 8);
            this.editAttributesButton.Name = "editAttributesButton";
            this.editAttributesButton.Size = new System.Drawing.Size(75, 40);
            this.editAttributesButton.TabIndex = 4;
            this.editAttributesButton.Text = "Edit";
            this.editAttributesButton.UseVisualStyleBackColor = true;
            // 
            // radialMenuButton
            // 
            this.radialMenuButton.Location = new System.Drawing.Point(0, 8);
            this.radialMenuButton.Name = "radialMenuButton";
            this.radialMenuButton.Size = new System.Drawing.Size(40, 40);
            this.radialMenuButton.TabIndex = 5;
            this.radialMenuButton.Text = "radial";
            this.radialMenuButton.UseVisualStyleBackColor = true;
            // 
            // levelText
            // 
            this.levelText.AutoSize = true;
            this.levelText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.levelText.Location = new System.Drawing.Point(0, 65);
            this.levelText.Name = "levelText";
            this.levelText.Size = new System.Drawing.Size(72, 26);
            this.levelText.TabIndex = 6;
            this.levelText.Text = "Level ?";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 0;
            // 
            // xpTotalText
            // 
            this.xpTotalText.AutoSize = true;
            this.xpTotalText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xpTotalText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xpTotalText.Location = new System.Drawing.Point(73, 65);
            this.xpTotalText.Name = "xpTotalText";
            this.xpTotalText.Size = new System.Drawing.Size(136, 26);
            this.xpTotalText.TabIndex = 7;
            this.xpTotalText.Text = "EXP: ??? / ???";
            // 
            // pokemonPortraitImage
            // 
            this.pokemonPortraitImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pokemonPortraitImage.Location = new System.Drawing.Point(0, 94);
            this.pokemonPortraitImage.Name = "pokemonPortraitImage";
            this.pokemonPortraitImage.Size = new System.Drawing.Size(208, 208);
            this.pokemonPortraitImage.TabIndex = 8;
            this.pokemonPortraitImage.TabStop = false;
            // 
            // expProgressBar
            // 
            this.expProgressBar.Location = new System.Drawing.Point(73, 48);
            this.expProgressBar.Name = "expProgressBar";
            this.expProgressBar.Size = new System.Drawing.Size(136, 14);
            this.expProgressBar.TabIndex = 9;
            // 
            // favoriteAttributes
            // 
            this.favoriteAttributes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.favoriteAttributes.Location = new System.Drawing.Point(215, 54);
            this.favoriteAttributes.Name = "favoriteAttributes";
            this.favoriteAttributes.Size = new System.Drawing.Size(257, 248);
            this.favoriteAttributes.TabIndex = 10;
            this.favoriteAttributes.TabStop = false;
            // 
            // PokemonActivePartyScreen
            // 
            this.ClientSize = new System.Drawing.Size(484, 761);
            this.Controls.Add(this.favoriteAttributes);
            this.Controls.Add(this.expProgressBar);
            this.Controls.Add(this.pokemonPortraitImage);
            this.Controls.Add(this.xpTotalText);
            this.Controls.Add(this.levelText);
            this.Controls.Add(this.radialMenuButton);
            this.Controls.Add(this.editAttributesButton);
            this.Controls.Add(this.typeIcon2);
            this.Controls.Add(this.typeIcon1);
            this.Controls.Add(this.pokemonNicknameTextbox);
            this.Controls.Add(this.pokemonSpeciesNameText);
            this.Name = "PokemonActivePartyScreen";
            ((System.ComponentModel.ISupportInitialize)(this.typeIcon1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeIcon2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pokemonPortraitImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.favoriteAttributes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label pokemonSpeciesNameText;
        private System.Windows.Forms.TextBox pokemonNicknameTextbox;
        private System.Windows.Forms.PictureBox typeIcon1;
        private System.Windows.Forms.PictureBox typeIcon2;
        private System.Windows.Forms.Button editAttributesButton;
        private System.Windows.Forms.Button radialMenuButton;
        private System.Windows.Forms.Label levelText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label xpTotalText;
        private System.Windows.Forms.PictureBox pokemonPortraitImage;
        private System.Windows.Forms.ProgressBar expProgressBar;
        private System.Windows.Forms.PictureBox favoriteAttributes;
    }
}
