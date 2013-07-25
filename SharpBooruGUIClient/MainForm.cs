using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using TA.SharpBooru.Client.GUI.Controls;

namespace TA.SharpBooru.Client.GUI
{
    public partial class MainForm : Form
    {
        private Booru _Booru;

        public MainForm(Booru Booru)
        {
            InitializeComponent();
            _Booru = Booru;
            searchBox.SetTags(_Booru.GetAllTags());
            searchBox.EnterPressed += tagTextBox1_EnterPressed;
            booruThumbView.SetBooru(_Booru);
            booruThumbView.ImageOpened += new BooruThumbView.ImageOpenedHandler(booruThumbView_ImageOpened);
        }

        void booruThumbView_ImageOpened(object sender, EventArgs e, object aObj)
        {
            BooruPost post = aObj as BooruPost;
            _Booru.GetImage(ref post);
            /*
            string tempFile = Helper.GetTempFile();
            post.Image.Save(ref tempFile);
            Process imgViewerProcess = Process.Start(tempFile);
            */
            PostViewerDialog pvd = new PostViewerDialog(_Booru, new List<BooruPost>() { post }, 0);
            pvd.ShowDialog();
        }

        private void tagTextBox1_EnterPressed(object sender, EventArgs e)
        {
            List<ulong> postIDs = _Booru.Search(searchBox.Text);
            booruThumbView.Posts = postIDs;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (TestForm tForm = new TestForm())
                tForm.ShowDialog();
        }

        private void buttonImportForm_Click(object sender, EventArgs e)
        {

        }
    }
}