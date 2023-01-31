
namespace HOKM
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.titleScreen1 = new HOKM.Screens.TitleScreen();
            this.firstFiveScreen1 = new HOKM.Screens.FirstFiveScreen();
            this.gameScreen1 = new HOKM.Screens.GameScreen();
            this.endScreen1 = new HOKM.Screens.EndScreen();
            this.SuspendLayout();
            // 
            // titleScreen1
            // 
            this.titleScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titleScreen1.Location = new System.Drawing.Point(0, 0);
            this.titleScreen1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.titleScreen1.MinimumSize = new System.Drawing.Size(862, 609);
            this.titleScreen1.Name = "titleScreen1";
            this.titleScreen1.Size = new System.Drawing.Size(862, 609);
            this.titleScreen1.TabIndex = 2;
            // 
            // firstFiveScreen1
            // 
            this.firstFiveScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.firstFiveScreen1.Location = new System.Drawing.Point(0, 0);
            this.firstFiveScreen1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.firstFiveScreen1.MinimumSize = new System.Drawing.Size(862, 609);
            this.firstFiveScreen1.Name = "firstFiveScreen1";
            this.firstFiveScreen1.Size = new System.Drawing.Size(862, 609);
            this.firstFiveScreen1.TabIndex = 1;
            // 
            // gameScreen1
            // 
            this.gameScreen1.AutoScroll = true;
            this.gameScreen1.AutoSize = true;
            this.gameScreen1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gameScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameScreen1.Location = new System.Drawing.Point(0, 0);
            this.gameScreen1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gameScreen1.MinimumSize = new System.Drawing.Size(862, 609);
            this.gameScreen1.Name = "gameScreen1";
            this.gameScreen1.Size = new System.Drawing.Size(862, 609);
            this.gameScreen1.TabIndex = 0;
            // 
            // endScreen1
            // 
            this.endScreen1.BackColor = System.Drawing.Color.LightGreen;
            this.endScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.endScreen1.Location = new System.Drawing.Point(0, 0);
            this.endScreen1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.endScreen1.MinimumSize = new System.Drawing.Size(862, 609);
            this.endScreen1.Name = "endScreen1";
            this.endScreen1.Size = new System.Drawing.Size(862, 609);
            this.endScreen1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(850, 578);
            this.Controls.Add(this.titleScreen1);
            this.Controls.Add(this.firstFiveScreen1);
            this.Controls.Add(this.gameScreen1);
            this.Controls.Add(this.endScreen1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(866, 617);
            this.Name = "Form1";
            this.Text = "HOKM";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Screens.GameScreen gameScreen1;
        private Screens.FirstFiveScreen firstFiveScreen1;
        private Screens.TitleScreen titleScreen1;
        private Screens.EndScreen endScreen1;
    }
}

