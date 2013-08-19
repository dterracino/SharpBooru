using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public partial class ConsoleForm : Form
    {
        public class TextBoxWriter : TextWriter
        {
            TextBox _output = null;

            public TextBoxWriter(TextBox output) { _output = output; }

            public override void Write(char value)
            {
                base.Write(value);
                _output.AppendText(value.ToString());
            }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public class TextBoxReader : TextReader
        {
            //TODO Override it
        }

        private BooruConsole _Console;

        public ConsoleForm(Booru Booru)
        {
            _Console = new BooruConsole(Booru, new TextBoxWriter(textBoxLog), new TextBoxReader()); 
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