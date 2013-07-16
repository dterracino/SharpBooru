using System.Windows.Forms;
using System.ComponentModel;

namespace TEAM_ALPHA.Controls
{
    partial class ScalablePictureBoxImp
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.pictureBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(14, 21);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(484, 355);
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            this.pictureBox.LocationChanged += new System.EventHandler(this.pictureBox_LocationChanged);
            // 
            // pictureBoxContextMenuStrip
            // 
            this.pictureBoxContextMenuStrip.Name = "pictureBoxContextMenuStrip";
            this.pictureBoxContextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // ScalablePictureBoxImp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = false;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pictureBox);
            this.Name = "ScalablePictureBoxImp";
            this.Size = new System.Drawing.Size(514, 390);
            this.Load += new System.EventHandler(this.ScalablePictureBoxImp_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox;
        private ContextMenuStrip pictureBoxContextMenuStrip;
        private IContainer components;
    }
}