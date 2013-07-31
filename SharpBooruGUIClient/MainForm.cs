﻿using System;
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
            InitializeComponent();
            _Booru = Booru;
            searchBox.SetTags(_Booru.GetAllTags());
            searchBox.EnterPressed += tagTextBox1_EnterPressed;
            booruThumbView.SetBooru(_Booru);
            booruThumbView.ImageOpened += new BooruThumbView.ImageOpenedHandler(booruThumbView_ImageOpened);
        }

        private void booruThumbView_ImageOpened(object sender, EventArgs e, object aObj)
        {
            BooruPost post = aObj as BooruPost;
            _Booru.GetImage(ref post);
            List<ulong> postIDs = booruThumbView.Posts;
            PostViewerDialog pvd = new PostViewerDialog(_Booru, postIDs, postIDs.IndexOf(post.ID));
            pvd.ShowDialog();
        }

        private void tagTextBox1_EnterPressed(object sender, EventArgs e)
        {
            booruThumbView.Posts = _Booru.Search(searchBox.Text);
            _LastSearch = searchBox.Text;
        }

        private void buttonImportDialog_Click(object sender, EventArgs e)
        {
            ImportDialog iDialog = new ImportDialog(_Booru);
            iDialog.ShowDialog();
        }

        private void buttonChangeUser_Click(object sender, EventArgs e)
        {
            using (LoginDialog ld = new LoginDialog())
                if (ld.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _Booru.ChangeUser(ld.Username, ld.Password);
                        MessageBox.Show("User successfully changed", "Change User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        booruThumbView.Posts = _Booru.Search(_LastSearch);
                    }
                    catch (BooruProtocol.BooruException bEx) { MessageBox.Show(bEx.Message, "ERROR: Change User", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
        }
    }
}