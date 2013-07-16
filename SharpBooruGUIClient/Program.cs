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
            //Application.Run(new MainForm(new Booru("localhost", 2400, "guest", "guest")));
            Application.Run(new testForm());
        }
    }
}
