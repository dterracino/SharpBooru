using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public class ThumbView : FlowLayoutPanel
    {
        public delegate void ImageOpenedHandler(object sender, EventArgs e, object aObj);
        public event ImageOpenedHandler ImageOpened;

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
            SelectablePictureBox pBox = new SelectablePictureBox()
            {
                Image = Thumbnail,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(_ThumbnailSize, _ThumbnailSize)
            };
            pBox.ImageOpened += (sender, e) =>
                {
                    if (ImageOpened != null)
                        ImageOpened(sender, e, aObj);
                };
            this.Controls.Add(pBox);
        }

        public void Clear() 
        {
            foreach (Control childControl in this.Controls)
            {
                this.Controls.Remove(childControl);
                (childControl as PictureBox).Image.Dispose();
                childControl.Dispose();
            }
        }

        public void RefreshControls()
        {
            foreach (Control childControl in this.Controls)
                childControl.Size = new Size(_ThumbnailSize, _ThumbnailSize);
        }

        //TODO Remove method
        //public void Remove(Bitmap Thumbnail) { _Thumbnails.Remove(Thumbnail); }
    }
}
