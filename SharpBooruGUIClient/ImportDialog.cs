using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

namespace TA.SharpBooru.Client.GUI
{
    public partial class ImportDialog : Form
    {
        private Booru _Booru;
        private BooruTagList _AllTags;

        public ImportDialog(Booru Booru)
        {
            _Booru = Booru;
            InitializeComponent();
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.UserDeletedRow += (sender, e) => CheckAndEditCheckedListBox();
            dataGridView.CellValueChanged += (sender, e) =>
                {
                    if (dataGridView.Columns[e.ColumnIndex].Name == "tags")
                        CheckAndEditCheckedListBox();
                };
            _AllTags = _Booru.GetAllTags();
            sharedTagsTagTextBox.SetTags(_AllTags);
            sharedTagsTagTextBox.ListBoxParent = this;
            sharedTagsTagTextBox.Dock = DockStyle.Fill;
            sharedTagsTagTextBox.LostFocus += (sender, e) => CheckAndEditCheckedListBox();
            splitContainer2.Dock = DockStyle.Fill;
            allowedTagsCheckedListBox.Dock = DockStyle.Fill;
            allowedTagsCheckedListBox.Sorted = true;
            dataGridView.Dock = DockStyle.Fill;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DataGridViewRowCollection rows = dataGridView.Rows;
            PleaseWaitDialog pwd = new PleaseWaitDialog();
            GUIHelper.SetFormCentered(this, pwd);
            GUIHelper.EnableWindow(this, false);
            pwd.Show();
            Thread thread = new Thread(() =>
                {
                    GUIHelper.Invoke(this, () =>
                        {
                            okButton.Enabled = false;
                            sharedTagsTagTextBox.Enabled = false;
                        });
                    pwd.Marquee = true;
                    CheckAndEditCheckedListBox();
                    pwd.Description = "Downloading images if neccessary...";
                    pwd.Marquee = false;
                    pwd.Max = rows.Count;
                    foreach (DataGridViewRow row in rows)
                    {
                        string imgPath = (string)row.Cells["image_file"].Value;
                        imgPath = imgPath.Trim();
                        if (Helper.CheckURL(imgPath))
                        {
                            imgPath = Helper.DownloadTemporary(imgPath);
                            row.Cells["image_file"].Value = imgPath;
                        }
                        pwd.Value++;
                    }
                    pwd.Description = "Saving posts to database...";
                    pwd.Value = 0;
                    List<DataGridViewRow> skippedRows = new List<DataGridViewRow>();
                    foreach (DataGridViewRow row in rows)
                    {
                        string imgPath = (string)row.Cells["image_file"].Value;
                        BooruPost post = new BooruPost();
                        try
                        {
                            BooruImage bImg = BooruImage.FromFileOrURL(imgPath);
                            if (bImg != null)
                                post.Image = bImg;
                            else throw new FileNotFoundException("Image");
                            post.Source = (string)row.Cells["source"].Value;
                            post.Comment = (string)row.Cells["comment"].Value;
                            post.Rating = Convert.ToByte(row.Cells["rating"].Value);
                            string tag_string = Convert.ToString(row.Cells["tags"].Value) + " " + sharedTagsTagTextBox.Text;
                            post.Tags = BooruTagList.FromString(tag_string);
                            RemoveUncheckedTagsFromBooruTagList(ref post.Tags);
                            _Booru.AddPost(post);
                        }
                        catch { skippedRows.Add(row); }
                        pwd.Value++;
                    }
                    pwd.Description = "Cleaning up the importer window...";
                    pwd.Marquee = true;
                    GUIHelper.Invoke(this, () =>
                        {
                            okButton.Enabled = true;
                            sharedTagsTagTextBox.Enabled = true;
                            dataGridView.Rows.Clear();
                            if (skippedRows.Count > 0)
                            {
                                dataGridView.Rows.AddRange(skippedRows.ToArray());
                                MessageBox.Show("There were errors saving some images, some rows are still left", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        });
                    CheckAndEditCheckedListBox();
                    pwd.Close();
                    GUIHelper.EnableWindow(this, true);
                });
            thread.Start();
        }

        public void AddRowFile(string File)
        {
            if (System.IO.File.Exists(File))
                using (BooruImage bm = BooruImage.FromFile(File))
                    if (bm != null)
                        AddRowInternal(
                            File,
                            string.Empty,
                            File,
                            "Imported from file",
                            null,
                            bm);
        }

        public void AddRowURL(string URL)
        {
            using (BooruImage bm = BooruImage.FromURL(URL))
                if (bm != null)
                    AddRowInternal(
                        URL,
                        string.Empty,
                        URL,
                        "Imported from internet",
                        null,
                        bm);
        }

        public void AddRowAPIPost(BooruAPIPost Post)
        {
            using (BooruImage thumb = BooruImage.FromURL(Post.ThumbnailURL))
                if (thumb != null)
                    AddRowInternal(
                        Post.ImageURL,
                        Post.Tags.ToString(),
                        Post.SourceURL,
                        "Imported from " + Post.APIName,
                        null,
                        thumb);
        }

        private object _AddRowInternalLock = new object();
        private void AddRowInternal(string file, string tags, string source, string comment, byte? rating, BooruImage thumb)
        {
            if (!rating.HasValue)
                rating = 7; //TODO StandardRating
            Bitmap thumbBitmap = null;
            try { thumbBitmap = thumb.CreateThumbnail(256).Bitmap; } //TODO ThumbnailSize
            catch { thumbBitmap = null; }
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = file.Trim() });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = tags });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = source });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = comment });
            /*
            DataGridViewComboBoxCell ratingCell = new DataGridViewComboBoxCell();
            BooruHelper.GetAllRatings(booru.wrapper).ForEach(x => ratingCell.Items.Add(x));
            if (!rating.HasValue)
                rating = booru.GetProperty<byte>(Booru.Property.StandardRating);
            ratingCell.Value = ratingCell.Items[rating.Value - 1];
            row.Cells.Add(ratingCell);
            */ //TODO RatingCell
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = rating });
            if (thumbBitmap != null)
                row.Cells.Add(new DataGridViewImageCell() { Value = thumbBitmap });
            else row.Cells.Add(new DataGridViewTextBoxCell() { Value = "ERROR" });
            lock (_AddRowInternalLock)
            {
                GUIHelper.Invoke(dataGridView, () => dataGridView.Rows.Add(row));
                GUIHelper.Invoke(allowedTagsCheckedListBox, () =>
                    BooruTagList.FromString(tags).ForEach(x =>
                        {
                            if (!allowedTagsCheckedListBox.Items.Contains(x.Tag))
                                allowedTagsCheckedListBox.Items.Add(x.Tag, _AllTags.Contains(x));
                        })
                );
            }
        }

        private void CheckAndEditCheckedListBox()
        {
            BooruTagList tags = null;
            GUIHelper.Invoke(sharedTagsTagTextBox, () => { tags = BooruTagList.FromString(sharedTagsTagTextBox.Text); });
            GUIHelper.Invoke(dataGridView, () =>
                {
                    foreach (DataGridViewRow row in dataGridView.Rows)
                        tags.AddRange(BooruTagList.FromString(Convert.ToString(row.Cells["tags"].Value)));
                });
            GUIHelper.Invoke(allowedTagsCheckedListBox, () =>
                {
                    for (int i = allowedTagsCheckedListBox.Items.Count - 1; !(i < 0); i--)
                        if (!tags.Contains(Convert.ToString(allowedTagsCheckedListBox.Items[i])))
                            allowedTagsCheckedListBox.Items.RemoveAt(i);
                    foreach (BooruTag tag in tags)
                        if (!allowedTagsCheckedListBox.Items.Contains(tag.Tag))
                            allowedTagsCheckedListBox.Items.Add(tag.Tag, _AllTags.Contains(tag));
                });
        }

        private void RemoveUncheckedTagsFromBooruTagList(ref BooruTagList TagList)
        {
            CheckedListBox.CheckedItemCollection checkedItems = null;
            GUIHelper.Invoke(allowedTagsCheckedListBox, () => { checkedItems = allowedTagsCheckedListBox.CheckedItems; });
            for (int i = TagList.Count - 1; !(i < 0); i--)
                if (!checkedItems.Contains(TagList[i].Tag))
                    TagList.RemoveAt(i);
        }

        private void MultiEditPostDialog_DragEnter(object sender, DragEventArgs e) { e.Effect = DragDropEffects.Copy; }

        private void MultiEditPostDialog_DragDrop(object sender, DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();
            if (e.Data.GetDataPresent("text/x-moz-url"))
            {
                //Link from firefox
                byte[] data = ((System.IO.MemoryStream)e.Data.GetData("text/x-moz-url")).ToArray();
                string str = Encoding.Unicode.GetString(data).Trim();
                str = str.Substring(0, str.IndexOf('\n'));
                if (Helper.CheckURL(str))
                {
                    List<BooruAPIPost> api_posts = BooruAPI.SearchPostsPerURL(str);
                    if (api_posts != null)
                        foreach (BooruAPIPost api_post in api_posts)
                            AddRowAPIPost(api_post);
                    else AddRowURL(str);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //File from windows explorer
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    if (File.Exists(file))
                        AddRowFile(file);
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat) ||
                     e.Data.GetDataPresent(DataFormats.Text))
            {
                //Text/String with file path or URL
                string data = Convert.ToString(e.Data.GetData(DataFormats.StringFormat));
                if (File.Exists(data))
                    AddRowFile(data);
                else if (Helper.CheckURL(data))
                    AddRowURL(data);
            }
        }

        private void buttonPasteClipboard_Click(object sender, EventArgs e)
        {
            string cbText = Clipboard.GetText();
            if (Helper.CheckURL(cbText))
            {
                List<BooruAPIPost> api_posts = BooruAPI.SearchPostsPerURL(cbText);
                if (api_posts != null)
                    foreach (BooruAPIPost api_post in api_posts)
                        AddRowAPIPost(api_post);
                else AddRowURL(cbText);
            }
            else MessageBox.Show("No URL found", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}