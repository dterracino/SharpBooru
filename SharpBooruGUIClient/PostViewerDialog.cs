using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class PostViewerDialog : Form
    {
        private Booru _Booru;
        private BooruPost _Post;
        private List<BooruPost> _Posts;
        private int _index = -1;
        private object _loadLock = new object();

        private int index
        {
            get { return _index; }
            set
            {
                if (value < 0) value = 0;
                else if (value > _Posts.Count - 1) value = _Posts.Count - 1;
                if (value != index)
                {
                    ChangePost(_Posts[value]);
                    _index = value;
                }
            }
        }

        public PostViewerDialog(Booru Booru, List<BooruPost> Posts, int StartIndex = 0)
        {
            _Booru = Booru;
            _Posts = Posts;
            if (Posts == null)
                throw new ArgumentNullException("Posts can not be null");
            else if (Posts.Count < 1)
                throw new ArgumentException("Posts count must be > 0");
            InitializeComponent();
            tagList.SetBooru(_Booru);
            //TODO GetAllTags
            //tagList.SetTagListBoxParams(_Booru.SearchTags(), this);
            //scalablePictureBox.ScalablePictureBoxImp.PictureBox.DoubleClick += new EventHandler(delegate { Invoke(new MethodInvoker(ToggleFullScreen)); });
            postScoreNumericUpDown.ValueChanged += new EventHandler((sender, e) =>
                {
                    _Post.Score = (int)postScoreNumericUpDown.Value;
                    _Post.EditCount++;
                });
            postScoreNumericUpDown.Maximum = int.MaxValue;
            postScoreNumericUpDown.Minimum = int.MinValue;
            //TODO GetAllRatings
            //BooruHelper.GetAllRatings(_Booru.wrapper).ForEach(x => postRatingComboBox.Items.Add(x));
            /*
            postRatingComboBox.SelectedIndexChanged += new EventHandler((sender, e) =>
                {
                    _Post.Rating = (byte)(postRatingComboBox.SelectedIndex + 1);
                    _Post.EditCount++;
                });
            */
            index = StartIndex;
            GUIHelper.CreateToolTip(editPostButton, "Edit post");
            GUIHelper.CreateToolTip(deletePostButton, "Delete post");
            GUIHelper.CreateToolTip(saveImageButton, "Save image");
            GUIHelper.CreateToolTip(toggleFullscreenButton, "Toggle fap mode");
            GUIHelper.CreateToolTip(setWallpaperButton, "Set image as desktop wallpaper");
            GUIHelper.CreateToolTip(nextPostButton, "Next post");
            GUIHelper.CreateToolTip(previosPostButton, "Previous post");
            GUIHelper.CreateToolTip(editImageButton, "Edit image");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!tagList.Focused)
                if (keyData == Keys.Left && previosPostButton.Enabled)
                {
                    index--;
                    return true;
                }
                else if (keyData == Keys.Right && nextPostButton.Enabled)
                {
                    index++;
                    return true;
                }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ChangePost(BooruPost Post)
        {
            _Post = Post;
            Thread loadThread = new Thread(new ThreadStart(delegate
                {
                    lock (_loadLock)
                    {
                        SetLoadingMode(true);
                        Bitmap image = _Post.Image.Bitmap;
                        try
                        {
                            MethodInvoker invoker = () => { scalablePictureBox.Picture = image; };
                            if (scalablePictureBox.InvokeRequired) scalablePictureBox.Invoke(invoker); else invoker();
                            invoker = () =>
                            {
                                tagList.Post = _Post;
                                _Post.ViewCount++;
                                postScoreNumericUpDown.Value = _Post.Score;
                                Text = string.Format("{0} - {1}x{2} - Views {4} - Added {3}", _Post.ID, _Post.Width, _Post.Height, _Post.CreationDate, _Post.ViewCount);
                            };
                            if (InvokeRequired) Invoke(invoker); else invoker();
                        }
                        catch (ObjectDisposedException) { }
                        SetLoadingMode(false);
                    }
                }));
            loadThread.Start();
        }

        private void SetLoadingMode(bool LoadingMode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetLoadingMode), LoadingMode);
                return;
            }
            editPostButton.Enabled = !LoadingMode;
            deletePostButton.Enabled = !LoadingMode;
            saveImageButton.Enabled = !LoadingMode;
            setWallpaperButton.Enabled = !LoadingMode;
            if (LoadingMode)
            {
                previosPostButton.Enabled = false;
                nextPostButton.Enabled = false;
            }
            else
            {
                previosPostButton.Enabled = index > 0;
                nextPostButton.Enabled = index < _Posts.Count - 1;
            }
            //toggleFullscreenButton.Enabled = !LoadingMode; TODO Fix fullscreen
            tagList.Enabled = !LoadingMode;
            editImageButton.Enabled = !LoadingMode;
            postScoreLabel.Enabled = !LoadingMode;
            postScoreNumericUpDown.Enabled = !LoadingMode;
        }

        private object[] saveState = null;
        private void ToggleFullScreen()
        {
            SuspendLayout();
            if (saveState != null)
            {
                scalablePictureBox.Dock = DockStyle.None;
                scalablePictureBox.Anchor = (AnchorStyles)saveState[0];
                saveState = null;
            }
            else
            {
                saveState = new object[2];
                saveState[0] = scalablePictureBox.Anchor;
                scalablePictureBox.Dock = DockStyle.Fill;
            }
            ResumeLayout();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            EditPostDialog edp = new EditPostDialog(_Booru, _Post);
            if (edp.ShowDialog() == DialogResult.OK)
                ChangePost(_Post);
            */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really wan't to delete this post?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _Booru.DeletePost(_Post.ID);
                if (nextPostButton.Enabled)
                    index++;
                else if (previosPostButton.Enabled)
                    index--;
                else Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All files|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _Post.Image.Save(sfd.FileName);
                MessageBox.Show("Image saved", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e) { Invoke(new MethodInvoker(ToggleFullScreen)); }

        private void button5_Click(object sender, EventArgs e)
        {
            /* TODO SetWallpaper
            if (Helper.SetWallpaper(_Post.Image, true))
                MessageBox.Show("Wallpaper changed", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("An error occured. Wallpaper not changed.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            */
        }

        private void previosPostButton_Click(object sender, EventArgs e) { index--; }

        private void nextPostButton_Click(object sender, EventArgs e) { index++; }

        private void editImageButton_Click(object sender, EventArgs e)
        {
            /* TODO Photoshop edit function
            string editorEXE = "C:\\Program Files\\Adobe\\Adobe Photoshop CS5 (64 Bit)\\Photoshop.exe";
            if (string.IsNullOrWhiteSpace(editorEXE))
                MessageBox.Show("No image editor defined in settings", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                string tempFile = Helper.GetTempFile();
                tempFile = BitmapFactory.SaveBitmap(_Post.Image, tempFile);
                Process editor = Process.Start(editorEXE, tempFile);
                editor.WaitForExit();
                _Post.Image = new BooruImage(tempFile);
            }
            */
        }
    }
}
