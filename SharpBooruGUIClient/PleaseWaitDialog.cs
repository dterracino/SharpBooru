using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class PleaseWaitDialog : Form
    {
        private bool codeClose = false;

        public PleaseWaitDialog()
        {
            InitializeComponent();
            FormClosing += (sender, e) => { e.Cancel = !codeClose; };
        }

        public bool Marquee
        {
            get { return progressBar1.Style == ProgressBarStyle.Marquee; }
            set { GUIHelper.Invoke(progressBar1, () => { progressBar1.Style = value ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous; }); }
        }

        public int Min
        {
            get { return progressBar1.Minimum; }
            set
            {
                if (!(value > Max))
                    GUIHelper.Invoke(progressBar1, () => { progressBar1.Minimum = value; });
            }
        }

        public int Max
        {
            get { return progressBar1.Maximum; }
            set
            {
                if (!(value < Min))
                    GUIHelper.Invoke(progressBar1, () => { progressBar1.Maximum = value; });
            }
        }

        public int Value
        {
            get { return progressBar1.Value; }
            set
            {
                if (!(value < Min || value > Max))
                    GUIHelper.Invoke(progressBar1, () => { progressBar1.Value = value; });
            }
        }

        public string Description
        {
            get { return label1.Text; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    value = string.Empty;
                GUIHelper.Invoke(label1, () => { label1.Text = value; });
            }
        }

        public string Title
        {
            get { return Text; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    GUIHelper.Invoke(this, () => { Text = value; });
            }
        }

        public static PleaseWaitDialog Show(string Title, string Description, bool Marquee)
        {
            PleaseWaitDialog pwd = new PleaseWaitDialog()
            {
                Title = Title,
                Description = Description,
                Marquee = Marquee
            };
            pwd.Show();
            return pwd;
        }

        public new void Close()
        {
            codeClose = true;
            GUIHelper.Invoke(this, () => { base.Close(); });
            codeClose = false;
        }

        public void ShowDialogNonBlocking(Control InvokeTarget)
        {
            Func<DialogResult> showDialogFunc = this.ShowDialog;
            InvokeTarget.BeginInvoke(showDialogFunc);
        }
    }
}