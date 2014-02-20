using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TA.SharpBooru.Client.GUI
{
    public partial class PostViewerDialog : Form
    {
        private BooruClient _Booru;
        private BooruPost _Post;
        private List<ulong> _PostIDs;
        private int _Index = -1;
        private FormWindowState _oldWindowState = FormWindowState.Normal;

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

        public PostViewerDialog(BooruClient Booru, List<ulong> PostIDs, int StartIndex = 0)
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
            GUIHelper.CreateToolTip(buttonEditPost, "Edit post");
            SetLoadingMode(false);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!tagList.Focused)
                if (keyData == Keys.Left && buttonPreviousPost.Enabled)
                {
                    Index--;
                    return true;
                }
                else if (keyData == Keys.Right && buttonNextPost.Enabled)
                {
                    Index++;
                    return true;
                }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void ChangePost(BooruPost Post)
        {
            _Post = Post;
            SetLoadingMode(true);
            if (_Post.Image == null)
                _Booru.GetImage(ref _Post);
            try
            {
                SetImage(_Post.Image.Bitmap);
                GUIHelper.Invoke(this, () =>
                    {
                        tagList.Tags = _Post.Tags;
                        Text = string.Format("# {0} - {1}x{2} - Views {3} - Added {4} by {5}", _Post.ID, _Post.Width, _Post.Height, _Post.ViewCount, _Post.CreationDate, _Post.User);
                    });
            }
            catch (ObjectDisposedException) { }
            SetLoadingMode(false);
        }

        private void SetLoadingMode(bool LoadingMode)
        {
            if (!InvokeRequired)
            {
                BooruUser cUser = _Booru.CurrentUser;
                buttonDeletePost.Enabled = !LoadingMode && cUser.CanDeletePosts;
                buttonSaveImage.Enabled = !LoadingMode;
                buttonSetWallpaper.Enabled = !LoadingMode;
                buttonEditPost.Enabled = !LoadingMode && cUser.CanEditPosts;
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
                buttonEditImage.Enabled = !LoadingMode && cUser.CanEditPosts && Helper.IsWindows();
                buttonClone.Enabled = !LoadingMode && cUser.CanAddPosts;
            }
            else Invoke(new Action<bool>(SetLoadingMode), LoadingMode);
        }

        private void buttonDeletePost_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really wan't to delete this post?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _Booru.DeletePost(_Post.ID);
                _PostIDs.Remove(_Post.ID);
                if (buttonNextPost.Enabled)
                    Index++;
                else if (buttonPreviousPost.Enabled)
                    Index--;
                else Close();
            }
        }

        private void buttonSaveImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = string.Format("image{0}.png", _Post.ID);
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
            else MessageBox.Show("An error occured. Wallpaper not changed.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonPreviousPost_Click(object sender, EventArgs e) { Index--; }

        private void buttonNextPost_Click(object sender, EventArgs e) { Index++; }

        private void buttonClone_Click(object sender, EventArgs e) { ClonePost(null); }

        private void ClonePost(BooruImage newImage = null)
        {
            BooruPost cPost = _Post.Clone() as BooruPost;
            if (_Booru.CurrentUser.AdvancePostControl)
            {
                cPost.CreationDate = DateTime.Now;
                cPost.EditCount = 0;
                cPost.ViewCount = 0;
            }
            string oldComment = cPost.Description;
            cPost.Description = "Cloned ID " + _Post.ID;
            if (!string.IsNullOrWhiteSpace(oldComment))
                cPost.Description += ", " + oldComment.Trim();
            if (newImage != null)
                cPost.Image = newImage;
            _Booru.AddPost(ref cPost);
            _PostIDs.Insert(0, cPost.ID);
            Index = 0;
        }

        private string getImageEditor()
        {
            //TODO Imag editor configurable paths
            List<string> editors = new List<string>();
            for (int i = 6; i > 2; i--)
            {
                editors.Add("C:\\Program Files\\Adobe\\Adobe Photoshop CS" + i + " (64 Bit)\\Photoshop.exe");
                editors.Add("C:\\Program Files (x86)\\Adobe\\Adobe Photoshop CS" + i + "\\Photoshop.exe");
            }
            editors.Add("C:\\Windows\\System32\\mspaint.exe");
            foreach (string editor in editors)
                if (File.Exists(editor))
                    return editor;
            return null;
        }

        private void buttonEditImage_Click(object sender, EventArgs e)
        {
            string editorEXE = getImageEditor();
            string tempFile = Helper.GetTempFile();
            _Post.Image.Save(ref tempFile, true);
            byte[] md5 = Helper.MD5OfFile(tempFile);
            Process editor = Process.Start(editorEXE, tempFile);
            editor.WaitForExit();
            bool isTheSame = Helper.MemoryCompare(md5, Helper.MD5OfFile(tempFile));
            if (isTheSame)
                if (MessageBox.Show("No changes detected (maybe you have saved the image with a different name). Dou you wan't to search for the image?", "ImageEdit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Title = "Load new image...";
                        ofd.Filter = "All files|*.*"; //TODO OFD Filter
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            tempFile = ofd.FileName;
                            isTheSame = false;
                        }
                    }
            if (!isTheSame)
            {
                DialogResult result = MessageBox.Show("Do you wan't to save the image in a cloned BooruPost (Yes) or in the same post (No)? ", "SharpBooru", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    _Post.Image = BooruImage.FromFile(tempFile);
                    _Booru.SaveImage(_Post);
                    SetImage(_Post.Image.Bitmap);
                }
                else if (result == DialogResult.Yes)
                    ClonePost(BooruImage.FromFile(tempFile));
            }
        }

        private void buttonEditPost_Click(object sender, EventArgs e)
        {
            using (EditDialog eDialog = new EditDialog())
            {
                GUIHelper.SetFormCentered(this, eDialog);
                if (eDialog.ShowDialog(_Booru, ref _Post) == DialogResult.OK)
                {
                    _Booru.SavePost(_Post);
                    ChangePost(_Booru.GetPost(_Post.ID));
                }
            }
        }

        private void SetImage(Bitmap Bitmap)
        {
            GUIHelper.Invoke(imageBox, () =>
                {
                    imageBox.Image = Bitmap;
                    imageBox.ZoomToFit();
                });
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (WindowState != _oldWindowState)
            {
                _oldWindowState = WindowState;
                imageBox.ZoomToFit();
            }
        }
    }
}