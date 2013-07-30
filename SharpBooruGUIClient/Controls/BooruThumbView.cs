﻿using System;
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
        public delegate void ImageOpenedHandler(object sender, EventArgs e, object aObj);
        public event ImageOpenedHandler ImageOpened;

        public bool AsynchronousLoading = false;

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

        public BooruThumbView()
        {
            InitializeComponent();
            pageSwitcher.PageChanged += new EventHandler(pageSwitcher_PageChanged);
            ajaxLoading1.Location = thumbView.Location;
            ajaxLoading1.Size = thumbView.Size;
            ajaxLoading1.Anchor = thumbView.Anchor;
            thumbView.ImageOpened += (sender, e, aObj) =>
                {
                    if (ImageOpened != null)
                        ImageOpened(sender, e, aObj);
                };
            thumbView.ForeColor = Color.Black;
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
                    thumbView.Add(postToAdd.Thumbnail.Bitmap, postToAdd);
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

        public void RefreshPages()
        {
            pageSwitcher.Pages = (int)((_Posts.Count - 1f) / _ThumbsPerPage) + 1;
            bool refreshManually = pageSwitcher.CurrentPage == 0;
            pageSwitcher.CurrentPage = 0;
            if (refreshManually)
                pageSwitcher_PageChanged(this, null);
        }

        public void RefreshLabel() 
        {
            if (!pageLabel.InvokeRequired)
                pageLabel.Text = string.Format("Page {0} of {1}", pageSwitcher.CurrentPageHuman, pageSwitcher.Pages);
            else pageLabel.Invoke(new Action(RefreshLabel));
        }
    }
}