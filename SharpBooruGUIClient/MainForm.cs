using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class MainForm : Form
    {
        private ClientBooru _Booru = null;
        private string _LastSearch = null;

        public MainForm(ClientBooru Booru)
        {
            _Booru = Booru;
            InitializeComponent();
            this.searchBox.SetTags(_Booru.GetAllTags());
            this.searchBox.EnterPressed += tagTextBox1_EnterPressed;
            this.booruThumbView.SetBooru(_Booru);
            this.booruThumbView.ImageOpened += (sender, aObj) => openImage(aObj);
            this.booruThumbView.ImageRightClick += (sender, e, aObj) => imageContextMenuStrip.Show(sender, e.Location);
            this.booruThumbView.LoadingStarted += () => SetLoadingMode(true);
            this.booruThumbView.LoadingFinished += () => SetLoadingMode(false);
            this.Shown += tagTextBox1_EnterPressed;
            this.openToolStripMenuItem.Click += (sender, e) => openImage(booruThumbView.SelectedPost);
            this.editToolStripMenuItem.Click += (sender, e) =>
                {
                    BooruPost post = booruThumbView.SelectedPost;
                    using (EditDialog editDialog = new EditDialog())
                        if (editDialog.ShowDialog(_Booru, ref post) == DialogResult.OK)
                            _Booru.SavePost(post);
                };
            this.deleteToolStripMenuItem.Click += (sender, e) => _Booru.DeletePost(booruThumbView.SelectedPost);
            GUIHelper.CreateToolTip(buttonRefresh, "Refresh searched posts");
            GUIHelper.CreateToolTip(buttonChangeUser, "Change the user");
            GUIHelper.CreateToolTip(buttonImportDialog, "Import posts into the booru");
            GUIHelper.CreateToolTip(buttonImgSearch, "Search for image duplicates");
            SetTitle();
            CheckPermissions();
        }

        public void SetLoadingMode(bool Loading)
        {
            if (!this.InvokeRequired)
            {
                searchBox.Enabled = !Loading;
                buttonRefresh.Enabled = !Loading;
                buttonChangeUser.Enabled = !Loading;
                buttonImgSearch.Enabled = !Loading;
                buttonImportDialog.Enabled = !Loading && _Booru.CurrentUser.CanAddPosts;
            }
            else this.Invoke(new Action<bool>(SetLoadingMode), Loading);
        }

        private void openImage(object aObj)
        {
            BooruPost post = aObj as BooruPost;
            List<ulong> postIDs = booruThumbView.Posts;
            using (PostViewerDialog pvd = new PostViewerDialog(_Booru, postIDs, postIDs.IndexOf(post.ID)))
                pvd.ShowDialog();
        }

        private void tagTextBox1_EnterPressed(object sender, EventArgs e)
        {
            booruThumbView.Posts = _Booru.Search(searchBox.Text);
            _LastSearch = searchBox.Text;
        }

        private void buttonImportDialog_Click(object sender, EventArgs e)
        {
            using (ImportDialog iDialog = new ImportDialog(_Booru))
                iDialog.ShowDialog();
        }

        private void buttonChangeUser_Click(object sender, EventArgs e)
        {
            using (LoginDialog ld = new LoginDialog(_Booru.CurrentUser.IsAdmin))
                if (ld.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _Booru.ChangeUser(ld.Username, ld.Password);
                        SetTitle();
                        CheckPermissions();
                    }
                    catch (BooruProtocol.BooruException bEx) { MessageBox.Show(bEx.Message, "ERROR: Change User", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
        }

        private void SetTitle()
        {
            Text = string.Format("SharpBooru GUI Client  -  {0} by {1} [{2}]",
                _Booru.GetBooruMiscOption<string>(BooruMiscOption.BooruName),
                _Booru.GetBooruMiscOption<string>(BooruMiscOption.BooruCreator),
                _Booru.CurrentUser.Username);
        }

        private void CheckPermissions()
        {
            BooruUser cUser = _Booru.CurrentUser;
            editToolStripMenuItem.Enabled = cUser.CanEditPosts;
            deleteToolStripMenuItem.Enabled = cUser.CanDeletePosts;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            SetTitle();
            CheckPermissions();
            searchBox.SetTags(_Booru.GetAllTags());
            _Booru.ClearCaches();
            booruThumbView.Posts = _Booru.Search(_LastSearch);
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); }

        private void buttonImgSearch_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image cbImg = Clipboard.GetImage();
                if (cbImg is Bitmap)
                    using (BooruImage bImg = BooruImage.FromBitmap(cbImg as Bitmap))
                    {
                        byte[] imgHash = bImg.CalculateImageHash();
                        booruThumbView.Posts = _Booru.FindImageDupes(imgHash);
                    }
                else MessageBox.Show("No valid bitmap in clipboard", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("No image in clipboard", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}