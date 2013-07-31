using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
            textBoxUsername.KeyDown += new KeyEventHandler(textBox_KeyDown);
            textBoxPassword.KeyDown += new KeyEventHandler(textBox_KeyDown);
        }

        void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                buttonOK_Click(sender, e);
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