//Based on Hans Passant's answer
//http://stackoverflow.com/questions/2743174

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public class SelectablePictureBox : PictureBox
    {
        public event EventHandler ImageOpened;

        public Color? Border = null;

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
            if (Border.HasValue)
                if (Border != Color.Transparent)
                {
                    Pen borderPen = new Pen(Border.Value, 3f);
                    Rectangle borderRect = new Rectangle(1, 1, this.ClientSize.Width - 3, this.ClientSize.Height - 3);
                    if (this.Focused)
                        borderPen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(borderPen, borderRect);
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