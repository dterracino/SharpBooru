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
            this.labelSearchBox = new System.Windows.Forms.Label();
            this.buttonImportDialog = new System.Windows.Forms.Button();
            this.searchBox = new TA.SharpBooru.Client.GUI.Controls.TagTextBox();
            this.booruThumbView = new TA.SharpBooru.Client.GUI.Controls.BooruThumbView();
            this.buttonChangeUser = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelSearchBox
            // 
            this.labelSearchBox.Location = new System.Drawing.Point(12, 15);
            this.labelSearchBox.Name = "labelSearchBox";
            this.labelSearchBox.Size = new System.Drawing.Size(100, 13);
            this.labelSearchBox.TabIndex = 4;
            this.labelSearchBox.Text = "Tag search";
            this.labelSearchBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonImportDialog
            // 
            this.buttonImportDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImportDialog.Location = new System.Drawing.Point(12, 312);
            this.buttonImportDialog.Name = "buttonImportDialog";
            this.buttonImportDialog.Size = new System.Drawing.Size(100, 23);
            this.buttonImportDialog.TabIndex = 3;
            this.buttonImportDialog.Text = "Import";
            this.buttonImportDialog.UseVisualStyleBackColor = true;
            this.buttonImportDialog.Click += new System.EventHandler(this.buttonImportDialog_Click);
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(12, 31);
            this.searchBox.Name = "searchBox";
            this.searchBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchBox.Size = new System.Drawing.Size(100, 20);
            this.searchBox.TabIndex = 0;
            // 
            // booruThumbView
            // 
            this.booruThumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.booruThumbView.BackColor = System.Drawing.Color.SteelBlue;
            this.booruThumbView.LabelForeColor = System.Drawing.Color.White;
            this.booruThumbView.Location = new System.Drawing.Point(118, 12);
            this.booruThumbView.Name = "booruThumbView";
            this.booruThumbView.Size = new System.Drawing.Size(459, 323);
            this.booruThumbView.TabIndex = 4;
            this.booruThumbView.ThumbViewBackColor = System.Drawing.Color.White;
            // 
            // buttonChangeUser
            // 
            this.buttonChangeUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonChangeUser.Location = new System.Drawing.Point(12, 283);
            this.buttonChangeUser.Name = "buttonChangeUser";
            this.buttonChangeUser.Size = new System.Drawing.Size(100, 23);
            this.buttonChangeUser.TabIndex = 2;
            this.buttonChangeUser.Text = "Change User";
            this.buttonChangeUser.UseVisualStyleBackColor = true;
            this.buttonChangeUser.Click += new System.EventHandler(this.buttonChangeUser_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefresh.Location = new System.Drawing.Point(12, 254);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(100, 23);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 347);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonChangeUser);
            this.Controls.Add(this.buttonImportDialog);
            this.Controls.Add(this.labelSearchBox);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.booruThumbView);
            this.Name = "MainForm";
            this.Text = "SharpBooru GUI Client";
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

    }
}