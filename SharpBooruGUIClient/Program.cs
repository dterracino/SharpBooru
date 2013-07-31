using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (Booru booru = new Booru("localhost", 2400, "guest", "guest")) //TODO Client config
                Application.Run(new MainForm(booru));
        }
    }
}
