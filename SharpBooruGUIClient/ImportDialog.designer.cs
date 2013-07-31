namespace TA.SharpBooru.Client.GUI
{
    partial class ImportDialog
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
            this.okButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.sharedTagsTagTextBox = new TA.SharpBooru.Client.GUI.Controls.TagTextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.allowedTagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.image_file = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tags = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.comment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.image = new System.Windows.Forms.DataGridViewImageColumn();
            this.buttonPasteClipboard = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_ok;
            this.okButton.Location = new System.Drawing.Point(782, 513);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(30, 30);
            this.okButton.TabIndex = 2;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.sharedTagsTagTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(800, 495);
            this.splitContainer1.SplitterDistance = 53;
            this.splitContainer1.TabIndex = 12;
            // 
            // sharedTagsTagTextBox
            // 
            this.sharedTagsTagTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sharedTagsTagTextBox.Location = new System.Drawing.Point(3, 3);
            this.sharedTagsTagTextBox.Multiline = true;
            this.sharedTagsTagTextBox.Name = "sharedTagsTagTextBox";
            this.sharedTagsTagTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sharedTagsTagTextBox.Size = new System.Drawing.Size(794, 47);
            this.sharedTagsTagTextBox.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.allowedTagsCheckedListBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dataGridView);
            this.splitContainer2.Size = new System.Drawing.Size(795, 432);
            this.splitContainer2.SplitterDistance = 124;
            this.splitContainer2.TabIndex = 5;
            // 
            // allowedTagsCheckedListBox
            // 
            this.allowedTagsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.allowedTagsCheckedListBox.FormattingEnabled = true;
            this.allowedTagsCheckedListBox.IntegralHeight = false;
            this.allowedTagsCheckedListBox.Location = new System.Drawing.Point(3, 3);
            this.allowedTagsCheckedListBox.Name = "allowedTagsCheckedListBox";
            this.allowedTagsCheckedListBox.Size = new System.Drawing.Size(118, 426);
            this.allowedTagsCheckedListBox.TabIndex = 4;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.image_file,
            this.tags,
            this.source,
            this.comment,
            this.rating,
            this.image});
            this.dataGridView.Location = new System.Drawing.Point(3, 3);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(661, 426);
            this.dataGridView.TabIndex = 3;
            // 
            // image_file
            // 
            this.image_file.HeaderText = "image_file";
            this.image_file.Name = "image_file";
            this.image_file.ReadOnly = true;
            // 
            // tags
            // 
            this.tags.HeaderText = "tags";
            this.tags.Name = "tags";
            // 
            // source
            // 
            this.source.HeaderText = "source";
            this.source.Name = "source";
            // 
            // comment
            // 
            this.comment.HeaderText = "comment";
            this.comment.Name = "comment";
            // 
            // rating
            // 
            this.rating.HeaderText = "rating";
            this.rating.Name = "rating";
            // 
            // image
            // 
            this.image.HeaderText = "image";
            this.image.Name = "image";
            this.image.ReadOnly = true;
            this.image.Width = 256;
            // 
            // buttonPasteClipboard
            // 
            this.buttonPasteClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPasteClipboard.Location = new System.Drawing.Point(12, 517);
            this.buttonPasteClipboard.Name = "buttonPasteClipboard";
            this.buttonPasteClipboard.Size = new System.Drawing.Size(164, 23);
            this.buttonPasteClipboard.TabIndex = 13;
            this.buttonPasteClipboard.Text = "Import URL from Clipboard";
            this.buttonPasteClipboard.UseVisualStyleBackColor = true;
            this.buttonPasteClipboard.Click += new System.EventHandler(this.buttonPasteClipboard_Click);
            // 
            // ImportDialog
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 552);
            this.Controls.Add(this.buttonPasteClipboard);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.okButton);
            this.MinimumSize = new System.Drawing.Size(580, 400);
            this.Name = "ImportDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MultiEditPostDialog_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MultiEditPostDialog_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private Controls.TagTextBox sharedTagsTagTextBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.CheckedListBox allowedTagsCheckedListBox;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn image_file;
        private System.Windows.Forms.DataGridViewTextBoxColumn tags;
        private System.Windows.Forms.DataGridViewTextBoxColumn source;
        private System.Windows.Forms.DataGridViewTextBoxColumn comment;
        private System.Windows.Forms.DataGridViewTextBoxColumn rating;
        private System.Windows.Forms.DataGridViewImageColumn image;
        private System.Windows.Forms.Button buttonPasteClipboard;
    }
}