using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class EditPostDialog : Form
    {
        private BooruPost _Post;
        private Booru booru;
        private BooruImage _Image;

        public BooruImage Image
        {
            get { return _Image; }
            set
            {
                if (value != null)
                {
                    _Image = value;
                    MethodInvoker invoker = new MethodInvoker(() => scalablePictureBox1.Picture = _Image.Bitmap);
                    if (scalablePictureBox1.InvokeRequired) scalablePictureBox1.Invoke(invoker); else invoker();
                }
            }
        }

        public EditPostDialog(Booru Booru, BooruPost Post)
        {
            booru = Booru;
            _Post = Post;
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            Text = "Edit post...";
            textBox1.Text = _Post.Source;
            textBox2.Text = _Post.Comment;
            List<BooruTag> tags = _Post.Tags;
            List<string> str_tags = BooruHelper.StringListFromBooruTags(tags);
            tagTextBox1.Text = string.Join(" ", str_tags);
            trackBar1.Value = (byte)_Post.Rating;
            tagTextBox1.SetTags(booru.SearchTags());
            UpdateTrackBar();
            Thread loadThread = new Thread((ThreadStart)delegate
                {
                    Image = _Post.Image;
                    MethodInvoker invoker = (MethodInvoker)delegate
                        {
                            button1.Enabled = true;
                            button2.Enabled = true;
                        };
                    if (InvokeRequired) Invoke(invoker); else invoker();
                });
            loadThread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Post.Source = textBox1.Text.Trim();
            _Post.Comment = textBox2.Text.Trim();
            _Post.Tags = BooruHelper.GetBooruTags(booru, tagTextBox1.Text);
            _Post.Image = Image;
            _Post.Rating = (byte)trackBar1.Value;
            _Post.EditCount++;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open image...";
            ofd.Multiselect = false;
            ofd.Filter = Constants.FILTER_IMAGES;
            if (ofd.ShowDialog() == DialogResult.OK)
                Image = new BooruImage(ofd.FileName);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.Invoke(new MethodInvoker(UpdateTrackBar));
        }

        private void UpdateTrackBar()
        {
            label1.Text = BooruHelper.GetRatingDescription(booru.wrapper, (byte)trackBar1.Value);
        }
    }
}
