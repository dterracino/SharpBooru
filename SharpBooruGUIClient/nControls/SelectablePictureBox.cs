//Based on Hans Passant's answer
//http://stackoverflow.com/questions/2743174

using System;
using System.Drawing;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public class SelectablePictureBox : PictureBox
    {
        public event EventHandler ImageOpened;

        public SelectablePictureBox()
        {
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.Image = this.ErrorImage;
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseDown(e);
        }
        
        protected override void OnEnter(EventArgs e)
        {
            this.Invalidate();
            base.OnEnter(e);
        }
        
        protected override void OnLeave(EventArgs e)
        {
            this.Invalidate();
            base.OnLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.Focused)
            {
                Brush activeBrush = new SolidBrush(Color.FromArgb(150, this.BackColor));
                e.Graphics.FillRectangle(activeBrush, this.ClientRectangle);
            }
        }

        private void FireImageOpenedEvent(EventArgs e)
        {
            if (ImageOpened != null)
                ImageOpened(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
                FireImageOpenedEvent(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            FireImageOpenedEvent(e);
        }
    }
}