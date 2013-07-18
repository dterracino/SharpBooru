using System;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public partial class PagedThumbView : UserControl
    {
        private BooruPostList _Posts;
        private ushort _ThumbsPerPage = 30;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BooruPostList Posts
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

        public PagedThumbView()
        {
            InitializeComponent();
            pageSwitcher.PageChanged += new EventHandler(pageSwitcher_PageChanged);
            ajaxProgressIndicator1.Location = thumbView.Location;
            ajaxProgressIndicator1.Size = thumbView.Size;
            ajaxProgressIndicator1.Anchor = thumbView.Anchor;
        }

        private void pageSwitcher_PageChanged(object sender, EventArgs e)
        {
                Thread loadThread = new Thread(pageSwitcher_PageChangedThreadMethod);
                loadThread.Start();
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
                for (int i = postIndex; i < postCount; i++)
                    thumbView.Add(_Posts[i].Thumbnail.Bitmap, _Posts[i]);
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
            pageSwitcher.CurrentPage = 1;
        }

        public void RefreshLabel() { pageLabel.Text = string.Format("Page {0} of {1}", pageSwitcher.CurrentPage, pageSwitcher.Pages); }
    }
}