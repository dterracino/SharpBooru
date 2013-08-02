using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using TA.SharpBooru.Client.GUI.Controls;

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
            searchBox.SetTags(_Booru.GetAllTags());
            searchBox.EnterPressed += tagTextBox1_EnterPressed;
            booruThumbView.SetBooru(_Booru);
            booruThumbView.ImageOpened += booruThumbView_ImageOpened;
            this.Shown += tagTextBox1_EnterPressed;
            this.buttonAdminTools.Click += (sender, e) => adminContextMenuStrip.Show(buttonAdminTools, new Point(buttonAdminTools.Width, 0));
            this.killServerToolStripMenuItem.Click += killServerToolStripMenuItem_Click;
            this.saveBooruToolStripMenuItem.Click += saveBooruToolStripMenuItem_Click;
            CheckPermissions();
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

        private void booruThumbView_ImageOpened(object sender, EventArgs e, object aObj)
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
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            CheckPermissions();
            searchBox.SetTags(_Booru.GetAllTags());
            booruThumbView.Posts = _Booru.Search(_LastSearch);
        }
    }
}