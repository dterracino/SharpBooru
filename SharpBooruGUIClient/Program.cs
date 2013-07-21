using System;
using System.Windows.Forms;

namespace TA.SharpBooru.Client.GUI
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Booru booru = new Booru("localhost", 2400, "guest", "guest");
            Application.Run(new MainForm(booru));
        }
    }
}
