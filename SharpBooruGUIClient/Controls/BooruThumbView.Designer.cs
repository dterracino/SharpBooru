namespace TA.SharpBooru.Client.GUI.Controls
{
    partial class BooruThumbView
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.pageLabel = new System.Windows.Forms.Label();
            this.ajaxLoading1 = new TA.SharpBooru.Client.GUI.Controls.AjaxLoading();
            this.pageSwitcher = new TA.SharpBooru.Client.GUI.Controls.PageSwitcher();
            this.thumbView = new TA.SharpBooru.Client.GUI.Controls.ThumbView();
            this.SuspendLayout();
            // 
            // pageLabel
            // 
            this.pageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pageLabel.BackColor = System.Drawing.Color.Transparent;
            this.pageLabel.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageLabel.Location = new System.Drawing.Point(109, 206);
            this.pageLabel.Name = "pageLabel";
            this.pageLabel.Size = new System.Drawing.Size(186, 25);
            this.pageLabel.TabIndex = 1;
            this.pageLabel.Text = "label1";
            this.pageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ajaxLoading1
            // 
            this.ajaxLoading1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ajaxLoading1.Location = new System.Drawing.Point(112, 206);
            this.ajaxLoading1.Name = "ajaxLoading1";
            this.ajaxLoading1.Size = new System.Drawing.Size(35, 25);
            this.ajaxLoading1.TabIndex = 0;
            this.ajaxLoading1.Text = "ajaxProgressIndicator1";
            this.ajaxLoading1.Value = 0D;
            this.ajaxLoading1.Visible = false;
            // 
            // pageSwitcher
            // 
            this.pageSwitcher.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pageSwitcher.CurrentPage = 0;
            this.pageSwitcher.Location = new System.Drawing.Point(4, 206);
            this.pageSwitcher.Name = "pageSwitcher";
            this.pageSwitcher.Pages = 0;
            this.pageSwitcher.Size = new System.Drawing.Size(99, 25);
            this.pageSwitcher.TabIndex = 2;
            this.pageSwitcher.Text = "pageSwitcher1";
            // 
            // thumbView
            // 
            this.thumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.thumbView.AutoScroll = true;
            this.thumbView.BackColor = System.Drawing.Color.White;
            this.thumbView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.thumbView.Location = new System.Drawing.Point(3, 3);
            this.thumbView.Name = "thumbView";
            this.thumbView.Size = new System.Drawing.Size(294, 200);
            this.thumbView.TabIndex = 0;
            this.thumbView.ThumbnailSize = ((ushort)(192));
            // 
            // BooruThumbView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.ajaxLoading1);
            this.Controls.Add(this.pageSwitcher);
            this.Controls.Add(this.pageLabel);
            this.Controls.Add(this.thumbView);
            this.Name = "BooruThumbView";
            this.Size = new System.Drawing.Size(300, 234);
            this.ResumeLayout(false);

        }

        #endregion

        private TA.SharpBooru.Client.GUI.Controls.ThumbView thumbView;
        private System.Windows.Forms.Label pageLabel;
        private PageSwitcher pageSwitcher;
        private AjaxLoading ajaxLoading1;
    }
}
