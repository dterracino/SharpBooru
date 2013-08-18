using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public partial class BooruThumbView : UserControl
    {
        public delegate void ImageOpenedHandler(SelectablePictureBox sender, object aObj);
        public event ImageOpenedHandler ImageOpened;

        public delegate void ImageRightClickHandler(SelectablePictureBox sender, MouseEventArgs e, object aObj);
        public event ImageRightClickHandler ImageRightClick;

        public bool AsynchronousLoading = true;

        private Booru _Booru = null;
        private List<ulong> _Posts = new List<ulong>();
        private ushort _ThumbsPerPage = 30;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ulong> Posts
        {
            get { return _Posts; }
            set { SetPosts(value); }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetPosts(List<ulong> PostIDs)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<List<ulong>>(SetPosts), PostIDs);
                return;
            }
            if (PostIDs != null)
            {
                int oldCount = _Posts.Count;
                ulong? firstPostInPage = null;
                if (_Posts.Count > 0)
                    firstPostInPage = _Posts[_ThumbsPerPage * pageSwitcher.CurrentPage];
                _Posts = PostIDs;
                int newPageIndex = 0;
                if (firstPostInPage.HasValue && _Posts.Count > 0)
                    if (_Posts.Contains(firstPostInPage.Value))
                    {
                        int newIndexInList = _Posts.IndexOf(firstPostInPage.Value);
                        newPageIndex = newIndexInList / _ThumbsPerPage;
                    }
                pageSwitcher.Pages = (int)((_Posts.Count - 1f) / _ThumbsPerPage) + 1;
                bool refreshManually = newPageIndex == pageSwitcher.CurrentPage;
                pageSwitcher.CurrentPage = newPageIndex;
                if (refreshManually)
                    pageSwitcher_PageChanged(this, null);
                if (_Posts.Count != oldCount)
                    RefreshLabel();
            }
        }

        public Color LabelForeColor
        {
            get { return pageLabel.ForeColor; }
            set { pageLabel.ForeColor = value; }
        }

        public Color ThumbViewBackColor
        {
            get { return thumbView.BackColor; }
            set { thumbView.BackColor = value; }
        }

        public BooruPost SelectedPost { get { return thumbView.SelectedObject as BooruPost; } }

        public BooruThumbView()
        {
            InitializeComponent();
            pageSwitcher.PageChanged += new EventHandler(pageSwitcher_PageChanged);
            ajaxLoading1.Location = thumbView.Location;
            ajaxLoading1.Size = thumbView.Size;
            ajaxLoading1.Anchor = thumbView.Anchor;
            thumbView.ImageOpened += (sender, aObj) =>
                {
                    if (ImageOpened != null)
                        ImageOpened(sender, aObj);
                };
            thumbView.ForeColor = Color.Black;
            thumbView.ImageRightClick += (sender, e, aObj) =>
                {
                    if (ImageRightClick != null)
                        ImageRightClick(sender, e, aObj);
                };
            RefreshLabel();
        }

        public void SetBooru(Booru Booru) { _Booru = Booru; }

        private void pageSwitcher_PageChanged(object sender, EventArgs e)
        {
            if (AsynchronousLoading)
            {
                Thread loadThread = new Thread(loadPage);
                loadThread.Start();
            }
            else loadPage();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void loadPage()
        {
            SetLoadingMode(true);
            int postIndex = pageSwitcher.CurrentPage * _ThumbsPerPage;
            int postCount = _Posts.Count - _ThumbsPerPage * pageSwitcher.CurrentPage;
            if (postCount > _ThumbsPerPage)
                postCount = _ThumbsPerPage;
            thumbView.Clear();
            for (int i = 0; i < postCount; i++)
            {
                ulong postID = _Posts[i + postIndex];
                if (_Booru != null)
                {
                    BooruPost postToAdd = _Booru.GetPost(postID);
                    string toolTipText = string.Format("# {0} - {1}x{2} - Added {3} by {4}", postToAdd.ID, postToAdd.Width, postToAdd.Height, postToAdd.CreationDate, postToAdd.Owner);
                    Color? borderColor = null;
                    if (postToAdd.Private)
                    {
                        if (postToAdd.Owner == _Booru.CurrentUser.Username)
                            borderColor = Color.Red;
                        else borderColor = Color.Orange;
                    }
                    else if (postToAdd.Owner == _Booru.CurrentUser.Username)
                        borderColor = Color.Green;
                    thumbView.Add(postToAdd.Thumbnail.Bitmap, postToAdd, toolTipText, borderColor);
                }
                else thumbView.Add(null, postID);
            }
            RefreshLabel();
            SetLoadingMode(false);
        }

        private void SetLoadingMode(bool LoadingMode)
        {
            if (!this.InvokeRequired)
            {
                pageSwitcher.Enabled = !LoadingMode;
                pageLabel.Enabled = !LoadingMode;
                ajaxLoading1.Enabled = LoadingMode;
            }
            else Invoke(new Action<bool>(SetLoadingMode), LoadingMode);
        }

        public void RefreshLabel() 
        {
            if (!pageLabel.InvokeRequired)
                pageLabel.Text = string.Format("Page {0} of {1}", pageSwitcher.CurrentPageHuman, pageSwitcher.Pages);
            else pageLabel.Invoke(new Action(RefreshLabel));
        }
    }
}