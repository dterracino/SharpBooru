using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class ConnectDialog : Form
    {
        public ConnectDialog()
        {
            InitializeComponent();
            textBoxServer.KeyDown += keyDown;
            textBoxServer.TextChanged += textChanged;
            buttonOK.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                        Close();
                };
            textChanged(this, EventArgs.Empty);
        }

        private void textChanged(object sender, EventArgs e) { buttonOK.Enabled = !string.IsNullOrWhiteSpace(Server); }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && buttonOK.Enabled)
                buttonOK_Click(sender, e);
            else if (e.KeyCode == Keys.Escape)
                Close();
        }

        public string Server
        {
            get { return textBoxServer.Text ?? string.Empty; }
            set { textBoxServer.Text = value ?? string.Empty; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public static bool ShowDialog(ref string Server)
        {
            using (ConnectDialog cDialog = new ConnectDialog())
            {
                if (Server != null)
                    cDialog.Server = Server;
                if (cDialog.ShowDialog() == DialogResult.OK)
                {
                    Server = cDialog.Server;
                    return true;
                }
                else return false;
            }
        }

        private void buttonSearchLAN_Click(object sender, EventArgs e)
        {
            Broadcaster.Response response = Broadcaster.SearchForServer(1000);
            if (response == null)
                MessageBox.Show("No LAN server found", "Server not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else Server = string.Format("{0}:{1}", response.EndPoint.Address, response.EndPoint.Port);
        }
    }
}