using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class MainForm : Form
    {
        private Booru _Booru = null;
        private string _LastSearch = null;

        public MainForm(Booru Booru)
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
            this.buttonAdminTools.Click += (sender, e) => adminContextMenuStrip.Show(buttonAdminTools, new Point(buttonAdminTools.Width, 0));
            this.killServerToolStripMenuItem.Click += killServerToolStripMenuItem_Click;
            this.saveBooruToolStripMenuItem.Click += saveBooruToolStripMenuItem_Click;
            this.openToolStripMenuItem.Click += (sender, e) => openImage(booruThumbView.SelectedPost);
            this.editToolStripMenuItem.Click += (sender, e) =>
                {
                    BooruPost post = booruThumbView.SelectedPost;
                    using (EditDialog editDialog = new EditDialog())
                        if (editDialog.ShowDialog(_Booru, ref post) == DialogResult.OK)
                            _Booru.SavePost(post);
                };
            this.deleteToolStripMenuItem.Click += (sender, e) => _Booru.DeletePost(booruThumbView.SelectedPost);
            //notifyIcon.Click += notifyIcon_Click;
            GUIHelper.CreateToolTip(buttonAdminTools, "Admin tools");
            GUIHelper.CreateToolTip(buttonRefresh, "Refresh searched posts");
            GUIHelper.CreateToolTip(buttonChangeUser, "Change the user");
            GUIHelper.CreateToolTip(buttonImportDialog, "Import posts into the booru");
            CheckPermissions();
        }

        /*
        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
            }
        }
        */

        public void SetLoadingMode(bool Loading)
        {
            if (!this.InvokeRequired)
            {
                searchBox.Enabled = !Loading;
                buttonRefresh.Enabled = !Loading;
                buttonChangeUser.Enabled = !Loading;
            }
            else this.Invoke(new Action<bool>(SetLoadingMode), Loading);
        }

        private void saveBooruToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _Booru.SaveServerBooru();
            MessageBox.Show("Booru saved", "SaveServerBooru", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void killServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "ForceKillServer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _Booru.ForceKillServer();
                MessageBox.Show("Server killed", "ForceKillServer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
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
                if (iDialog.ShowDialog() == DialogResult.OK)
                    Refresh();
        }

        private void buttonChangeUser_Click(object sender, EventArgs e)
        {
            using (LoginDialog ld = new LoginDialog())
                if (ld.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _Booru.ChangeUser(ld.Username, ld.Password);
                        CheckPermissions();
                        booruThumbView.Posts = _Booru.Search(_LastSearch);
                    }
                    catch (BooruProtocol.BooruException bEx) { MessageBox.Show(bEx.Message, "ERROR: Change User", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
        }

        private void CheckPermissions()
        {
            BooruUser cUser = _Booru.CurrentUser;
            buttonImportDialog.Enabled = cUser.CanAddPosts;
            buttonAdminTools.Visible = cUser.IsAdmin;
            editToolStripMenuItem.Enabled = cUser.CanEditPosts;
            deleteToolStripMenuItem.Enabled = cUser.CanDeletePosts;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            CheckPermissions();
            searchBox.SetTags(_Booru.GetAllTags());
            booruThumbView.Posts = _Booru.Search(_LastSearch);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
                this.Hide();
        }
    }
}