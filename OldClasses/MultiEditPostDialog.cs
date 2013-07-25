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
    public partial class MultiEditPostDialog : Form
    {
        private Booru booru;
        private System.Windows.Forms.Timer timer;

        public MultiEditPostDialog(Booru Booru)
        {
            booru = Booru;
            InitializeComponent();
            dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.UserDeletedRow += new DataGridViewRowEventHandler((sender, e) => CheckAndEditCheckedListBox());
            dataGridView.CellValueChanged += new DataGridViewCellEventHandler((sender, e) =>
                {
                    if (dataGridView.Columns[e.ColumnIndex].Name == "tags")
                        CheckAndEditCheckedListBox();
                });
            sharedTagsTagTextBox.SetTags(booru.SearchTags());
            sharedTagsTagTextBox.ListBoxParent = this;
            sharedTagsTagTextBox.Dock = DockStyle.Fill;
            sharedTagsTagTextBox.LostFocus += new EventHandler((sender, e) => CheckAndEditCheckedListBox());
            splitContainer2.Dock = DockStyle.Fill;
            allowedTagsCheckedListBox.Dock = DockStyle.Fill;
            allowedTagsCheckedListBox.Sorted = true;
            dataGridView.Dock = DockStyle.Fill;
            timer = new System.Windows.Forms.Timer() { Interval = 200, Enabled = false };
            timer.Tick += new EventHandler(timer_Elapsed);
            monitorClipboardCheckBox.CheckedChanged += new EventHandler((sender, e) => timer.Enabled = monitorClipboardCheckBox.Enabled);
        }

        private string oldCB = string.Empty;
        [STAThread]
        private void timer_Elapsed(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                if (text != null)
                    if (text != oldCB)
                        if (Helper.CheckURL(text))
                        {
                            List<BooruAPIPost> api_posts = BooruAPI.SearchPostsPerURL(text);
                            if (api_posts != null)
                                foreach (BooruAPIPost api_post in api_posts)
                                    AddRowAPIPost(api_post);
                            else AddRowURL(text);
                            oldCB = text;
                        }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DataGridViewRowCollection rows = dataGridView.Rows;
            PleaseWaitDialog pwd = new PleaseWaitDialog();
            Helper.SetFormCentered(this, pwd);
            Helper.EnableWindow(this, false);
            pwd.Show();
            Thread thread = new Thread(() =>
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        okButton.Enabled = false;
                        sharedTagsTagTextBox.Enabled = false;
                    }));
                    pwd.Marquee = true;
                    pwd.Description = "Loading and preparing tags...";
                    CheckAndEditCheckedListBox();
                    Dictionary<string, BooruTag> bTags = new Dictionary<string, BooruTag>();
                    foreach (int chIndex in allowedTagsCheckedListBox.CheckedIndices)
                    {
                        string tagString = Convert.ToString(allowedTagsCheckedListBox.Items[chIndex]);
                        BooruTag tag = new BooruTag(booru, tagString);
                        bTags.Add(tagString, tag);
                    }
                    pwd.Description = "Downloading images if neccessary...";
                    pwd.Marquee = false;
                    pwd.Max = rows.Count;
                    foreach (DataGridViewRow row in rows)
                    {
                        string imgPath = (string)row.Cells["image_file"].Value;
                        imgPath = imgPath.Trim();
                        if (!imgPath.StartsWith("ID_"))
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
                        BooruPost post = null;
                        if (imgPath.ToUpper().StartsWith("ID_"))
                        {
                            post = new BooruPost(booru.wrapper, Convert.ToInt32(imgPath.Substring(3)));
                            post.EditCount++;
                        }
                        else post = new BooruPost(booru);
                        try
                        {
                            if (!imgPath.StartsWith("ID_"))
                            {
                                BooruImage bImg = BooruImage.FromFileOrURL(imgPath);
                                if (bImg != null)
                                    post.Image = bImg;
                                else throw new FileNotFoundException("Image");
                            }
                            post.Source = (string)row.Cells["source"].Value;
                            post.Comment = (string)row.Cells["comment"].Value;
                            string str_rating = row.Cells["rating"].Value.ToString();
                            int rating = BooruHelper.GetAllRatings(booru.wrapper).IndexOf(str_rating) + 1;
                            if (rating < 1 || rating > byte.MaxValue)
                                rating = booru.GetProperty<int>(Booru.Property.StandardRating);
                            post.Rating = (byte)rating;
                            string tag_string = Convert.ToString(row.Cells["tags"].Value) + " " + sharedTagsTagTextBox.Text;
                            List<string> str_tags = BooruHelper.TagsFromString(tag_string);
                            RemoveUncheckedTagsFromStringList(ref str_tags);
                            List<BooruTag> booru_tags = new List<BooruTag>();
                            str_tags.ForEach(x => booru_tags.Add(bTags[x]));
                            post.Tags = booru_tags;
                        }
                        catch
                        {
                            if (!imgPath.ToUpper().StartsWith("ID_"))
                                booru.DeletePost(post.ID);
                            else post.EditCount++;
                            skippedRows.Add(row);
                        }
                        pwd.Value++;
                    }
                    pwd.Description = "Cleaning up the importer window...";
                    pwd.Marquee = true;
                    this.Invoke(new MethodInvoker(delegate
                        {
                            okButton.Enabled = true;
                            sharedTagsTagTextBox.Enabled = true;
                            dataGridView.Rows.Clear();
                            if (skippedRows.Count > 0)
                            {
                                dataGridView.Rows.AddRange(skippedRows.ToArray());
                                MessageBox.Show("There were errors saving some images, some rows are still left", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }));
                    CheckAndEditCheckedListBox();
                    pwd.Close();
                    Helper.EnableWindow(this, true);
                });
            thread.Start();
        }

        public void AddRowPost(BooruPost Post)
        {
            List<string> tags = new List<string>();
            Post.Tags.ForEach(x => tags.Add(x.Tag));
            AddRowInternal(
                string.Format("ID_{0}", Post.ID),
                string.Join(" ", tags),
                Post.Source,
                Post.Comment,
                Post.Rating,
                Post.Thumbnail);
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

        public void AddRowAPIPost(BooruAPIs.BooruAPIPost Post)
        {
            using (BooruImage thumb = BooruImage.FromURL(Post.ThumbnailURL))
                if (thumb != null)
                    AddRowInternal(
                        Post.ImageURL,
                        string.Join(" ", Post.Tags),
                        Post.SourceURL,
                        "Imported from " + Post.APIName,
                        booru.GetProperty<byte>(Booru.Property.StandardRating),
                        thumb);
        }

        private object _AddRowInternalLock = new object();
        private void AddRowInternal(string file, string tags, string source, string comment, byte? rating, BooruImage thumb)
        {
            if (!rating.HasValue)
                rating = booru.GetProperty<byte>(Booru.Property.StandardRating);
            Bitmap thumbBitmap = null;
            try { thumbBitmap = BitmapFactory.CreateThumbnail(thumb).Bitmap; }
            catch { thumbBitmap = null; }
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = file.Trim() });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = tags });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = source });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = comment });
            DataGridViewComboBoxCell ratingCell = new DataGridViewComboBoxCell();
            BooruHelper.GetAllRatings(booru.wrapper).ForEach(x => ratingCell.Items.Add(x));
            if (!rating.HasValue)
                rating = booru.GetProperty<byte>(Booru.Property.StandardRating);
            ratingCell.Value = ratingCell.Items[rating.Value - 1];
            row.Cells.Add(ratingCell);
            if (thumbBitmap != null)
                row.Cells.Add(new DataGridViewImageCell() { Value = thumbBitmap });
            else row.Cells.Add(new DataGridViewTextBoxCell() { Value = "ERROR" });
            lock (_AddRowInternalLock)
            {
                MethodInvoker invoker = (MethodInvoker)delegate { dataGridView.Rows.Add(row); };
                if (dataGridView.InvokeRequired) dataGridView.Invoke(invoker); else invoker();
                invoker = (MethodInvoker)delegate
                    {
                        BooruHelper.TagsFromString(tags).ForEach(x =>
                            {
                                if (!allowedTagsCheckedListBox.Items.Contains(x))
                                    allowedTagsCheckedListBox.Items.Add(x, BooruTag.Exists(booru, x, true));
                            });
                    };
                if (allowedTagsCheckedListBox.InvokeRequired) allowedTagsCheckedListBox.Invoke(invoker); else invoker();
            }
        }

        private void CheckAndEditCheckedListBox()
        {
            List<string> tags = null;
            MethodInvoker invoker = (MethodInvoker)delegate { tags = BooruHelper.TagsFromString(sharedTagsTagTextBox.Text); };
            if (sharedTagsTagTextBox.InvokeRequired) sharedTagsTagTextBox.Invoke(invoker); else invoker();
            invoker = (MethodInvoker)delegate
                {
                    foreach (DataGridViewRow row in dataGridView.Rows)
                        tags.AddRange(BooruHelper.TagsFromString(Convert.ToString(row.Cells["tags"].Value)));
                };
            if (dataGridView.InvokeRequired) dataGridView.Invoke(invoker); else invoker();
            invoker = (MethodInvoker)delegate
                {
                    for (int i = allowedTagsCheckedListBox.Items.Count - 1; !(i < 0); i--)
                        if (!tags.Contains(Convert.ToString(allowedTagsCheckedListBox.Items[i])))
                            allowedTagsCheckedListBox.Items.RemoveAt(i);
                    foreach (string tag in tags)
                        if (!allowedTagsCheckedListBox.Items.Contains(tag))
                            allowedTagsCheckedListBox.Items.Add(tag, BooruTag.Exists(booru, tag, true));
                };
            if (allowedTagsCheckedListBox.InvokeRequired) allowedTagsCheckedListBox.Invoke(invoker); else invoker();
        }

        private void RemoveUncheckedTagsFromStringList(ref List<string> StringList)
        {
            CheckedListBox.CheckedItemCollection checkedItems = null;
            MethodInvoker invoker = (MethodInvoker)delegate { checkedItems = allowedTagsCheckedListBox.CheckedItems; };
            if (allowedTagsCheckedListBox.InvokeRequired) allowedTagsCheckedListBox.Invoke(invoker); else invoker();
            for (int i = StringList.Count - 1; !(i < 0); i--)
                if (!checkedItems.Contains(StringList[i]))
                    StringList.RemoveAt(i);
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
                    else
                        AddRowURL(str);
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

        private void button1_Click(object sender, EventArgs e)
        {
            Invoke((Action)(() => button1.Enabled = false));
            CheckAndEditCheckedListBox();
            foreach (int chIndex in allowedTagsCheckedListBox.CheckedIndices)
            {
                string tagString = Convert.ToString(allowedTagsCheckedListBox.Items[chIndex]);
                if (!BooruTag.Exists(booru, tagString, true))
                { BooruTag tag = new BooruTag(booru, tagString); }
            }
            Invoke((Action)(() => button1.Enabled = true));
        }
    }
}