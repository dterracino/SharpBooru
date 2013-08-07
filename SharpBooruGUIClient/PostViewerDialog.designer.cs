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
            this.imageBox = new Cyotek.Windows.Forms.ImageBox();
            this.tagList = new TA.SharpBooru.Client.GUI.Controls.TagList();
            this.buttonClone = new System.Windows.Forms.Button();
            this.buttonEditPost = new System.Windows.Forms.Button();
            this.buttonEditImage = new System.Windows.Forms.Button();
            this.buttonNextPost = new System.Windows.Forms.Button();
            this.buttonPreviousPost = new System.Windows.Forms.Button();
            this.buttonSetWallpaper = new System.Windows.Forms.Button();
            this.buttonSaveImage = new System.Windows.Forms.Button();
            this.buttonDeletePost = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // imageBox
            // 
            this.imageBox.AllowClickZoom = true;
            this.imageBox.AlwaysShowHScroll = true;
            this.imageBox.AlwaysShowVScroll = true;
            this.imageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imageBox.Location = new System.Drawing.Point(191, 12);
            this.imageBox.Name = "imageBox";
            this.imageBox.ShowPixelGrid = true;
            this.imageBox.Size = new System.Drawing.Size(361, 308);
            this.imageBox.TabIndex = 11;
            // 
            // tagList
            // 
            this.tagList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.tagList.BackColor = System.Drawing.SystemColors.Control;
            this.tagList.Location = new System.Drawing.Point(11, 12);
            this.tagList.Name = "tagList";
            this.tagList.Size = new System.Drawing.Size(174, 236);
            this.tagList.TabIndex = 0;
            // 
            // buttonClone
            // 
            this.buttonClone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClone.Enabled = false;
            this.buttonClone.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_clone;
            this.buttonClone.Location = new System.Drawing.Point(84, 290);
            this.buttonClone.Name = "buttonClone";
            this.buttonClone.Size = new System.Drawing.Size(30, 30);
            this.buttonClone.TabIndex = 10;
            this.buttonClone.UseVisualStyleBackColor = true;
            this.buttonClone.Click += new System.EventHandler(this.buttonClone_Click);
            // 
            // buttonEditPost
            // 
            this.buttonEditPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEditPost.Enabled = false;
            this.buttonEditPost.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_edit;
            this.buttonEditPost.Location = new System.Drawing.Point(12, 290);
            this.buttonEditPost.Name = "buttonEditPost";
            this.buttonEditPost.Size = new System.Drawing.Size(30, 30);
            this.buttonEditPost.TabIndex = 9;
            this.buttonEditPost.UseVisualStyleBackColor = true;
            this.buttonEditPost.Click += new System.EventHandler(this.buttonEditPost_Click);
            // 
            // buttonEditImage
            // 
            this.buttonEditImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEditImage.Enabled = false;
            this.buttonEditImage.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_photoshop;
            this.buttonEditImage.Location = new System.Drawing.Point(48, 290);
            this.buttonEditImage.Name = "buttonEditImage";
            this.buttonEditImage.Size = new System.Drawing.Size(30, 30);
            this.buttonEditImage.TabIndex = 4;
            this.buttonEditImage.UseVisualStyleBackColor = true;
            this.buttonEditImage.Click += new System.EventHandler(this.buttonEditImage_Click);
            // 
            // buttonNextPost
            // 
            this.buttonNextPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNextPost.Enabled = false;
            this.buttonNextPost.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_arrow_right;
            this.buttonNextPost.Location = new System.Drawing.Point(155, 290);
            this.buttonNextPost.Name = "buttonNextPost";
            this.buttonNextPost.Size = new System.Drawing.Size(30, 30);
            this.buttonNextPost.TabIndex = 6;
            this.buttonNextPost.UseVisualStyleBackColor = true;
            this.buttonNextPost.Click += new System.EventHandler(this.buttonNextPost_Click);
            // 
            // buttonPreviousPost
            // 
            this.buttonPreviousPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPreviousPost.Enabled = false;
            this.buttonPreviousPost.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_arrow_left;
            this.buttonPreviousPost.Location = new System.Drawing.Point(119, 290);
            this.buttonPreviousPost.Name = "buttonPreviousPost";
            this.buttonPreviousPost.Size = new System.Drawing.Size(30, 30);
            this.buttonPreviousPost.TabIndex = 5;
            this.buttonPreviousPost.UseVisualStyleBackColor = true;
            this.buttonPreviousPost.Click += new System.EventHandler(this.buttonPreviousPost_Click);
            // 
            // buttonSetWallpaper
            // 
            this.buttonSetWallpaper.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSetWallpaper.Enabled = false;
            this.buttonSetWallpaper.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_image;
            this.buttonSetWallpaper.Location = new System.Drawing.Point(48, 254);
            this.buttonSetWallpaper.Name = "buttonSetWallpaper";
            this.buttonSetWallpaper.Size = new System.Drawing.Size(30, 30);
            this.buttonSetWallpaper.TabIndex = 2;
            this.buttonSetWallpaper.UseVisualStyleBackColor = true;
            this.buttonSetWallpaper.Click += new System.EventHandler(this.buttonSetWallpaper_Click);
            // 
            // buttonSaveImage
            // 
            this.buttonSaveImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSaveImage.Enabled = false;
            this.buttonSaveImage.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_save;
            this.buttonSaveImage.Location = new System.Drawing.Point(12, 254);
            this.buttonSaveImage.Name = "buttonSaveImage";
            this.buttonSaveImage.Size = new System.Drawing.Size(30, 30);
            this.buttonSaveImage.TabIndex = 1;
            this.buttonSaveImage.UseVisualStyleBackColor = true;
            this.buttonSaveImage.Click += new System.EventHandler(this.buttonSaveImage_Click);
            // 
            // buttonDeletePost
            // 
            this.buttonDeletePost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDeletePost.Enabled = false;
            this.buttonDeletePost.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_delete;
            this.buttonDeletePost.Location = new System.Drawing.Point(84, 254);
            this.buttonDeletePost.Name = "buttonDeletePost";
            this.buttonDeletePost.Size = new System.Drawing.Size(30, 30);
            this.buttonDeletePost.TabIndex = 3;
            this.buttonDeletePost.UseVisualStyleBackColor = true;
            this.buttonDeletePost.Click += new System.EventHandler(this.buttonDeletePost_Click);
            // 
            // PostViewerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 332);
            this.Controls.Add(this.imageBox);
            this.Controls.Add(this.buttonClone);
            this.Controls.Add(this.buttonEditPost);
            this.Controls.Add(this.buttonEditImage);
            this.Controls.Add(this.buttonNextPost);
            this.Controls.Add(this.buttonPreviousPost);
            this.Controls.Add(this.buttonSetWallpaper);
            this.Controls.Add(this.buttonSaveImage);
            this.Controls.Add(this.buttonDeletePost);
            this.Controls.Add(this.tagList);
            this.MinimumSize = new System.Drawing.Size(580, 370);
            this.Name = "PostViewerDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Post Viewer";
            this.ResumeLayout(false);

        }

        #endregion

        private TagList tagList;
        private System.Windows.Forms.Button buttonDeletePost;
        private System.Windows.Forms.Button buttonSaveImage;
        private System.Windows.Forms.Button buttonSetWallpaper;
        private System.Windows.Forms.Button buttonPreviousPost;
        private System.Windows.Forms.Button buttonNextPost;
        private System.Windows.Forms.Button buttonEditImage;
        private System.Windows.Forms.Button buttonEditPost;
        private System.Windows.Forms.Button buttonClone;
        private Cyotek.Windows.Forms.ImageBox imageBox;
    }
}