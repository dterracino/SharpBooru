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

        private void button1_Click(object sender, EventArgs e)
        {
            string[] files = System.IO.Directory.GetFiles(@"C:\temp\____thumbs");
            int i = 1;
            foreach (string file in files)
            {
                thumbnailView1.Add(new Bitmap(file), i);
                i++;
            }
            thumbnailView1.ImageOpened += (isender, ie, iid) =>
                MessageBox.Show(string.Format("Image {0} opened", iid));
        }
    }
}
