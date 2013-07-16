using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public partial class TagList : UserControl
    {
        private Booru _Booru = null;
        private BooruPost _Post = null;

        //private int offset = (int)((Constants.Controls.TAGLIST_ITEM_HEIGHT - Constants.Controls.TAGLIST_FONT_HEIGHT) * 0.5 + 0.5);

        /// <summary>The tags, which should appear in the control</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BooruPost Post
        {
            get { return _Post; }
            set
            {
                if (value != null)
                {
                    _Post = value;
                    listBox1.Items.Clear();
                    _Post.Tags.ForEach(x => listBox1.Items.Add(x.Tag));
                    listBox1.Height = (int)(listBox1.ItemHeight * listBox1.Items.Count + 0.4 * listBox1.ItemHeight + 0.5);
                    listBox1.Width = measureAllTagsMaximumWidth();
                    if (tagTextBox1.Visible)
                        ToggleEditMode(false);
                    Refresh();
                }
            }
        }

        public override bool Focused { get { return tagTextBox1.Visible && tagTextBox1.Focused; } }

        /// <summary>The background color of the control</summary>
        public new Color BackColor
        {
            get { return listBox1.BackColor; }
            set { listBox1.BackColor = value; }
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                Refresh();
            }
        }

        /*
        public ListBox InternalListBox
        {
            get { return listBox1; }
            set { if (value != null) listBox1 = value; }
        }
        */

        /// <summary>Creates a new TagList</summary>
        public TagList()
        {
            InitializeComponent();
            base.BackColor = Color.Transparent;
            panel1.Location = new Point(0, 0);
            panel1.BackColor = Color.Transparent;
            panel1.Dock = DockStyle.Fill;
            panel1.AutoScroll = true;
            panel1.Scroll += (ScrollEventHandler)delegate { listBox1.Invalidate(); };
            listBox1.Location = new Point(0, 0);
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.Font = new Font("Verdana", 14f, GraphicsUnit.Pixel);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.Enabled = false;
            //listBox1.MeasureItem += new MeasureItemEventHandler(listBox1_MeasureItem);
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            panel1.DoubleClick += new EventHandler((sender, e) => ToggleEditMode(false));
            tagTextBox1.Location = new Point(0, tagTextBox1.Location.Y);
            tagTextBox1.Size = new Size(Width, Height - tagTextBox1.Location.Y);
        }

        public void SetTagListBoxParams(List<BooruTag> Tags, Control Parent)
        {
            if (Tags != null)
                if (Tags.Count > 0)
                    tagTextBox1.SetTags(Tags);
            tagTextBox1.ListBoxParent = Parent;
        }

        public void SetBooru(Booru Booru) { _Booru = Booru; }

        public void ToggleEditMode(bool SaveTags = false)
        {
            if (_Booru != null)
                if (tagTextBox1.Visible)
                {
                    tagTextBox1.Visible = false;
                    closeButton.Visible = false;
                    okButton.Visible = false;
                    panel1.Visible = true;
                    if (SaveTags)
                    {
                        _Post.Tags = new BooruTagList();
                        string tagString = tagTextBox1.Text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').ToLower();
                        foreach (string str_tag in tagString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                            _Post.Tags.Add(new BooruTag(str_tag));
                        _Post.EditCount++;
                    }
                }
                else
                {
                    List<string> stringTags = new List<string>();
                    _Post.Tags.ForEach(x => stringTags.Add(x.Tag));
                    tagTextBox1.Text = string.Join(Environment.NewLine, stringTags);
                    panel1.Visible = false;
                    closeButton.Visible = true;
                    okButton.Visible = true;
                    tagTextBox1.Visible = true;
                    tagTextBox1.Focus();
                }
        }

        /*
        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = (int)(Constants.Controls.TAGLIST_ITEM_HEIGHT + 0.5);
            e.ItemWidth = measureString(_Tags[e.Index]);
        }
        */

        private int measureAllTagsMaximumWidth()
        {
            int max = 0;
            foreach (BooruTag tag in _Post.Tags)
            {
                int measure = TextRenderer.MeasureText(tag.Tag, listBox1.Font).Width;
                if (measure > max)
                    max = measure;
            }
            return max;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            //e.DrawBackground();
            //e.Graphics.FillRectangle(new SolidBrush(listBox1.BackColor), e.Bounds);
            Point p = e.Bounds.Location;
            //p.X += offset;
            //p.Y += offset;
            Color color = Enabled ? Color.FromArgb(_Post.Tags[e.Index].Color) : Color.LightGray;
            using (SolidBrush brush = new SolidBrush(color))
                e.Graphics.DrawString(_Post.Tags[e.Index].Tag, e.Font, brush, p);
        }

        private void closeButton_Click(object sender, EventArgs e) { ToggleEditMode(false); }

        private void okButton_Click(object sender, EventArgs e) { ToggleEditMode(true); }
    }
}