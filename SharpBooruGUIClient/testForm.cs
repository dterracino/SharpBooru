using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class testForm : Form
    {
        public testForm()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string[] files = System.IO.Directory.GetFiles(@"C:\temp\____thumbs");
            BooruPostList posts = new BooruPostList();
            foreach (string file in files)
            {
                posts.Add(new BooruPost()
                {
                    Thumbnail = new BooruImage(file)
                });
            }
            pagedThumbView1.Posts = posts;
            //pagedThumbView1.ImageOpened += (isender, ie, iid) =>
            //    MessageBox.Show(string.Format("Image {0} opened", iid));
        }
    }
}
