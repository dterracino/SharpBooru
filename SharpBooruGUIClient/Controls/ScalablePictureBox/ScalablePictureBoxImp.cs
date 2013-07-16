using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// This is public domain software - that is, you can do whatever you want
/// with it, and include it software that is licensed under the GNU or the
/// BSD license, or whatever other licence you choose, including proprietary
/// closed source licenses.  I do ask that you leave this lcHeader in tact.

namespace TEAM_ALPHA.Controls
{
    public partial class ScalablePictureBoxImp : UserControl
    {
        // The PictureBox storing the image
        [Bindable(false)]
        public PictureBox PictureBox
        {
            get { return pictureBox; }
        }

        // Previous scale percentage for the picture box
        public int PreviousScalePercent { get; set; }

        // Keep record of the logic positions of the image corner points for fast reference and calculation
        private int leftX;
        public int LeftX { get { return leftX; } set { leftX = value; } }
        private int upperY;
        public int UpperY { get { return upperY; } set { upperY = value; } }
        private int rightX;
        public int RightX { get { return rightX; } set { rightX = value; } }
        private int lowerY;
        public int LowerY { get { return lowerY; } set { lowerY = value; } }

        // Keep record of the dimensions of original & scaled images for fast reference and calculation
        public int ScaledPictureWidth { get; set; }
        public int ScaledPictureHeight { get; set; }

        // A copy of the original picture
        private Image originalPicture;
        public Image OriginalPicture
        {
            get { return originalPicture; }
            set
            {
                originalPicture = value;

                if (value == null)
                    return;

                LeftX = 0;
                UpperY = 0;
                RightX = value.Width;
                LowerY = value.Height;
            }
        }

        // Scale percentage of picture box in zoom mode
        private int currentScalePercent = Common.ORIGINALSCALEPERCENT;
        // Scale percentage for the picture box
        public int CurrentScalePercent
        {
            get { return currentScalePercent; }
            set
            {
                // No image or the scale remains the same, no need to redraw
                if (PictureBox.Image == null || PreviousScalePercent == value)
                    return;

                // Calculate the minimum and maximum scale percentages allowed by predefined values for the
                // width and height respectively to make sure neither of them exceeds the preset upper and lower scale limits
                int minScalePercent = (int)(100 * Math.Max((float)Common.PICTUREWIDTHMIN / (float)OriginalPicture.Width,
                  (float)Common.PICTUREHEIGHTMIN / (float)OriginalPicture.Height));
                int maxScalePercent = (int)(100 * Math.Min((float)Common.PICTUREWIDTHMAX / (float)OriginalPicture.Width,
                  (float)Common.PICTUREHEIGHTMAX / (float)OriginalPicture.Height));

                // Set the previous scale percent and the current one
                PreviousScalePercent = CurrentScalePercent;
                currentScalePercent = Math.Max(Math.Min(value, maxScalePercent), minScalePercent);

                // Set the parent control scale percent to pass the value to the PictureTracker
                scalablePictureBoxParent.ScalePercent = CurrentScalePercent;

                // Calculate the scaled picture dimensions
                ScaledPictureWidth = (int)(OriginalPicture.Width * (float)currentScalePercent / (float)Common.ORIGINALSCALEPERCENT);
                ScaledPictureHeight = (int)(OriginalPicture.Height * (float)currentScalePercent / (float)Common.ORIGINALSCALEPERCENT);

                // Redraw scaled picture 
                ReDrawPicture();
            }
        }

        // The referenece to the parent ScalablePictureBox
        public ScalablePictureBox scalablePictureBoxParent { get; set; }

        // The zoom rate
        private float zoomRate = 0;
        private float ZoomRate
        {
            get
            {
                return zoomRate;
            }
            set
            {
                if (value > 0)
                    ZoomInPicture();
                else
                    ZoomOutPicture();
            }
        }

        //  Zoom in the picture
        private void ZoomInPicture()
        {
            // Make sure the zoom in ratio is noticeable but also within reasonable range
            PreviousScalePercent = CurrentScalePercent;
            CurrentScalePercent = Math.Max(PreviousScalePercent + 10, (int)(Common.ZOOMINRATIO * CurrentScalePercent));
        }

        //  Zoom out the picture
        private void ZoomOutPicture()
        {
            // Make sure the zoom out ratio is noticeable but also within reasonable range
            PreviousScalePercent = CurrentScalePercent;
            CurrentScalePercent = Math.Min(PreviousScalePercent - 10, (int)(Common.ZOOMOUTRATIO * CurrentScalePercent));
        }

        // The flag to stop the pictureBox_MouseWheel event handler being triggered more than once during one scrolling
        private bool mouseWheelEventHandled = true;

        public void pictureBox_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Check the flag. Early out if the flag is true
            if (!mouseWheelEventHandled)
                return;

            ZoomRate = e.Delta / 120;
            // Set the flag to false to mark the mouse wheel event has not been handled
            mouseWheelEventHandled = false;
        }

        public void pictureBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Mouse button up means the wheel scrolling has finished - Set the flag to false
            mouseWheelEventHandled = true;
        }

        // Image in picture box
        [Bindable(true)]
        public Image Picture
        {
            get
            {
                return pictureBox.Image;
            }
            set
            {
                // Dispose the previous pictureBox.Image if it exists
                //if (pictureBox.Image != null)
                //    pictureBox.Image.Dispose();

                // Set the value to pictureBox.Image
                pictureBox.Image = value;

                if (value == null)
                    return;

                // Set the dimensions of the pictureBox
                pictureBox.Width = value.Width;
                pictureBox.Height = value.Height;

                // Refresh the pop-up menu
                RefreshContextMenuStrip();
            }
        }

        /// <summary>
        /// delegate of PictureBox painted event handler
        /// </summary>
        /// <param name="visibleAreaRect">currently visible area of picture</param>
        /// <param name="pictureBoxRect">picture box area</param>
        public delegate void PictureBoxPaintedEventHandler(Rectangle visibleAreaRect, Rectangle pictureBoxRect);

        /// <summary>
        /// PictureBox painted event
        /// </summary>
        public event PictureBoxPaintedEventHandler PictureBoxPaintedEvent;

        // Raise pictureBox painted event for adjusting picture tracking control
        public void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (PictureBoxPaintedEvent != null)
            {
                Rectangle controlClientRect = ClientRectangle;
                controlClientRect.X -= AutoScrollPosition.X;
                controlClientRect.Y -= AutoScrollPosition.Y;
                PictureBoxPaintedEvent(controlClientRect, pictureBox.ClientRectangle);
            }
        }

        public ScalablePictureBoxImp()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Enable auto scroll of this control
            AutoScroll = true;

            // Hook up the event handler pictureBox_Paint, which prepare the data and 
            // raise the PictureBoxPainted event so the PictureTracker can catch the event
            pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(pictureBox_Paint);
        }

        /// <summary>
        /// scroll picture programmatically by the event from PictureTracker
        /// </summary>
        /// <param name="xMovementRate">horizontal scroll movement rate which may be nagtive value</param>
        /// <param name="yMovementRate">vertical scroll movement rate which may be nagtive value</param>
        public void pictureBox_ScrollPictureEvent(float xMovementRate, float yMovementRate)
        {
            // NOTICE : usage of Math.Abs(AutoScrollPosition.X) and Math.Abs(AutoScrollPosition.Y)
            // The get method of the Panel.AutoScrollPosition.X property and
            // the get method of the Panel.AutoScrollPosition.Y property return negative values.
            // However, positive values are required.
            // You can use the Math.Abs function to obtain a positive value from the Panel.AutoScrollPosition.X property and
            // the Panel.AutoScrollPosition.Y property
            int X = (int)(Math.Abs(AutoScrollPosition.X) + pictureBox.ClientRectangle.Width * xMovementRate);
            int Y = (int)(Math.Abs(AutoScrollPosition.Y) + pictureBox.ClientRectangle.Height * yMovementRate);
            AutoScrollPosition = new Point(X, Y);
        }

        /// <summary>
        /// Repaint picture box when its location changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox_LocationChanged(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        internal List<ZoomItem> ZoomList { get; set; }

        /// <summary>
        /// Initialize ZoomList
        /// </summary>
        private void InitialiseZoomList()
        {
            if (ZoomList == null)
                ZoomList = new List<ZoomItem>();
            else
                ZoomList.Clear();

            /// Fit width zoom rate is unknow at this stage since the image is NOT loaded yet.
            ZoomList.Add(new ZoomItem(Common.FITWIDTHMENUITEMNAME));
            /// Fit height zoom rate is unknow at this stage since the image is NOT loaded yet.
            ZoomList.Add(new ZoomItem(Common.FITHEIGHTMENUITEMNAME));
            // Add a separator
            ZoomList.Add(new ZoomItem("-"));

            ZoomList.Add(new ZoomItem(Common.MINSCALEPERCENT));

            for (int scale = 25; scale <= 150; scale += 25)
            {
                ZoomList.Add(new ZoomItem(scale));
            }

            for (int scale = 200; scale <= Common.MAXSCALEPERCENT; scale += 200)
            {
                ZoomList.Add(new ZoomItem(scale));
            }
        }

        // Refresh the pop-up menu - the menu of availabe zoom rates
        private void RefreshContextMenuStrip()
        {
            // Populate ZoomeList if it is empty
            if (ZoomList == null || ZoomList.Count <= 0)
                InitialiseZoomList();

            // Clear the previous menu and suspend layout
            pictureBoxContextMenuStrip.SuspendLayout();
            pictureBoxContextMenuStrip.Items.Clear();

            foreach (ZoomItem Item in ZoomList)
            {
                switch (Item.ZoomName)
                {
                    case Common.FITHEIGHTMENUITEMNAME:
                        // Add the menu item "scale to fit height"
                        int fitHeightScalePercent = FitHeightScalePercent;
                        Item.ZoomRate = fitHeightScalePercent;
                        ToolStripMenuItem fitHeightScaleMenuItem = CreateToolStripMenuItem(fitHeightScalePercent);
                        fitHeightScaleMenuItem.Name = Common.FITHEIGHTMENUITEMNAME;
                        fitHeightScaleMenuItem.Text = "Fit height(" + fitHeightScalePercent + "%)";
                        pictureBoxContextMenuStrip.Items.Add(fitHeightScaleMenuItem);
                        break;
                    case Common.FITWIDTHMENUITEMNAME:
                        // Add the menu item "scale to fit width"
                        int fitWidthScalePercent = FitWidthScalePercent;
                        Item.ZoomRate = fitWidthScalePercent;
                        ToolStripMenuItem fitWidthScaleMenuItem = CreateToolStripMenuItem(fitWidthScalePercent);
                        fitWidthScaleMenuItem.Name = Common.FITWIDTHMENUITEMNAME;
                        fitWidthScaleMenuItem.Text = "Fit width(" + fitWidthScalePercent + "%)";
                        pictureBoxContextMenuStrip.Items.Add(fitWidthScaleMenuItem);
                        break;
                    case "-":
                        ToolStripSeparator SeparatorItem = new ToolStripSeparator();
                        SeparatorItem.Tag = "-";
                        pictureBoxContextMenuStrip.Items.Add(SeparatorItem);
                        break;
                    default:
                        ToolStripMenuItem menuItem = CreateToolStripMenuItem(Item.ZoomRate);
                        pictureBoxContextMenuStrip.Items.Add(menuItem);
                        break;
                }
            }

            // Resume layout and set the pop-up menu 
            pictureBoxContextMenuStrip.ResumeLayout();
            ContextMenuStrip = pictureBoxContextMenuStrip;

            // Set the last selected menu item status to Checked
            CheckLastSelectedMenuItem();
        }

        /// <summary>
        /// Get fit height scale percent of current image
        /// </summary>
        /// <returns>fit height scale percent</returns>
        private int FitHeightScalePercent
        {
            get
            {
                if (Picture == null || OriginalPicture == null)
                    return Common.MAXSCALEPERCENT;

                return ClientSize.Height * 100 / OriginalPicture.Height;
            }
        }

        /// <summary>
        /// Get fit width scale percent of current image
        /// </summary>
        /// <returns>fit width scale percent</returns>
        private int FitWidthScalePercent
        {
            get
            {
                if (Picture == null || OriginalPicture == null)
                    return Common.MAXSCALEPERCENT;

                return ClientSize.Width * 100 / OriginalPicture.Width;
            }
        }

        /// <summary>
        /// Create a tool strip menu item with given scale percent
        /// </summary>
        /// <param name="scalePercent">the percentage to scale picture</param>
        /// <returns>a tool strip menu item</returns>
        private ToolStripMenuItem CreateToolStripMenuItem(int scalePercent)
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Name = scalePercent.ToString();
            toolStripMenuItem.Text = scalePercent.ToString() + "%";
            toolStripMenuItem.Tag = scalePercent;
            toolStripMenuItem.Click += new EventHandler(toolStripMenuItem_Click);
            return toolStripMenuItem;
        }

        public string lastSelectedMenuItemName = Common.ORIGINALSCALEPERCENT.ToString();

        /// <summary>
        /// Scale the picture box size with the scale percentage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem selectedMenuItem = sender as ToolStripMenuItem;

            if (selectedMenuItem.Name == Common.FITWIDTHMENUITEMNAME)
                CurrentScalePercent = FitWidthScalePercent;
            else if (selectedMenuItem.Name == Common.FITHEIGHTMENUITEMNAME)
                CurrentScalePercent = FitHeightScalePercent;
            else
                CurrentScalePercent = (int)selectedMenuItem.Tag;

            // Set the last selected menu item status to Checked
            lastSelectedMenuItemName = selectedMenuItem.Name;
            CheckLastSelectedMenuItem();
        }

        /// <summary>
        /// Set the last selected menu item status to Checked
        /// </summary>
        private void CheckLastSelectedMenuItem()
        {
            // Find the last selected menu item status
            for (int i = 0; i < pictureBoxContextMenuStrip.Items.Count; i++)
            {
                if (pictureBoxContextMenuStrip.Items[i].Tag.ToString() == "-")
                    continue;
                ToolStripMenuItem menuItem = (ToolStripMenuItem)pictureBoxContextMenuStrip.Items[i];
                menuItem.Checked = lastSelectedMenuItemName == menuItem.Name;
            }
        }

        // Zoom the image based on the cursor position.
        private int GetZoomedPicturePosition(ref int leftX, ref int upperY, ref int rightX, ref int lowerY)
        {
            // Do nothing if no scaling happens
            if (PreviousScalePercent == CurrentScalePercent)
                return Common.SUCCESSBUTNOTDONE;

            // Initialise the foreground's border values 
            leftX = 10;
            upperY = 10;
            rightX = ScaledPictureWidth - 20;
            lowerY = ScaledPictureHeight - 20;
            return Common.GENERALSUCCESS;
        }

        /// <summary>
        /// Generic function to fill BitMap region on screen or in PictureBox control
        /// </summary>
        /// <param name="bmap"></param>
        /// <param name="colour"></param>
        /// <param name="leftX"></param>
        /// <param name="upperY"></param>
        /// <param name="rightX"></param>
        /// <param name="lowerY"></param>
        /// <returns></returns>
        unsafe private bool FillBitMapRegion(ref Bitmap bmap, Color colour, int leftX, int upperY, int rightX, int lowerY)
        {
            if (leftX < 1 || upperY < 1 || rightX > bmap.Width || lowerY > bmap.Height ||
                leftX >= rightX || upperY >= lowerY)
                return true;

            leftX--; upperY--; rightX--; lowerY--;
            BitmapData bmd = bmap.LockBits(new Rectangle(leftX, upperY, rightX - leftX + 1, lowerY - upperY + 1),
                ImageLockMode.ReadOnly, bmap.PixelFormat);
            int PixelSize = 4;
            Overlay overlay = new Overlay();
            for (int y = 0; y < bmd.Height; y++)
            {
                byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);
                for (int x = 0; x < bmd.Width; x++)
                {
                    overlay.u32 = (uint)colour.ToArgb();
                    row[x * PixelSize] = overlay.u8_0;
                    row[x * PixelSize + 1] = overlay.u8_1;
                    row[x * PixelSize + 2] = overlay.u8_2;
                    row[x * PixelSize + 3] = overlay.u8_3;
                }
            }
            bmap.UnlockBits(bmd);

            return true;
        }

        /// <summary>
        /// Fill the side frame for the picture as background
        /// </summary>
        /// <param name="pictureWithFrame"></param>
        /// <param name="leftX"></param>
        /// <param name="upperY"></param>
        /// <param name="rightX"></param>
        /// <param name="lowerY"></param>
        /// <returns></returns>
        private bool FillBackground(out Bitmap pictureWithFrame, int leftX, int upperY, int rightX, int lowerY)
        {
            pictureWithFrame = new Bitmap(ScaledPictureWidth, ScaledPictureHeight);

            // Fill the two areas above & below the zoomed foreground in the background with default colour 
            if (!FillBitMapRegion(ref pictureWithFrame, Common.BGCOLOUR, 1, 1, ScaledPictureWidth, upperY))
                return false;

            if (!FillBitMapRegion(ref pictureWithFrame, Common.BGCOLOUR, 1, lowerY + 1, ScaledPictureWidth, ScaledPictureHeight))
                return false;

            // Fill the two areas on the left  & right side of the zoomed foreground in the background with default colour 
            if (!FillBitMapRegion(ref pictureWithFrame, Common.BGCOLOUR, 1, upperY + 1, leftX, lowerY + 1))
                return false;

            if (!FillBitMapRegion(ref pictureWithFrame, Common.BGCOLOUR, rightX, upperY + 1, ScaledPictureWidth, lowerY + 1))
                return false;

            return true;
        }

        /// <summary>
        /// Reraw the changed image in memory and assign its value to Picture
        /// </summary>
        /// <returns></returns>
        private bool ReDrawPicture()
        {
            Bitmap pictureWithFrame;

            // Get the four corner point coordinates of the scaled image 
            switch (GetZoomedPicturePosition(ref leftX, ref upperY, ref rightX, ref lowerY))
            {
                case Common.SUCCESSBUTNOTDONE:
                    return true;
                case Common.GENERALERROR:
                    return false;
                case Common.GENERALSUCCESS:
                    break;
                default:
                    throw new NotImplementedException();
            }

            // The background frames need to be filled every time the foreground is zoomed  
            if (!FillBackground(out pictureWithFrame, leftX, upperY, rightX, lowerY))
                return false;

            // Draw the zoomed image
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(pictureWithFrame);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImage(OriginalPicture, leftX, upperY, rightX, lowerY);

            // Set the picture dimensions
            PictureBox.Width = ScaledPictureWidth;
            PictureBox.Height = ScaledPictureHeight;

            // Set the picture
            Picture = pictureWithFrame;

            return true;
        }

        // Initiate ScalablePictureBoxImp
        private void ScalablePictureBoxImp_Load(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                OriginalPicture = new Bitmap(pictureBox.Image);
            }
        }
    }
}