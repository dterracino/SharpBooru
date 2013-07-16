namespace TA.SharpBooru.Client.GUI
{
    partial class testForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.thumbnailView1 = new TA.SharpBooru.Client.GUI.nControls.ThumbnailView();
            this.ajaxProgressIndicator1 = new TA.SharpBooru.Client.GUI.nControls.AjaxProgressIndicator();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(12, 240);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // thumbnailView1
            // 
            this.thumbnailView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.thumbnailView1.AutoScroll = true;
            this.thumbnailView1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.thumbnailView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.thumbnailView1.Location = new System.Drawing.Point(12, 12);
            this.thumbnailView1.Name = "thumbnailView1";
            this.thumbnailView1.Size = new System.Drawing.Size(209, 145);
            this.thumbnailView1.TabIndex = 0;
            // 
            // ajaxProgressIndicator1
            // 
            this.ajaxProgressIndicator1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ajaxProgressIndicator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ajaxProgressIndicator1.Location = new System.Drawing.Point(95, 163);
            this.ajaxProgressIndicator1.Name = "ajaxProgressIndicator1";
            this.ajaxProgressIndicator1.Size = new System.Drawing.Size(126, 100);
            this.ajaxProgressIndicator1.TabIndex = 2;
            this.ajaxProgressIndicator1.Value = 0.1D;
            // 
            // testForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(233, 275);
            this.Controls.Add(this.ajaxProgressIndicator1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.thumbnailView1);
            this.Name = "testForm";
            this.Text = "testForm";
            this.ResumeLayout(false);

        }

        #endregion

        private nControls.ThumbnailView thumbnailView1;
        private System.Windows.Forms.Button button1;
        private nControls.AjaxProgressIndicator ajaxProgressIndicator1;
    }
}