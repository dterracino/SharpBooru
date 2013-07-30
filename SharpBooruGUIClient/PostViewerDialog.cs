﻿using System;
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
        private List<ulong> _PostIDs;
        private int _Index = -1;
        private object _loadLock = new object();

        private int Index
        {
            get { return _Index; }
            set
            {
                if (value < 0) value = 0;
                else if (value > _PostIDs.Count - 1) 
                    value = _PostIDs.Count - 1;
                if (value != _Index)
                {
                    BooruPost postToShow = _Booru.GetPost(_PostIDs[value]);
                    ChangePost(postToShow);
                    _Index = value;
                }
            }
        }

        public PostViewerDialog(Booru Booru, List<ulong> PostIDs, int StartIndex = 0)
        {
            _Booru = Booru;
            if (PostIDs == null)
                throw new ArgumentNullException("Posts can not be null");
            else if (PostIDs.Count < 1)
                throw new ArgumentException("Posts count must be > 0");
            else _PostIDs = PostIDs;
            InitializeComponent();
            Index = StartIndex;
            GUIHelper.CreateToolTip(buttonDeletePost, "Delete post");
            GUIHelper.CreateToolTip(buttonSaveImage, "Save image");
            GUIHelper.CreateToolTip(buttonSetWallpaper, "Set image as desktop wallpaper");
            GUIHelper.CreateToolTip(buttonNextPost, "Next post");
            GUIHelper.CreateToolTip(buttonPreviousPost, "Previous post");
            GUIHelper.CreateToolTip(buttonEditImage, "Edit image");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!tagList.Focused)
                if (keyData == Keys.Left && buttonPreviousPost.Enabled)
                {
                    _Index--;
                    return true;
                }
                else if (keyData == Keys.Right && buttonNextPost.Enabled)
                {
                    _Index++;
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
                        if (_Post.Image == null)
                            _Booru.GetImage(ref _Post);
                        Bitmap image = _Post.Image.Bitmap;
                        try
                        {
                            GUIHelper.Invoke(scalablePictureBox, () => { scalablePictureBox.Picture = image; });
                            GUIHelper.Invoke(this, () =>
                                {
                                    tagList.Tags = _Post.Tags;
                                    //TODO Window title
                                    Text = string.Format("{0} - {1}x{2} - Views {4} - Added {3}", _Post.ID, _Post.Width, _Post.Height, _Post.CreationDate, _Post.ViewCount);
                                });
                        }
                        catch (ObjectDisposedException) { }
                        SetLoadingMode(false);
                    }
                })) { IsBackground = true };
            loadThread.Start();
        }

        private void SetLoadingMode(bool LoadingMode)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetLoadingMode), LoadingMode);
                return;
            }
            buttonDeletePost.Enabled = !LoadingMode;
            buttonSaveImage.Enabled = !LoadingMode;
            buttonSetWallpaper.Enabled = !LoadingMode;
            if (LoadingMode)
            {
                buttonPreviousPost.Enabled = false;
                buttonNextPost.Enabled = false;
            }
            else
            {
                buttonPreviousPost.Enabled = Index > 0;
                buttonNextPost.Enabled = Index < _PostIDs.Count - 1;
            }
            tagList.Enabled = !LoadingMode;
            buttonEditImage.Enabled = !LoadingMode;
        }

        private void buttonDeletePost_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really wan't to delete this post?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _Booru.DeletePost(_Post.ID);
                if (buttonNextPost.Enabled)
                    Index++;
                else if (buttonPreviousPost.Enabled)
                    Index--;
                else Close();
            }
        }

        private void buttonSaveImage_Click(object sender, EventArgs e)
        {
            //TODO Better filter = rename file after saving
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All files|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _Post.Image.Save(sfd.FileName);
                MessageBox.Show("Image saved", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonSetWallpaper_Click(object sender, EventArgs e)
        {
            if (GUIHelper.SetWallpaper(_Post.Image, true))
                MessageBox.Show("Wallpaper changed", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("An error occured. Wallpaper not changed.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonPreviousPost_Click(object sender, EventArgs e) { Index--; }

        private void buttonNextPost_Click(object sender, EventArgs e) { Index++; }

        private void buttonEditImage_Click(object sender, EventArgs e)
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