namespace TA.SharpBooru.Client.GUI.Controls
{
    partial class EditPanel
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
            this.textBoxTags = new TA.SharpBooru.Client.GUI.Controls.TagTextBox();
            this.textBoxComment = new System.Windows.Forms.TextBox();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.labelTags = new System.Windows.Forms.Label();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelComment = new System.Windows.Forms.Label();
            this.checkBoxPrivate = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelRating = new System.Windows.Forms.Label();
            this.textBoxRating = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRating)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxTags
            // 
            this.textBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTags.Location = new System.Drawing.Point(95, 3);
            this.textBoxTags.Multiline = true;
            this.textBoxTags.Name = "textBoxTags";
            this.textBoxTags.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTags.Size = new System.Drawing.Size(260, 74);
            this.textBoxTags.TabIndex = 0;
            // 
            // textBoxComment
            // 
            this.textBoxComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxComment.Location = new System.Drawing.Point(95, 109);
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(260, 20);
            this.textBoxComment.TabIndex = 2;
            // 
            // textBoxSource
            // 
            this.textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSource.Location = new System.Drawing.Point(95, 83);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(260, 20);
            this.textBoxSource.TabIndex = 3;
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.Location = new System.Drawing.Point(3, 6);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(31, 13);
            this.labelTags.TabIndex = 4;
            this.labelTags.Text = "Tags";
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(3, 86);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(41, 13);
            this.labelSource.TabIndex = 5;
            this.labelSource.Text = "Source";
            // 
            // labelComment
            // 
            this.labelComment.AutoSize = true;
            this.labelComment.Location = new System.Drawing.Point(3, 112);
            this.labelComment.Name = "labelComment";
            this.labelComment.Size = new System.Drawing.Size(51, 13);
            this.labelComment.TabIndex = 6;
            this.labelComment.Text = "Comment";
            // 
            // checkBoxPrivate
            // 
            this.checkBoxPrivate.AutoSize = true;
            this.checkBoxPrivate.Location = new System.Drawing.Point(95, 136);
            this.checkBoxPrivate.Name = "checkBoxPrivate";
            this.checkBoxPrivate.Size = new System.Drawing.Size(59, 17);
            this.checkBoxPrivate.TabIndex = 7;
            this.checkBoxPrivate.Text = "Private";
            this.checkBoxPrivate.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_ok;
            this.buttonOK.Location = new System.Drawing.Point(325, 197);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(30, 30);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelRating
            // 
            this.labelRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRating.AutoSize = true;
            this.labelRating.Location = new System.Drawing.Point(248, 137);
            this.labelRating.Name = "labelRating";
            this.labelRating.Size = new System.Drawing.Size(38, 13);
            this.labelRating.TabIndex = 9;
            this.labelRating.Text = "Rating";
            // 
            // textBoxRating
            // 
            this.textBoxRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRating.Location = new System.Drawing.Point(292, 135);
            this.textBoxRating.Name = "textBoxRating";
            this.textBoxRating.Size = new System.Drawing.Size(63, 20);
            this.textBoxRating.TabIndex = 10;
            // 
            // EditPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxRating);
            this.Controls.Add(this.labelRating);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.checkBoxPrivate);
            this.Controls.Add(this.labelComment);
            this.Controls.Add(this.labelSource);
            this.Controls.Add(this.labelTags);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.textBoxComment);
            this.Controls.Add(this.textBoxTags);
            this.Name = "EditPanel";
            this.Size = new System.Drawing.Size(358, 230);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TagTextBox textBoxTags;
        private System.Windows.Forms.TextBox textBoxComment;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Label labelComment;
        private System.Windows.Forms.CheckBox checkBoxPrivate;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelRating;
        private System.Windows.Forms.NumericUpDown textBoxRating;
    }
}
