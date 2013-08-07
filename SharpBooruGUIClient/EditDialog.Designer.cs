namespace TA.SharpBooru.Client.GUI
{
    partial class EditDialog
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
            this.textBoxRating = new System.Windows.Forms.NumericUpDown();
            this.labelRating = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxPrivate = new System.Windows.Forms.CheckBox();
            this.labelComment = new System.Windows.Forms.Label();
            this.labelSource = new System.Windows.Forms.Label();
            this.labelTags = new System.Windows.Forms.Label();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.textBoxComment = new System.Windows.Forms.TextBox();
            this.textBoxTags = new TA.SharpBooru.Client.GUI.Controls.TagTextBox();
            this.textBoxOwner = new System.Windows.Forms.TextBox();
            this.labelOwner = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRating)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxRating
            // 
            this.textBoxRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRating.Location = new System.Drawing.Point(352, 170);
            this.textBoxRating.Name = "textBoxRating";
            this.textBoxRating.Size = new System.Drawing.Size(63, 20);
            this.textBoxRating.TabIndex = 31;
            // 
            // labelRating
            // 
            this.labelRating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRating.AutoSize = true;
            this.labelRating.Location = new System.Drawing.Point(308, 172);
            this.labelRating.Name = "labelRating";
            this.labelRating.Size = new System.Drawing.Size(38, 13);
            this.labelRating.TabIndex = 30;
            this.labelRating.Text = "Rating";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_ok;
            this.buttonOK.Location = new System.Drawing.Point(385, 214);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(30, 30);
            this.buttonOK.TabIndex = 29;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // checkBoxPrivate
            // 
            this.checkBoxPrivate.AutoSize = true;
            this.checkBoxPrivate.Location = new System.Drawing.Point(108, 171);
            this.checkBoxPrivate.Name = "checkBoxPrivate";
            this.checkBoxPrivate.Size = new System.Drawing.Size(196, 17);
            this.checkBoxPrivate.TabIndex = 28;
            this.checkBoxPrivate.Text = "Private (only Owner/Admin can see)";
            this.checkBoxPrivate.UseVisualStyleBackColor = true;
            // 
            // labelComment
            // 
            this.labelComment.AutoSize = true;
            this.labelComment.Location = new System.Drawing.Point(12, 121);
            this.labelComment.Name = "labelComment";
            this.labelComment.Size = new System.Drawing.Size(51, 13);
            this.labelComment.TabIndex = 27;
            this.labelComment.Text = "Comment";
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(12, 95);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(41, 13);
            this.labelSource.TabIndex = 26;
            this.labelSource.Text = "Source";
            // 
            // labelTags
            // 
            this.labelTags.AutoSize = true;
            this.labelTags.Location = new System.Drawing.Point(12, 15);
            this.labelTags.Name = "labelTags";
            this.labelTags.Size = new System.Drawing.Size(31, 13);
            this.labelTags.TabIndex = 25;
            this.labelTags.Text = "Tags";
            // 
            // textBoxSource
            // 
            this.textBoxSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSource.Location = new System.Drawing.Point(108, 92);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(307, 20);
            this.textBoxSource.TabIndex = 24;
            // 
            // textBoxComment
            // 
            this.textBoxComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxComment.Location = new System.Drawing.Point(108, 118);
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(307, 20);
            this.textBoxComment.TabIndex = 23;
            // 
            // textBoxTags
            // 
            this.textBoxTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTags.Location = new System.Drawing.Point(108, 12);
            this.textBoxTags.Multiline = true;
            this.textBoxTags.Name = "textBoxTags";
            this.textBoxTags.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTags.Size = new System.Drawing.Size(307, 74);
            this.textBoxTags.TabIndex = 22;
            // 
            // textBoxOwner
            // 
            this.textBoxOwner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOwner.Location = new System.Drawing.Point(108, 144);
            this.textBoxOwner.Name = "textBoxOwner";
            this.textBoxOwner.Size = new System.Drawing.Size(307, 20);
            this.textBoxOwner.TabIndex = 32;
            // 
            // labelOwner
            // 
            this.labelOwner.AutoSize = true;
            this.labelOwner.Location = new System.Drawing.Point(12, 147);
            this.labelOwner.Name = "labelOwner";
            this.labelOwner.Size = new System.Drawing.Size(38, 13);
            this.labelOwner.TabIndex = 33;
            this.labelOwner.Text = "Owner";
            // 
            // EditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 256);
            this.Controls.Add(this.labelOwner);
            this.Controls.Add(this.textBoxOwner);
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
            this.Name = "EditDialog";
            this.Text = "EditDialog";
            ((System.ComponentModel.ISupportInitialize)(this.textBoxRating)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown textBoxRating;
        private System.Windows.Forms.Label labelRating;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxPrivate;
        private System.Windows.Forms.Label labelComment;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Label labelTags;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.TextBox textBoxComment;
        private Controls.TagTextBox textBoxTags;
        private System.Windows.Forms.TextBox textBoxOwner;
        private System.Windows.Forms.Label labelOwner;

    }
}