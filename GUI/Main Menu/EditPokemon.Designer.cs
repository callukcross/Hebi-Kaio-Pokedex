namespace GUI
{
    partial class EditPokemon
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
            this.shinyButton = new System.Windows.Forms.Button();
            this.maleButton = new System.Windows.Forms.Button();
            this.femaleButton = new System.Windows.Forms.Button();
            this.pokemonSpriteImg = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pokemonSpriteImg)).BeginInit();
            this.SuspendLayout();
            // 
            // shinyButton
            // 
            this.shinyButton.Location = new System.Drawing.Point(555, 12);
            this.shinyButton.Name = "shinyButton";
            this.shinyButton.Size = new System.Drawing.Size(100, 45);
            this.shinyButton.TabIndex = 0;
            this.shinyButton.Text = "Shiny?";
            this.shinyButton.UseVisualStyleBackColor = true;
            // 
            // maleButton
            // 
            this.maleButton.Location = new System.Drawing.Point(555, 63);
            this.maleButton.Name = "maleButton";
            this.maleButton.Size = new System.Drawing.Size(45, 45);
            this.maleButton.TabIndex = 1;
            this.maleButton.Text = "M";
            this.maleButton.UseVisualStyleBackColor = true;
            // 
            // femaleButton
            // 
            this.femaleButton.Location = new System.Drawing.Point(610, 63);
            this.femaleButton.Name = "femaleButton";
            this.femaleButton.Size = new System.Drawing.Size(45, 45);
            this.femaleButton.TabIndex = 2;
            this.femaleButton.Text = "F";
            this.femaleButton.UseVisualStyleBackColor = true;
            // 
            // pokemonSpriteImg
            // 
            this.pokemonSpriteImg.Location = new System.Drawing.Point(12, 12);
            this.pokemonSpriteImg.Name = "pokemonSpriteImg";
            this.pokemonSpriteImg.Size = new System.Drawing.Size(45, 45);
            this.pokemonSpriteImg.TabIndex = 3;
            this.pokemonSpriteImg.TabStop = false;
            // 
            // EditPokemon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 912);
            this.Controls.Add(this.pokemonSpriteImg);
            this.Controls.Add(this.femaleButton);
            this.Controls.Add(this.maleButton);
            this.Controls.Add(this.shinyButton);
            this.Name = "EditPokemon";
            this.Text = "EditPokemon";
            ((System.ComponentModel.ISupportInitialize)(this.pokemonSpriteImg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button shinyButton;
        private System.Windows.Forms.Button maleButton;
        private System.Windows.Forms.Button femaleButton;
        private System.Windows.Forms.PictureBox pokemonSpriteImg;
    }
}