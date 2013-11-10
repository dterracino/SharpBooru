using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class LoginDialog : Form
    {
        private bool _IsAdminMode = false;


        public LoginDialog(bool CurrentUserIsAdmin = false)
        {
            InitializeComponent();
            if (CurrentUserIsAdmin)
            {
                _IsAdminMode = true;
                textBoxPassword.Enabled = false;
            }
            else
            {
                textBoxPassword.KeyDown += keyDown;
                textBoxPassword.TextChanged += textChanged;
            }
            textBoxUsername.KeyDown += keyDown;
            textBoxUsername.TextChanged += textChanged;
            buttonOK.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                        Close();
                };
            textChanged(this, EventArgs.Empty);
        }

        private void textChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = !string.IsNullOrWhiteSpace(Username) 
                && (!string.IsNullOrEmpty(Password) || _IsAdminMode); 
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && buttonOK.Enabled)
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


        public static bool ShowDialog(ref string Username, ref string Password)
        {
            using (LoginDialog lDialog = new LoginDialog())
            {
                if (Username != null)
                    lDialog.Username = Username;
                if (Password != null)
                    lDialog.Password = Password;
                if (lDialog.ShowDialog() == DialogResult.OK)
                {
                    Username = lDialog.Username;
                    Password = lDialog.Password;
                    return true;
                }
                else return false;
            }
        }
    }
}