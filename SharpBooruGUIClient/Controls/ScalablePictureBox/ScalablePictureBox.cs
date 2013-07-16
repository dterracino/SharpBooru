using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

/// <summary>
/// This is public domain software - that is, you can do whatever you want
/// with it, and include it software that is licensed under the GNU or the
/// BSD license, or whatever other licence you choose, including proprietary
/// closed source licenses.  I do ask that you leave this lcHeader in tact.

namespace TEAM_ALPHA.Controls
{
    /// <summary>
    /// Front end control of the scrollable, zoomable and scalable picture box.
    /// It is a facade and mediator of ScalablePictureBoxImp control and PictureTracker control.
    /// An application should use this control for showing picture
    /// instead of using ScalablePictureBoxImp or PictureTracker control directly.
    /// </summary>
    public partial class ScalablePictureBox : UserControl
    {
        /// <summary>
        /// indicating mouse dragging mode of picture tracker control
        /// </summary>
        private bool isDraggingPictureTracker = false;

        /// <summary>
        /// last mouse position of mouse dragging
        /// </summary>
        Point lastMousePos;

        /// <summary>
        /// the new area where the picture tracker control to be dragged
        /// </summary>
        Rectangle draggingRectangle;

        /// <summary>
        /// Constructor
        /// </summary>
        public ScalablePictureBox()
        {
            InitializeComponent();

            // Assign value to scalablePictureBoxParent of the scalablePictureBoxImp control as the reference to parent control
            scalablePictureBoxImp.scalablePictureBoxParent = this;

            //pictureTracker.BringToFront();
            pictureTracker.Visible = false;

            // enable double buffering
            SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);

            // register event handler for events from ScalablePictureBox
            scalablePictureBoxImp.PictureBoxPaintedEvent += new ScalablePictureBoxImp.PictureBoxPaintedEventHandler(pictureTracker.OnPictureBoxPainted);
            scalablePictureBoxImp.MouseWheel += new MouseEventHandler(scalablePictureBoxImp.pictureBox_MouseWheel);
            scalablePictureBoxImp.MouseUp += new MouseEventHandler(scalablePictureBoxImp.pictureBox_MouseUp);
            // register event handler for events from PictureTracker
            pictureTracker.ScrollPictureEvent += new PictureTracker.ScrollPictureEventHandler(scalablePictureBoxImp.pictureBox_ScrollPictureEvent);
            this.MouseWheel += new MouseEventHandler(scalablePictureBoxImp.pictureBox_MouseWheel);
        }

        /// <summary>
        /// Set a picture to show in ScalablePictureBox and PictureTracker controls
        /// </summary>
        public Image Picture
        {
            set
            {
                if (value == null)
                    return;

                pictureTracker.Picture = new Bitmap(value);
                scalablePictureBoxImp.Picture = value;
                scalablePictureBoxImp.OriginalPicture = new Bitmap(value);

                // Set the width and height of ScalablePictureBox.PictureBox
                PictureBox.Width = value.Width;
                PictureBox.Height = value.Height;
            }
        }

        /// <summary>
        /// Get picture box control
        /// </summary>
        [Bindable(false)]
        public PictureBox PictureBox
        {
            get { return scalablePictureBoxImp.PictureBox; }
        }

        public float ScalePercent
        {
            set { pictureTracker.ScalePercent = value; }
        }

        /// <summary>
        /// Draw a reversible rectangle
        /// </summary>
        /// <param name="rect">rectangle to be drawn</param>
        private void DrawReversibleRect(Rectangle rect)
        {
            // Convert the location of rectangle to screen coordinates.
            rect.Location = PointToScreen(rect.Location);

            // Draw the reversible frame.
            ControlPaint.DrawReversibleFrame(rect, Color.Navy, FrameStyle.Thick);
        }

        /// <summary>
        /// begin to drag picture tracker control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureTracker_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingPictureTracker = true;    // Make a note that we are dragging picture tracker control

            // Store the last mouse poit for this rubber-band rectangle.
            lastMousePos.X = e.X;
            lastMousePos.Y = e.Y;

            // draw initial dragging rectangle
            draggingRectangle = pictureTracker.Bounds;
            DrawReversibleRect(draggingRectangle);
        }

        /// <summary>
        /// dragging picture tracker control in mouse dragging mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureTracker_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPictureTracker)
            {
                // caculating next candidate dragging rectangle
                Point newPos = new Point(draggingRectangle.Location.X + e.X - lastMousePos.X,
                                         draggingRectangle.Location.Y + e.Y - lastMousePos.Y);
                Rectangle newPictureTrackerArea = draggingRectangle;
                newPictureTrackerArea.Location = newPos;

                // saving current mouse position to be used for next dragging
                lastMousePos = new Point(e.X, e.Y);

                // dragging picture tracker only when the candidate dragging rectangle
                // is within this ScalablePictureBox control
                if (ClientRectangle.Contains(newPictureTrackerArea))
                {
                    // removing previous rubber-band frame
                    DrawReversibleRect(draggingRectangle);

                    // updating dragging rectangle
                    draggingRectangle = newPictureTrackerArea;

                    // drawing new rubber-band frame
                    DrawReversibleRect(draggingRectangle);
                }
            }
        }

        /// <summary>
        /// end dragging picture tracker control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureTracker_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingPictureTracker)
            {
                isDraggingPictureTracker = false;

                // erase dragging rectangle
                DrawReversibleRect(draggingRectangle);

                // move the picture tracker control to the new position
                pictureTracker.Location = draggingRectangle.Location;
            }
        }

        /// <summary>
        /// change zoom rate when the picturebox is repainted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            scalablePictureBoxImp.SetAutoScrollMargin(50, 50);
        }

        /// <summary>
        /// relocate picture box at bottom right corner when the control size changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            int x = ClientSize.Width - pictureTracker.Width - 20;
            int y = ClientSize.Height - pictureTracker.Height - 20;

            pictureTracker.Location = new Point(x, y);
        }
    }
}