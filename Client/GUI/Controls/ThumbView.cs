using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public class ThumbView : FlowLayoutPanel
    {
        public delegate void ImageOpenedHandler(SelectablePictureBox sender, object aObj);
        public event ImageOpenedHandler ImageOpened;

        public delegate void ImageRightClickHandler(SelectablePictureBox sender, MouseEventArgs e, object aObj);
        public event ImageRightClickHandler ImageRightClick;

        private ushort _ThumbnailSize = 192;
        private ushort? _ColCount = null;
        private Dictionary<SelectablePictureBox, object> _aObjDict = new Dictionary<SelectablePictureBox, object>();

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

        public object SelectedObject
        {
            get
            {
                foreach (Control ctrl in this.Controls)
                    if (ctrl is SelectablePictureBox)
                        if (ctrl.Focused)
                            return _aObjDict[ctrl as SelectablePictureBox];
                throw new KeyNotFoundException("No image selected");
            }
        }

        public ThumbView()
        {
            this.FlowDirection = FlowDirection.LeftToRight;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.WrapContents = true;
            this.AutoScroll = true;
        }

        public void Add(Bitmap Thumbnail, object aObj, string ToolTipText = null, Color? Border = null)
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
                            ImageOpened(pBox, aObj);
                    };
                pBox.MouseClick += (sender, e) =>
                    {
                        if (e.Button == MouseButtons.Right)
                            if (ImageRightClick != null)
                                ImageRightClick(pBox, e, aObj);
                    };
                if (!string.IsNullOrWhiteSpace(ToolTipText))
                    GUIHelper.CreateToolTip(pBox, ToolTipText.Trim());
                pBox.Border = Border;
                _aObjDict.Add(pBox, aObj);
                this.Controls.Add(pBox);
            }
            else try { Invoke(new Action<Bitmap, object, string, Color?>(Add), Thumbnail, aObj, ToolTipText, Border); }
                catch (ObjectDisposedException) { }

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
                _aObjDict.Clear();
                this.Controls.Clear();
            }
            else Invoke(new Action(Clear));
        }

        public void RefreshControls()
        {
            foreach (Control childControl in this.Controls)
                childControl.Size = new Size(_ThumbnailSize, _ThumbnailSize);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            //TODO Implement arrow key navigation
            return base.ProcessDialogKey(keyData);
        }

        private ushort GetRowCount()
        {
            int rowCount = this.Controls.Count / GetColumnCount();
            if (this.Controls.Count % GetColumnCount() > 0)
                return (ushort)(rowCount + 1);
            else return (ushort)rowCount;
        }

        private ushort GetColumnCount()
        {
            if (!_ColCount.HasValue)
            {
                List<int> xCoords = new List<int>();
                foreach (Control ctrl in this.Controls)
                    if (ctrl is PictureBox)
                        if (!xCoords.Contains(ctrl.Location.X))
                            xCoords.Add(ctrl.Location.X);
                _ColCount = (ushort)xCoords.Count;
            }
            return _ColCount.Value;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            _ColCount = null;
            base.OnResize(eventargs);
        }
    }
}
