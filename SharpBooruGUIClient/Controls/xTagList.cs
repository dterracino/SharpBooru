using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public partial class xTagList : UserControl
    {
        private BooruTagList _Tags;

        //private int offset = (int)((Constants.Controls.TAGLIST_ITEM_HEIGHT - Constants.Controls.TAGLIST_FONT_HEIGHT) * 0.5 + 0.5);

        /// <summary>The tags, which should appear in the control</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BooruTagList Tags
        {
            get { return _Tags; }
            set
            {
                if (value != null)
                {
                    _Tags = value;
                    listBox1.Items.Clear();
                    _Tags.ForEach(x => listBox1.Items.Add(x.Tag));
                    listBox1.Height = (int)(listBox1.ItemHeight * listBox1.Items.Count + 0.4 * listBox1.ItemHeight + 0.5);
                    listBox1.Width = measureAllTagsMaximumWidth();
                    Refresh();
                }
            }
        }

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

        /// <summary>Creates a new TagList</summary>
        public xTagList()
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
            foreach (BooruTag tag in _Tags)
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
            Color color = Enabled ? Color.FromArgb(_Tags[e.Index].Color) : Color.LightGray;
            using (SolidBrush brush = new SolidBrush(color))
                e.Graphics.DrawString(_Tags[e.Index].Tag, e.Font, brush, p);
        }
    }
}