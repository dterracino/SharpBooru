using System;
using System.Drawing;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI.Controls
{
    public class AjaxLoading : Panel
    {
        private double _Value;
        public double Value
        {
            get { return _Value; }
            set
            {
                if (value > 1)
                    _Value = 1;
                else if (value < 0)
                    _Value = 0;
                else _Value = value;
                MethodInvoker mInvoker = () => { progressLabel.Text = string.Format("{0}%", (int)(_Value * 100)); };
                if (progressLabel.InvokeRequired)
                    progressLabel.Invoke(mInvoker);
                else mInvoker();
            }
        }

        private Label progressLabel;

        public AjaxLoading()
        {
            PictureBox pictureBox = new PictureBox()
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.ajax_loading,
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            progressLabel = new Label()
            {
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            progressLabel.Parent = pictureBox;
            Value = 0;
            this.Controls.Add(pictureBox);
            BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
