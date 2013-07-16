using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public partial class ThumbnailView : UserControl
    {
        private List<BooruPost> _Posts = new List<BooruPost>();
        private int _pageCount = 1, _currentPage = 0;
        private ContextMenuStrip _contextMenu;
        private bool _LoadThreadRunning = false;
        private int _ThumbsPerPage = 30, _ThumbsSize = 180;
        private bool _DragMouseDown = false;

        /// <summary>This event will be fired if thumbnail is opened</summary>
        public event EventHandler PostOpened;

        /// <summary>This event will be fired if the loading is finished</summary>
        public event EventHandler LoadingFinished;

        /// <summary>The post which is currently selected</summary>
        public List<BooruPost> SelectedPosts
        {
            get
            {
                List<BooruPost> result = new List<BooruPost>();
                for (int i = 0; i < listView1.SelectedIndices.Count; i++)
                    result.Add(_Posts[CurrentPage * ThumbsPerPage + listView1.SelectedIndices[i]]);
                return result;
            }
        }

        public new ContextMenuStrip ContextMenu
        {
            get { return _contextMenu; }
            set { _contextMenu = value; }
        }

        public Label StatusLabel { get { return label1; } }

        /// <summary>The total page count</summary>
        public int PageCount
        {
            get { return _pageCount; }
            private set { _pageCount = value; }
        }

        public int ThumbsPerPage
        {
            get { return _ThumbsPerPage; }
            set { if (value > 0) _ThumbsPerPage = value; }
        }

        public int ThumbsSize
        {
            get { return _ThumbsSize; }
            set { if (value > 0)_ThumbsSize = value; }
        }

        /// <summary>The currently shown page</summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                if (!(value < 0) && value < PageCount)
                    if (!_LoadThreadRunning)
                    {
                        _currentPage = value;
                        Thread loadPageThread = new Thread((ThreadStart)(delegate
                            {
                                _LoadThreadRunning = true;
                                SetLabel();
                                MethodInvoker invoker = (MethodInvoker)delegate
                                {
                                    leftButton.Enabled = false;
                                    rightButton.Enabled = false;
                                    textBox1.Enabled = false;
                                    listView1.Items.Clear();
                                    pictureBox1.Visible = true;
                                    label2.Visible = true;
                                    label2.Text = "0%";
                                };
                                if (InvokeRequired) Invoke(invoker); else invoker();
                                invoker = (MethodInvoker)delegate
                                    {
                                        if (listView1.LargeImageList != null)
                                            using (ImageList il = listView1.LargeImageList)
                                            {
                                                il.Images.Clear();
                                                listView1.LargeImageList = null;
                                                il.Dispose();
                                            }
                                    };
                                if (listView1.InvokeRequired) listView1.Invoke(invoker); else invoker();
                                ImageList imglist = CreateImageList(ThumbsSize);
                                float imagesOnThisPage = (value < PageCount - 1 ? ThumbsPerPage : (Posts.Count - (PageCount - 1) * ThumbsPerPage)) - 1;
                                for (int i = value * ThumbsPerPage; i < ThumbsPerPage * (value + 1) && i < Posts.Count; i++)
                                {
                                    imglist.Images.Add(Posts[i].Thumbnail.Bitmap);
                                    float zeroBasedI = i - value * ThumbsPerPage; //TODO Should this really be inside a loop?
                                    string label2Text = string.Format("{0}%", (int)Math.Floor(100 * zeroBasedI / imagesOnThisPage + 0.5));
                                    invoker = (MethodInvoker)delegate { label2.Text = label2Text; };
                                    if (label2.InvokeRequired) label2.Invoke(invoker); else invoker(); //Until here
                                }
                                for (int i = value * ThumbsPerPage; i < ThumbsPerPage * (value + 1) && i < Posts.Count; i++)
                                {
                                    ListViewItem item = new ListViewItem();
                                    item.Text = string.Empty;
                                    item.ImageIndex = i - _currentPage * ThumbsPerPage;
                                    invoker = (MethodInvoker)delegate { listView1.Items.Add(item); };
                                    if (listView1.InvokeRequired) listView1.Invoke(invoker); else invoker();
                                }
                                invoker = (MethodInvoker)delegate { listView1.LargeImageList = imglist; };
                                if (listView1.InvokeRequired) listView1.Invoke(invoker); else invoker();
                                invoker = (MethodInvoker)delegate
                                {
                                    leftButton.Enabled = CurrentPage > 0;
                                    rightButton.Enabled = CurrentPage < PageCount - 1;
                                    textBox1.Enabled = true;
                                    pictureBox1.Visible = false;
                                };
                                if (InvokeRequired) Invoke(invoker); else invoker();
                                _LoadThreadRunning = false;
                                if (LoadingFinished != null)
                                    LoadingFinished(this, null);
                            }));
                        loadPageThread.Start();
                    }
            }
        }

        private void SetLabel()
        {
            MethodInvoker invoker = (MethodInvoker)delegate
                {
                    label1.Text = string.Format("Page {0} / {1} (Posts: {2})", _currentPage + 1, PageCount, _Posts.Count);
                    textBox1.Text = (_currentPage + 1).ToString();
                };
            if (InvokeRequired)
                Invoke(invoker);
            else invoker();
        }

        /// <summary>The posts to show</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<BooruPost> Posts
        {
            get { return _Posts; }
            set
            {
                if (value != null)
                {
                    _Posts = value;
                    PageCount = (int)Math.Truncate((value.Count - 1f) / ThumbsPerPage) + 1;
                    if (listView1.IsHandleCreated)
                        CurrentPage = 0;
                }
            }
        }

        /// <summary>Creates a new ThumbnailView</summary>
        public ThumbnailView()
        {
            InitializeComponent();
            int side_length = ThumbsSize;
            listView1.LargeImageList = CreateImageList(side_length);
            Helper.SetListViewPadding(listView1.Handle, side_length + 12, side_length + 12);
            SetLabel();
            listView1.HandleCreated += new EventHandler((sender, e) => CurrentPage = 0);
            listView1.DoubleClick += new EventHandler(listView1_DoubleClick);
            listView1.KeyDown += new KeyEventHandler(listView1_KeyDown);
            pictureBox1.Visible = false;
            pictureBox1.BackColor = Color.White;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.Image = Properties.Resources.ajax_loading;
            Size pbOffset = new Size(1, 1);
            pictureBox1.Location = Point.Add(listView1.Location, pbOffset);
            pictureBox1.Size = Size.Subtract(listView1.Size, new Size(pbOffset.Width * 2, pbOffset.Height * 2));
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            textBox1.Click += new EventHandler((sender, e) => textBox1.SelectAll());
            textBox1.MouseWheel += new MouseEventHandler(textBox1_MouseWheel);
            label2.Visible = false;
            label2.Parent = pictureBox1;
            label2.Dock = DockStyle.Fill;
            label2.TextAlign = ContentAlignment.MiddleCenter;
            listView1.MouseDown += new MouseEventHandler(listView1_MouseDown);
            listView1.MouseUp += new MouseEventHandler((sender, e) => _DragMouseDown = false);
            listView1.MouseLeave += new EventHandler(listView1_MouseLeave);
        }

        void listView1_MouseLeave(object sender, EventArgs e)
        {
            //TODO DragDrop support
            /*
            if (_DragMouseDown)
            {
                DataObject dao = SelectedPosts[0].GetDragDropDataObject(true, true);

            string tempfile = Helper.GetTempFile();
                SelectedPosts[0].Image.Save
            tempfile = BitmapFactory.SaveBitmap(ImageNotThumbnail ? Image : Thumbnail, tempfile, true, null);
            DataObject dao = new DataObject(DataFormats.FileDrop, new string[1] { tempfile });
            //TODO NI Implement drag image
            return dao;
        }

                DoDragDrop(dao, DragDropEffects.Copy);
            }
            */
        }

        private void setPageByTextBox(int value, bool setPage)
        {
            if (value < 1)
                textBox1.Text = "1";
            else if (value > PageCount)
                textBox1.Text = PageCount.ToString();
            else if (setPage)
                CurrentPage = value - 1;
            else textBox1.Text = value.ToString();
        }

        void textBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            int value;
            if (int.TryParse(textBox1.Text, out value))
            {
                value += e.Delta > 0 ? 1 : -1;
                setPageByTextBox(value, false);
            }
        }

        void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            int page = 0;
            if (e.KeyCode == Keys.Enter)
                if (int.TryParse(textBox1.Text, out page))
                    setPageByTextBox(page, true);
        }

        void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ContextMenu != null && e.Button == MouseButtons.Right)
            {
                int index = listView1.Items.IndexOf(listView1.GetItemAt(e.X, e.Y));
                _contextMenu.Show(listView1, e.Location);
            }
            if (e.Button == MouseButtons.Left)
                _DragMouseDown = true;
        }

        private ImageList CreateImageList(int side_length)
        {
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(side_length, side_length);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            return imageList;
        }

        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                listView1_DoubleClick(sender, e);
        }

        private void leftButton_Click(object sender, EventArgs e) { CurrentPage--; }

        private void rightButton_Click(object sender, EventArgs e) { CurrentPage++; }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                if (PostOpened != null)
                    PostOpened(this, new EventArgs());
        }
    }
}