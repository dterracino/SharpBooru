using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public class TagTextBox : TextBox
    {
        public event EventHandler EnterPressed;

        private ListBox _listBox;
        private string _formerValue = string.Empty;
        private List<string> _Tags;
        private char[] ws = new char[4] { ' ', '\r', '\n', '\t' };

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> Tags
        {
            get { return _Tags; }
            set { _Tags = value == null ? new List<string>() : value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control ListBoxParent
        {
            get
            {
                if (_listBox.Parent == null)
                    _listBox.Parent = Parent;
                return _listBox.Parent;
            }
            set { _listBox.Parent = value ?? Parent; }
        }

        public TagTextBox()
        {
            _listBox = new ListBox();
            _listBox.DoubleClick += new EventHandler(_listBox_DoubleClick);
            _listBox.LostFocus += new EventHandler(_LostFocus);
            _listBox.Sorted = true;
            ScrollBars = ScrollBars.Vertical;
            LostFocus += new EventHandler(_LostFocus);
            KeyDown += this_KeyDown;
            KeyUp += this_KeyUp;
            ListBoxParent = null;
            AcceptsTab = false;
            ResetListBox();
        }

        private void _LostFocus(object sender, EventArgs e)
        {
            if (!(_listBox.Focused || this.Focused))
                ResetListBox();
        }

        void _listBox_DoubleClick(object sender, EventArgs e) { this_KeyDown(sender, new KeyEventArgs(Keys.Tab)); }

        public void SetTags(List<BooruTag> Tags)
        {
            if (Tags == null)
                this.Tags = null;
            else
            {
                List<string> tmp = new List<string>();
                foreach (BooruTag tag in Tags)
                    tmp.Add(tag.Tag);
                this.Tags = tmp;
            }
        }

        private void ShowListBox()
        {
            Point p1 = PointToScreen(new Point(0, Height));
            Point p2 = ListBoxParent.PointToScreen(new Point(2, 2));
            _listBox.Location = Point.Subtract(p1, new Size(p2));
            _listBox.Visible = true;
            _listBox.BringToFront();
        }

        public void ResetListBox() { _listBox.Visible = false; }

        private void this_KeyUp(object sender, KeyEventArgs e) { UpdateListBox(); }

        private void handleInsert()
        {
            if (_listBox.Visible)
            {
                string selected = (string)_listBox.SelectedItem;
                if (selected != "...")
                {
                    InsertWord(selected);
                    ResetListBox();
                    _formerValue = Text;
                }
            }
        }

        private void this_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    if (!Multiline)
                        handleInsert();
                    break;
                case Keys.Down:
                    if ((_listBox.Visible) && (_listBox.SelectedIndex < _listBox.Items.Count - 1))
                        _listBox.SelectedIndex++;
                    break;
                case Keys.Up:
                    if ((_listBox.Visible) && (_listBox.SelectedIndex > 0))
                        _listBox.SelectedIndex--;
                    break;
                case Keys.Enter:
                    if (!Multiline)
                        handleInsert();
                    if (_listBox.Visible)
                        ResetListBox();
                    if (EnterPressed != null)
                        EnterPressed(this, e);
                    break;
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab && Multiline)
            {
                handleInsert();
                return true;
            }
            else return false;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Tab)
                if (!Multiline)
                    return true;
            return base.IsInputKey(keyData);
        }

        private void UpdateListBox()
        {
            if (Text == _formerValue) 
                return;
            _formerValue = Text;
            string word = GetWord();
            bool negate = word.StartsWith("-");
            if (Tags != null && word.Length > 0)
            {
                List<string> matches = Tags.FindAll(x => x.ToLower().Contains(negate ? word.Substring(1) : word));
                if (matches.Count > 0)
                {
                    ShowListBox();
                    _listBox.Items.Clear();
                    if (matches.Count > 15)
                    {
                        matches = matches.GetRange(0, 14);
                        matches.Add("...");
                    }
                    matches.ForEach(x => _listBox.Items.Add(negate ? "-" + x : x));
                    int maybeAbsoluteMatchIndex = _listBox.Items.IndexOf(word);
                    _listBox.SelectedIndex = maybeAbsoluteMatchIndex < 0 ? 0 : maybeAbsoluteMatchIndex;
                    _listBox.Height = 0;
                    Focus();
                    using (Graphics graphics = _listBox.CreateGraphics())
                    {
                        for (int i = 0; i < _listBox.Items.Count; i++)
                        {
                            _listBox.Height += _listBox.GetItemHeight(i);
                            int itemWidth = (int)graphics.MeasureString(((string)_listBox.Items[i]) + "_", _listBox.Font).Width;
                            _listBox.Width = (_listBox.Width < itemWidth) ? itemWidth : _listBox.Width;
                        }
                    }
                }
                else ResetListBox();
            }
            else ResetListBox();
        }

        private string GetWord()
        {
            string text = Text;
            int pos = SelectionStart;
            int startPost = pos < 1 ? 0 : pos - 1;
            int posStart = text.LastIndexOfAny(ws, startPost);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOf(' ', pos);
            posEnd = (posEnd == -1) ? text.Length : posEnd;
            int length = ((posEnd - posStart) < 0) ? 0 : posEnd - posStart;
            return text.Substring(posStart, length).Trim();
        }

        private void InsertWord(string newTag)
        {
            string text = Text;
            int pos = SelectionStart;
            int posStart = text.LastIndexOfAny(ws, (pos < 1) ? 0 : pos - 1);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOfAny(ws, pos);
            string firstPart = text.Substring(0, posStart) + newTag;
            string updatedText = firstPart + ((posEnd == -1) ? "" : text.Substring(posEnd, text.Length - posEnd));
            Text = updatedText;
            SelectionStart = firstPart.Length;
        }

        public List<string> SelectedValues
        {
            get
            {
                string[] result = Text.Split(ws, StringSplitOptions.RemoveEmptyEntries);
                return new List<string>(result);
            }
        }
    }
}