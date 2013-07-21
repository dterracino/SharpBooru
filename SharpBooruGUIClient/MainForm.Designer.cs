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
            this.testFormButton = new System.Windows.Forms.Button();
            this.labelSearchBox = new System.Windows.Forms.Label();
            this.searchBox = new TA.SharpBooru.Client.GUI.nControls.TagTextBox();
            this.booruThumbView = new TA.SharpBooru.Client.GUI.nControls.BooruThumbView();
            this.SuspendLayout();
            // 
            // testFormButton
            // 
            this.testFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testFormButton.Location = new System.Drawing.Point(12, 312);
            this.testFormButton.Name = "testFormButton";
            this.testFormButton.Size = new System.Drawing.Size(100, 23);
            this.testFormButton.TabIndex = 3;
            this.testFormButton.Text = "TestForm";
            this.testFormButton.UseVisualStyleBackColor = true;
            this.testFormButton.Click += new System.EventHandler(this.button1_Click);
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
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(12, 31);
            this.searchBox.Name = "searchBox";
            this.searchBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.searchBox.Size = new System.Drawing.Size(100, 20);
            this.searchBox.TabIndex = 1;
            // 
            // booruThumbView
            // 
            this.booruThumbView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.booruThumbView.BackColor = System.Drawing.Color.Lavender;
            this.booruThumbView.Location = new System.Drawing.Point(118, 12);
            this.booruThumbView.Name = "booruThumbView";
            this.booruThumbView.Size = new System.Drawing.Size(459, 323);
            this.booruThumbView.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 347);
            this.Controls.Add(this.labelSearchBox);
            this.Controls.Add(this.testFormButton);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.booruThumbView);
            this.Name = "MainForm";
            this.Text = "SharpBooru GUI Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private nControls.BooruThumbView booruThumbView;
        private nControls.TagTextBox searchBox;
        private System.Windows.Forms.Button testFormButton;
        private System.Windows.Forms.Label labelSearchBox;

    }
}