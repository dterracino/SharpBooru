using TA.SharpBooru.Client.GUI.Controls;

namespace TA.SharpBooru.Client.GUI
{
    partial class PostViewerDialog
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.editPostButton = new System.Windows.Forms.Button();
            this.deletePostButton = new System.Windows.Forms.Button();
            this.saveImageButton = new System.Windows.Forms.Button();
            this.toggleFullscreenButton = new System.Windows.Forms.Button();
            this.setWallpaperButton = new System.Windows.Forms.Button();
            this.previosPostButton = new System.Windows.Forms.Button();
            this.nextPostButton = new System.Windows.Forms.Button();
            this.postScoreNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.postScoreLabel = new System.Windows.Forms.Label();
            this.editImageButton = new System.Windows.Forms.Button();
            this.scalablePictureBox = new TA.SharpBooru.Client.GUI.Controls.ScalablePictureBox();
            this.tagList = new TA.SharpBooru.Client.GUI.Controls.xTagList();
            ((System.ComponentModel.ISupportInitialize)(this.postScoreNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // editPostButton
            // 
            this.editPostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editPostButton.Enabled = false;
            this.editPostButton.Location = new System.Drawing.Point(11, 284);
            this.editPostButton.Name = "editPostButton";
            this.editPostButton.Size = new System.Drawing.Size(31, 30);
            this.editPostButton.TabIndex = 4;
            this.editPostButton.UseVisualStyleBackColor = true;
            this.editPostButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // deletePostButton
            // 
            this.deletePostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deletePostButton.Enabled = false;
            this.deletePostButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_delete;
            this.deletePostButton.Location = new System.Drawing.Point(48, 284);
            this.deletePostButton.Name = "deletePostButton";
            this.deletePostButton.Size = new System.Drawing.Size(30, 30);
            this.deletePostButton.TabIndex = 5;
            this.deletePostButton.UseVisualStyleBackColor = true;
            this.deletePostButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // saveImageButton
            // 
            this.saveImageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.saveImageButton.Enabled = false;
            this.saveImageButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_save;
            this.saveImageButton.Location = new System.Drawing.Point(12, 248);
            this.saveImageButton.Name = "saveImageButton";
            this.saveImageButton.Size = new System.Drawing.Size(30, 30);
            this.saveImageButton.TabIndex = 2;
            this.saveImageButton.UseVisualStyleBackColor = true;
            this.saveImageButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // toggleFullscreenButton
            // 
            this.toggleFullscreenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toggleFullscreenButton.Enabled = false;
            this.toggleFullscreenButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_fullscreen;
            this.toggleFullscreenButton.Location = new System.Drawing.Point(120, 248);
            this.toggleFullscreenButton.Name = "toggleFullscreenButton";
            this.toggleFullscreenButton.Size = new System.Drawing.Size(30, 30);
            this.toggleFullscreenButton.TabIndex = 8;
            this.toggleFullscreenButton.UseVisualStyleBackColor = true;
            this.toggleFullscreenButton.Click += new System.EventHandler(this.button4_Click);
            // 
            // setWallpaperButton
            // 
            this.setWallpaperButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.setWallpaperButton.Enabled = false;
            this.setWallpaperButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_image;
            this.setWallpaperButton.Location = new System.Drawing.Point(48, 248);
            this.setWallpaperButton.Name = "setWallpaperButton";
            this.setWallpaperButton.Size = new System.Drawing.Size(30, 30);
            this.setWallpaperButton.TabIndex = 3;
            this.setWallpaperButton.UseVisualStyleBackColor = true;
            this.setWallpaperButton.Click += new System.EventHandler(this.button5_Click);
            // 
            // previosPostButton
            // 
            this.previosPostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.previosPostButton.Enabled = false;
            this.previosPostButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_arrow_left;
            this.previosPostButton.Location = new System.Drawing.Point(84, 284);
            this.previosPostButton.Name = "previosPostButton";
            this.previosPostButton.Size = new System.Drawing.Size(30, 30);
            this.previosPostButton.TabIndex = 6;
            this.previosPostButton.UseVisualStyleBackColor = true;
            this.previosPostButton.Click += new System.EventHandler(this.previosPostButton_Click);
            // 
            // nextPostButton
            // 
            this.nextPostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nextPostButton.Enabled = false;
            this.nextPostButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_arrow_right;
            this.nextPostButton.Location = new System.Drawing.Point(120, 284);
            this.nextPostButton.Name = "nextPostButton";
            this.nextPostButton.Size = new System.Drawing.Size(30, 30);
            this.nextPostButton.TabIndex = 7;
            this.nextPostButton.UseVisualStyleBackColor = true;
            this.nextPostButton.Click += new System.EventHandler(this.nextPostButton_Click);
            // 
            // postScoreNumericUpDown
            // 
            this.postScoreNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.postScoreNumericUpDown.Enabled = false;
            this.postScoreNumericUpDown.Location = new System.Drawing.Point(84, 222);
            this.postScoreNumericUpDown.Name = "postScoreNumericUpDown";
            this.postScoreNumericUpDown.Size = new System.Drawing.Size(66, 20);
            this.postScoreNumericUpDown.TabIndex = 11;
            this.postScoreNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // postScoreLabel
            // 
            this.postScoreLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.postScoreLabel.AutoSize = true;
            this.postScoreLabel.Enabled = false;
            this.postScoreLabel.Location = new System.Drawing.Point(12, 224);
            this.postScoreLabel.Name = "postScoreLabel";
            this.postScoreLabel.Size = new System.Drawing.Size(35, 13);
            this.postScoreLabel.TabIndex = 12;
            this.postScoreLabel.Text = "Score";
            // 
            // editImageButton
            // 
            this.editImageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editImageButton.Enabled = false;
            this.editImageButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.logo_photoshop;
            this.editImageButton.Location = new System.Drawing.Point(84, 248);
            this.editImageButton.Name = "editImageButton";
            this.editImageButton.Size = new System.Drawing.Size(30, 30);
            this.editImageButton.TabIndex = 14;
            this.editImageButton.UseVisualStyleBackColor = true;
            this.editImageButton.Click += new System.EventHandler(this.editImageButton_Click);
            // 
            // scalablePictureBox
            // 
            this.scalablePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.scalablePictureBox.Location = new System.Drawing.Point(162, 12);
            this.scalablePictureBox.Name = "scalablePictureBox";
            this.scalablePictureBox.Size = new System.Drawing.Size(346, 302);
            this.scalablePictureBox.TabIndex = 0;
            // 
            // tagList
            // 
            this.tagList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.tagList.BackColor = System.Drawing.SystemColors.Control;
            this.tagList.Location = new System.Drawing.Point(11, 12);
            this.tagList.Name = "tagList";
            this.tagList.Size = new System.Drawing.Size(138, 177);
            this.tagList.TabIndex = 1;
            // 
            // PostViewerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 326);
            this.Controls.Add(this.editImageButton);
            this.Controls.Add(this.postScoreLabel);
            this.Controls.Add(this.postScoreNumericUpDown);
            this.Controls.Add(this.scalablePictureBox);
            this.Controls.Add(this.nextPostButton);
            this.Controls.Add(this.previosPostButton);
            this.Controls.Add(this.setWallpaperButton);
            this.Controls.Add(this.toggleFullscreenButton);
            this.Controls.Add(this.saveImageButton);
            this.Controls.Add(this.deletePostButton);
            this.Controls.Add(this.editPostButton);
            this.Controls.Add(this.tagList);
            this.MinimumSize = new System.Drawing.Size(536, 364);
            this.Name = "PostViewerDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Post Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.postScoreNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScalablePictureBox scalablePictureBox;
        private xTagList tagList;
        private System.Windows.Forms.Button editPostButton;
        private System.Windows.Forms.Button deletePostButton;
        private System.Windows.Forms.Button saveImageButton;
        private System.Windows.Forms.Button toggleFullscreenButton;
        private System.Windows.Forms.Button setWallpaperButton;
        private System.Windows.Forms.Button previosPostButton;
        private System.Windows.Forms.Button nextPostButton;
        private System.Windows.Forms.NumericUpDown postScoreNumericUpDown;
        private System.Windows.Forms.Label postScoreLabel;
        private System.Windows.Forms.Button editImageButton;
    }
}