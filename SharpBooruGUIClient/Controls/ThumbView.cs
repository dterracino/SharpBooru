using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public class ThumbView : FlowLayoutPanel
    {
        public delegate void ImageOpenedHandler(object sender, EventArgs e, object aObj);
        public event ImageOpenedHandler ImageOpened;

        //public Color? BorderColor = Color.Black;

        private ushort _ThumbnailSize = 192;

        public ushort ThumbnailSize
        {
            get { return _ThumbnailSize; }
            set
            {
                if (_ThumbnailSize < 1)
                    value = 1;
                if (value != _ThumbnailSize)
                {
                    _ThumbnailSize = value;
                    RefreshControls();
                }
            }
        }

        public ThumbView()
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.WrapContents = true;
            this.AutoScroll = true;
        }

        public void Add(Bitmap Thumbnail, object aObj)
        {
            if (!this.InvokeRequired)
            {
                SelectablePictureBox pBox = new SelectablePictureBox()
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(_ThumbnailSize, _ThumbnailSize)
                };
                if (Thumbnail != null)
                    pBox.Image = (Bitmap)Thumbnail.Clone();
                pBox.ImageOpened += (sender, e) =>
                    {
                        if (ImageOpened != null)
                            ImageOpened(sender, e, aObj);
                    };
                this.Controls.Add(pBox);
            }
            else Invoke(new Action<Bitmap, object>(Add), Thumbnail, aObj);
        }

        public void Clear()
        {
            if (!this.InvokeRequired)
            {
                foreach (Control childControl in this.Controls)
                {
                    if (childControl is PictureBox)
                        (childControl as PictureBox).Image.Dispose();
                    childControl.Dispose();
                }
                this.Controls.Clear();
            }
            else Invoke(new Action(Clear));
        }

        public void RefreshControls()
        {
            foreach (Control childControl in this.Controls)
                childControl.Size = new Size(_ThumbnailSize, _ThumbnailSize);
        }

        /*
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.BorderStyle == BorderStyle.FixedSingle
                && BorderColor.HasValue)
            {
                Rectangle borderRect = e.ClipRectangle;
                borderRect.Width--; borderRect.Height--;
                Pen borderPen = new Pen(BorderColor.Value, 1f);
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
        }
        */
    }
}
