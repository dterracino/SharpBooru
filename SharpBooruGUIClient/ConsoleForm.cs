using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class ConsoleForm : Form
    {
        private Booru _Booru;

        public ConsoleForm(Booru Booru)
        {
            _Booru = Booru;
            InitializeComponent();
            this.textBoxCmd.KeyDown += textBoxCmd_KeyDown;
        }

        private void textBoxCmd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
                TabPressed();
            else if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrWhiteSpace(this.textBoxCmd.Text))
                    EnterPressed();
            }
            else if (e.KeyCode == Keys.Escape)
                this.textBoxCmd.Clear();
        }

        private void TabPressed()
        {
            //TODO Implement TAB completion
        }

        private void EnterPressed()
        {
            //TODO Implement command execution
        }
    }
}