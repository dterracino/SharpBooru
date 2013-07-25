using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class MainForm : Form
    {
        private Booru _Booru;

        private object _PostsLock = new object();
        private BooruPost[] _Posts = new BooruPost[0];

        public void SetPosts(string Search)
        {
            /*
            lock (_PostsLock)
                _Posts = _Booru.Search(Search);
            */
        }

        public MainForm(Booru Booru)
        {
            _Booru = Booru;
            List<string> strTags = new List<string>();
            //TODO Tag Edit
            //_Booru.SearchTags().ForEach(x => strTags.Add(x.Tag));
            InitializeComponent();
            thumbnailView.BackColor = Helper.RandomColor();
            thumbnailView.StatusLabel.ForeColor = Helper.OppositeColor(thumbnailView.BackColor);
            thumbnailView.PostOpened += new EventHandler(toolStripMenuItem1_Click);
            thumbnailView.LoadingFinished += new EventHandler(thumbnailView_LoadingFinished);
            thumbnailView.ContextMenu = contextMenuStrip1;
            //TODO GetProperty Thumbs properties
            thumbnailView.ThumbsPerPage = 30;
            thumbnailView.ThumbsSize = 128;
            //thumbnailView.ThumbsPerPage = _Booru.GetProperty<int>(SQLBooru.Booru.Property.ThumbsPerPage);
            //thumbnailView.ThumbsSize = _Booru.GetProperty<int>(SQLBooru.Booru.Property.ThumbsSize);
            searchBox.ListBoxParent = this;
            searchBox.Tags = strTags;
            searchBox.KeyDown += new KeyEventHandler(searchBox_KeyDown);
            searchBox.Focus();
            MethodInvoker invoker = (MethodInvoker)delegate
                {
                    //thumbnailView.Posts = GetPosts();
                    RefreshStatusBar();
                };
            if (InvokeRequired) Invoke(invoker); else invoker();
            Text = string.Format("{0} by {1} - SQLBooru",
                //_Booru.GetProperty<string>(SQLBooru.Booru.Property.BooruName),
                //_Booru.GetProperty<string>(SQLBooru.Booru.Property.BooruCreator));
                "Xbooru", "Xcreator"); //TODO GetProperty server side
        }

        void thumbnailView_LoadingFinished(object sender, EventArgs e)
        {
            MethodInvoker invoker = (MethodInvoker)delegate { searchBox.Enabled = true; };
            if (searchBox.InvokeRequired) searchBox.Invoke(invoker); else invoker();
        }

        void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetPosts(searchBox.Text);
                MethodInvoker invoker = (MethodInvoker)delegate
                    {
                        searchBox.Enabled = false;
                        //thumbnailView.Posts = GetPosts();
                        RefreshStatusBar();
                    };
                if (InvokeRequired) Invoke(invoker); else invoker();
            }
        }

        private void RefreshStatusBar()
        { //toolStripStatusLabel1.Text = string.Format("Actual posts: {0}", GetPosts().Count); }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) { Close(); }

        private void querierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            SQLQuerierDialog sqlqd = new SQLQuerierDialog(_Booru);
            Helper.SetFormCentered(this, sqlqd);
            sqlqd.Show();
            */
        }

        private void addPostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            MultiEditPostDialog mepd = new MultiEditPostDialog(_Booru);
            Helper.SetFormCentered(this, mepd);
            mepd.Show();
            */
        }

        private void fromCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* TODO CSV Import
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            ofd.Title = "Import from CSV...";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int c = _Booru.ImportFromCSV(ofd.FileName);
                if (c < 0)
                    MessageBox.Show("An error occured", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(string.Format("{0} posts imported", c), "Import successfull", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            */
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (BooruPost post in thumbnailView.SelectedPosts)
            {
                /*
                PostViewerDialog pvd = new PostViewerDialog(_Booru, _Posts, _Posts.IndexOf(post));
                Helper.SetFormCentered(this, pvd);
                pvd.Show();
                */
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (thumbnailView.SelectedPosts.Count > 1)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select target folder...";
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string target = fbd.SelectedPath.Trim();
                    if (!target.EndsWith("\\")) target += "\\";
                    Thread saveImageThread = new Thread((ThreadStart)delegate
                        {
                            thumbnailView.SelectedPosts.ForEach(x => x.Image.Save(string.Format("{0}{1}.png", target, x.ID)));
                            MessageBox.Show("Images saved", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    saveImageThread.Start();
                }
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "All files|*.*";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    BooruPost selectedPost = thumbnailView.SelectedPosts[0];
                    Thread saveImageThread = new Thread((ThreadStart)delegate
                        {
                            selectedPost.Image.Save(sfd.FileName);
                            MessageBox.Show("Image saved", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    saveImageThread.Start();
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* TODO Post delete
            int count = thumbnailView.SelectedPosts.Count;
            string message = string.Format("Do you really wan't do delete {0} {1}?", count, count > 1 ? "posts" : "post");
            if (MessageBox.Show(message, "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                thumbnailView.SelectedPosts.ForEach(x => _Booru.DeletePost(x.ID));
                message = string.Format("{0} deleted", count > 1 ? "Posts" : "Post");
                MessageBox.Show(message, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            */
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* TODO Settings dialog
            SettingsDialog sd = new SettingsDialog(_Booru);
            sd.ShowDialog();
            */
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            MultiEditPostDialog mepd = new MultiEditPostDialog(_Booru);
            thumbnailView.SelectedPosts.ForEach(x => mepd.AddRowPost(x));
            mepd.Show();
            */
        }

        private void editImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* TODO Photoshop edit function
            string editorEXE = "C:\\Program Files\\Adobe\\Adobe Photoshop CS5 (64 Bit)\\Photoshop.exe";
            if (string.IsNullOrWhiteSpace(editorEXE))
                MessageBox.Show("No image editor defined in settings", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                string tempFile = Helper.GetTempFile();
                //tempFile = BitmapFactory.SaveBitmap(_Post.Image, tempFile);
                Process editor = Process.Start(editorEXE, tempFile);
                editor.WaitForExit();
                //_Post.Image = new BooruImage(tempFile);
            }
            */
        }
    }
}