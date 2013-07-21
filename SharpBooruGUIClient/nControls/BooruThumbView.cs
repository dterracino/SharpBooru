using System;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public partial class BooruThumbView : UserControl
    {
        public delegate void ImageOpenedHandler(object sender, EventArgs e, object aObj);
        public event ImageOpenedHandler ImageOpened;

        private Booru _Booru = null;
        private List<ulong> _Posts = new List<ulong>();
        private ushort _ThumbsPerPage = 30;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ulong> Posts
        {
            get { return _Posts; }
            set
            {
                if (value != null)
                {
                    int oldCount = _Posts.Count;
                    _Posts = value;
                    RefreshPages();
                    if (_Posts.Count != oldCount)
                        RefreshLabel();
                }
            }
        }

        public BooruThumbView()
        {
            InitializeComponent();
            pageSwitcher.PageChanged += new EventHandler(pageSwitcher_PageChanged);
            ajaxProgressIndicator1.Location = thumbView.Location;
            ajaxProgressIndicator1.Size = thumbView.Size;
            ajaxProgressIndicator1.Anchor = thumbView.Anchor;
            thumbView.ImageOpened += (sender, e, aObj) =>
                {
                    if (ImageOpened != null)
                        ImageOpened(sender, e, aObj);
                };
        }

        public void SetBooru(Booru Booru) { _Booru = Booru; }

        private void pageSwitcher_PageChanged(object sender, EventArgs e)
        {
            //TODO Multithreading
            //Thread loadThread = new Thread(pageSwitcher_PageChangedThreadMethod);
            //loadThread.Start();
            pageSwitcher_PageChangedThreadMethod();
            RefreshLabel();
        }

        private object _loadThreadLock = new object();
        private void pageSwitcher_PageChangedThreadMethod()
        {
            lock (_loadThreadLock)
            {
                SetLoadingMode(true);
                int postIndex = pageSwitcher.CurrentPage * _ThumbsPerPage;
                int postCount = _Posts.Count - _ThumbsPerPage * pageSwitcher.CurrentPage;
                if (postCount > _ThumbsPerPage)
                    postCount = _ThumbsPerPage;
                thumbView.Clear();
                for (int i = 0; i < postCount; i++)
                {
                    ulong postID = _Posts[i+postIndex];
                    if (_Booru != null)
                    {
                        BooruPost postToAdd = _Booru.GetPost(postID);
                        thumbView.Add(postToAdd.Thumbnail.Bitmap, postToAdd);
                    }
                    else thumbView.Add(null, postID);
                }
                SetLoadingMode(false);
            }
        }

        private void SetLoadingMode(bool LoadingMode)
        {
            MethodInvoker invoker = () =>
                {
                    pageSwitcher.Enabled = !LoadingMode;
                    pageLabel.Enabled = !LoadingMode;
                    ajaxProgressIndicator1.Enabled = LoadingMode;
                };
            if (InvokeRequired)
                Invoke(invoker);
            else invoker();
        }

        public void RefreshPages()
        {
            pageSwitcher.Pages = (int)((_Posts.Count - 1f) / _ThumbsPerPage) + 1;
            bool refreshManually = pageSwitcher.CurrentPage == 0;
            pageSwitcher.CurrentPage = 0;
            if (refreshManually)
                pageSwitcher_PageChanged(this, null);
        }

        public void RefreshLabel() { pageLabel.Text = string.Format("Page {0} of {1}", pageSwitcher.CurrentPage + 1, pageSwitcher.Pages); }
    }
}