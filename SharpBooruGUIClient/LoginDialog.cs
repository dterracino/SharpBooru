using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
            textBoxUsername.KeyDown += keyDown;
            textBoxPassword.KeyDown += keyDown;
            buttonOK.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                        Close();
                };
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                buttonOK_Click(sender, e);
            else if (e.KeyCode == Keys.Escape)
                Close();
        }

        public string Username
        {
            get { return textBoxUsername.Text ?? string.Empty; }
            set { textBoxUsername.Text = value ?? string.Empty; }
        }

        public string Password
        {
            get { return textBoxPassword.Text ?? string.Empty; }
            set { textBoxPassword.Text = value ?? string.Empty; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}