namespace TA.SharpBooru.Client.GUI
{
    partial class ConnectDialog
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
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.labelServer = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonSearchLAN = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxServer
            // 
            this.textBoxServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServer.Location = new System.Drawing.Point(75, 12);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.Size = new System.Drawing.Size(157, 20);
            this.textBoxServer.TabIndex = 0;
            this.textBoxServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelServer
            // 
            this.labelServer.AutoSize = true;
            this.labelServer.Location = new System.Drawing.Point(12, 15);
            this.labelServer.Name = "labelServer";
            this.labelServer.Size = new System.Drawing.Size(38, 13);
            this.labelServer.TabIndex = 3;
            this.labelServer.Text = "Server";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Image = global::TA.SharpBooru.Client.GUI.Properties.Resources.icon_ok;
            this.buttonOK.Location = new System.Drawing.Point(202, 64);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(30, 30);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonSearchLAN
            // 
            this.buttonSearchLAN.Location = new System.Drawing.Point(12, 71);
            this.buttonSearchLAN.Name = "buttonSearchLAN";
            this.buttonSearchLAN.Size = new System.Drawing.Size(96, 23);
            this.buttonSearchLAN.TabIndex = 4;
            this.buttonSearchLAN.Text = "Search LAN";
            this.buttonSearchLAN.UseVisualStyleBackColor = true;
            this.buttonSearchLAN.Click += new System.EventHandler(this.buttonSearchLAN_Click);
            // 
            // ConnectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 102);
            this.Controls.Add(this.buttonSearchLAN);
            this.Controls.Add(this.labelServer);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ConnectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.Button buttonSearchLAN;
    }
}