namespace TA.SharpBooru.Client.GUI
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelSearchBox = new System.Windows.Forms.Label();
            this.buttonImportDialog = new System.Windows.Forms.Button();
            this.buttonChangeUser = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.imageContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonImgSearch = new System.Windows.Forms.Button();
            this.searchBox = new TA.SharpBooru.Client.GUI.Controls.TagTextBox();
            this.booruThumbView = new TA.SharpBooru.Client.GUI.Controls.BooruThumbView();
            this.imageContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSearchBox
            // 
            this.labelSearchBox.Location = new System.Drawing.Point(12, 15);
            this.labelSearchBox.Name = "labelSearchBox";
            this.labelSearchBox.Size = new System.Drawing.Size(122, 13);
            this.labelSearchBox.TabIndex = 4;
            this.labelSearchBox.Text = "Tag search";
            this.labelSearchBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonImportDialog
            // 
            this.buttonImportDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImportDialog.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_import;
            this.buttonImportDialog.Location = new System.Drawing.Point(12, 277);
            this.buttonImportDialog.Name = "buttonImportDialog";
            this.buttonImportDialog.Size = new System.Drawing.Size(58, 58);
            this.buttonImportDialog.TabIndex = 3;
            this.buttonImportDialog.UseVisualStyleBackColor = true;
            this.buttonImportDialog.Click += new System.EventHandler(this.buttonImportDialog_Click);
            // 
            // buttonChangeUser
            // 
            this.buttonChangeUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonChangeUser.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_users;
            this.buttonChangeUser.Location = new System.Drawing.Point(76, 277);
            this.buttonChangeUser.Name = "buttonChangeUser";
            this.buttonChangeUser.Size = new System.Drawing.Size(58, 58);
            this.buttonChangeUser.TabIndex = 2;
            this.buttonChangeUser.UseVisualStyleBackColor = true;
            this.buttonChangeUser.Click += new System.EventHandler(this.buttonChangeUser_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefresh.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_refresh;
            this.buttonRefresh.Location = new System.Drawing.Point(76, 213);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(58, 58);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // imageContextMenuStrip
            // 
            this.imageContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.imageContextMenuStrip.Name = "imageContextMenuStrip";
            this.imageContextMenuStrip.Size = new System.Drawing.Size(108, 70);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // buttonImgSearch
            // 
            this.buttonImgSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImgSearch.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_imgsearch;
            this.buttonImgSearch.Location = new System.Drawing.Point(12, 213);
            this.buttonImgSearch.Name = "buttonImgSearch";
            this.buttonImgSearch.Size = new System.Drawing.Size(58, 58);
            this.buttonImgSearch.TabIndex = 5;
            this.buttonImgSearch.UseVisualStyleBackColor = true;
            this.buttonImgSearch.Click += new System.EventHandler(this.buttonImgSearch_Click);
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(12, 31);
            this.searchBox.Name = "searchBox";
            this.searchBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchBox.Size = new System.Drawing.Size(122, 20);
            this.searchBox.TabIndex = 0;
            // 
            // booruThumbView
            // 
            this.booruThumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.booruThumbView.BackColor = System.Drawing.Color.SteelBlue;
            this.booruThumbView.LabelForeColor = System.Drawing.Color.White;
            this.booruThumbView.Location = new System.Drawing.Point(140, 12);
            this.booruThumbView.Name = "booruThumbView";
            this.booruThumbView.Size = new System.Drawing.Size(437, 323);
            this.booruThumbView.TabIndex = 4;
            this.booruThumbView.ThumbViewBackColor = System.Drawing.Color.White;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 347);
            this.Controls.Add(this.buttonImportDialog);
            this.Controls.Add(this.labelSearchBox);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.buttonImgSearch);
            this.Controls.Add(this.booruThumbView);
            this.Controls.Add(this.buttonChangeUser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "SharpBooru GUI Client";
            this.imageContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.BooruThumbView booruThumbView;
        private Controls.TagTextBox searchBox;
        private System.Windows.Forms.Label labelSearchBox;
        private System.Windows.Forms.Button buttonImportDialog;
        private System.Windows.Forms.Button buttonChangeUser;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.ContextMenuStrip imageContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button buttonImgSearch;

    }
}