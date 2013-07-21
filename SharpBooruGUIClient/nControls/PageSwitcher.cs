using System;
using System.Drawing;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI.nControls
{
    public class PageSwitcher : Control
    {
        public event EventHandler PageChanged;

        private Button _LeftButton, _RightButton;
        private TextBox _TextBox;
        private int _Pages = 0;
        private int _CurrentPage = 0;

        public PageSwitcher()
        {
            Size elementSize = new Size(26, 26);
            int elementSpacing = 4;
            Size = new Size(3 * elementSize.Width + 2 * elementSpacing, elementSize.Height);
            _LeftButton = new Button()
            {
                Size = elementSize,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                Top = 0,
                Left = 0,
                Image = Properties.Resources.icon_arrow_left,
                Enabled = false
            };
            _RightButton = new Button()
            {
                AutoSize = false,
                Size = elementSize,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Top = 0,
                Left = 2 * (elementSize.Width + elementSpacing),
                Image = Properties.Resources.icon_arrow_right,
                Enabled = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _TextBox = new TextBox()
            {
                TextAlign= HorizontalAlignment.Center,
                Size = elementSize,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Left = elementSize.Width + elementSpacing,
                Text = "0",
            };
            _LeftButton.Click += (sender, e) => { CurrentPage--; };
            _RightButton.Click += (sender, e) => { CurrentPage++; };
            _TextBox.Click += (sender, e) => { _TextBox.SelectAll(); };
            _TextBox.KeyDown += new KeyEventHandler(_TextBox_KeyDown);
            _TextBox.MouseWheel += (sender, e) =>
                {
                    CurrentPage += e.Delta > 0 ? 1 : -1;
                    _TextBox.Focus();
                };
            EventHandler resizeHandler = (sender, e) => { _TextBox.Top = (int)(0.5d * (this.Height - _TextBox.Height) + 0.5d); };
            Resize += resizeHandler;
            resizeHandler(this, null);
            this.Controls.Add(_LeftButton);
            this.Controls.Add(_RightButton);
            this.Controls.Add(_TextBox);
        }

        private void _TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                CurrentPage++;
            else if (e.KeyCode == Keys.Down)
                CurrentPage--;
            else if (e.KeyCode == Keys.Enter)
            {
                int? page = GetTextBoxValue();
                if (page.HasValue)
                    CurrentPage = page.Value - 1;
                CheckControls();
                _TextBox.SelectAll();
            }
        }

        private int? GetTextBoxValue()
        {
            int value = -1;
            if (int.TryParse(_TextBox.Text, out value))
                return value;
            else return null;
        }

        public int CurrentPage
        {
            get { return _CurrentPage; }
            set
            {
                if (_Pages < 1 || value < 0)
                    value = 0;
                else if (!(value < _Pages))
                    value = _Pages - 1;
                if (value != _CurrentPage)
                {
                    _CurrentPage = value;
                    CheckControls();
                    FirePageChangedEvent();
                }
            }
        }

        public int Pages
        {
            get { return _Pages; }
            set
            {
                if (value < 0)
                    _Pages = 0;
                else _Pages = value;
                _CurrentPage = 0;
                CheckControls();
            }
        }

        private void CheckControls()
        {
            _LeftButton.Enabled = _CurrentPage > 0;
            _RightButton.Enabled = _CurrentPage < _Pages - 1;
            if (_Pages > 0)
                _TextBox.Text = (_CurrentPage + 1).ToString();
            else _TextBox.Text = "0";
        }

        private void FirePageChangedEvent()
        {
            if (PageChanged != null)
                PageChanged(this, new EventArgs());
        }
    }
}