using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TA.SharpBooru.Client.GUI
{
    public partial class ConsoleForm : Form
    {
        public class TextBoxWriter : TextWriter
        {
            private TextBox _output = null;

            public TextBoxWriter(TextBox output) { _output = output; }

            public override void Write(char value) { GUIHelper.Invoke(_output, () => _output.AppendText(value.ToString())); }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public class LineBufferReader : TextReader
        {
            private object _lock = new object();
            private List<string> _lineBuf = new List<string>();
            private ManualResetEvent _event = new ManualResetEvent(false);

            public void AddLine(string Line)
            {
                lock (_lock)
                {
                    _lineBuf.Add(Line);
                    _event.Set();
                }
            }

            public override string ReadLine()
            {
                lock (_lock)
                {
                    if (_lineBuf.Count < 1)
                        _event.WaitOne();
                    string line = _lineBuf[0];
                    _lineBuf.RemoveAt(0);
                    return line;
                }
            }
        }

        private BooruConsole _Console;
        private LineBufferReader _LineBufferReader;
        private Thread _ConsoleThread;

        public ConsoleForm(Booru Booru)
        {
            InitializeComponent();
            _LineBufferReader = new LineBufferReader();
            _Console = new BooruConsole(Booru, new TextBoxWriter(textBoxLog), _LineBufferReader);
            this.textBoxCmd.KeyDown += textBoxCmd_KeyDown;
            _ConsoleThread = _Console.StartAsync();
            this.FormClosing += (sender, e) => _ConsoleThread.Join();
        }

        private void textBoxCmd_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Tab)
            //TODO Implement TAB completion
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrWhiteSpace(this.textBoxCmd.Text))
                {
                    _LineBufferReader.AddLine(this.textBoxCmd.Text);
                    this.textBoxCmd.Clear();
                }
            }
            else if (e.KeyCode == Keys.Escape)
                this.textBoxCmd.Clear();
        }
    }
}