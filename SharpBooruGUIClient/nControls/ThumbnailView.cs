using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public class ThumbnailView : FlowLayoutPanel
    {
        public delegate void ImageOpenedHandler(object sender, EventArgs e, object aObj);
        public event ImageOpenedHandler ImageOpened;

        public Size ThumbnailSize = new Size(100, 100);

        public ThumbnailView()
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
                Size = ThumbnailSize
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
                childControl.Size = ThumbnailSize;
        }

        //TODO Remove method
        //public void Remove(Bitmap Thumbnail) { _Thumbnails.Remove(Thumbnail); }
    }
}
