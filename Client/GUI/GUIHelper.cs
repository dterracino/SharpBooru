using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TA.SharpBooru.Client.GUI
{
    public static class GUIHelper
    {
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int _SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static int SetListViewPadding(ListView ListView, int leftPadding, int topPadding)
        {
            if (Helper.IsWindows())
            {
                const int LVM_FIRST = 0x1000;
                const int LVM_SETICONSPACING = LVM_FIRST + 53;
                int arg = (int)(((ushort)leftPadding) | (uint)((short)topPadding << 16));
                return _SendMessage(ListView.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)arg);
            }
            else return -1;
        }

        public static void CreateToolTip(Control Control, string Text)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(Control, Text);
            Control.Disposed += (sender, e) =>
                tt.Dispose();
        }

        public static void SetFormCentered(Form Parent, Form Child)
        {
            Point childLocation = new Point(
                Parent.Location.X + (Parent.Width - Child.Width) / 2,
                Parent.Location.Y + (Parent.Height - Child.Height) / 2);
            Child.Load += (sender, e) => { Child.Location = childLocation; };
        }

        private static string GetSpecialFolder(Environment.SpecialFolder SpecialFolder)
        {
            string fPath = Environment.GetFolderPath(SpecialFolder);
            if (!string.IsNullOrWhiteSpace(fPath))
                return fPath;
            else return null;
        }

        public static bool SetWallpaper(BooruImage Bitmap, bool DeleteTempFileOnSuccess = false)
        {
            if (Helper.IsWindows())
            {
                string bgPath = GetSpecialFolder(Environment.SpecialFolder.MyPictures)
                                ?? GetSpecialFolder(Environment.SpecialFolder.Personal)
                                ?? GetSpecialFolder(Environment.SpecialFolder.Windows)
                                ?? Path.GetTempPath();
                string bgFile = Path.Combine(bgPath, "SharpBooruWallpaper.bmp");
                Bitmap.Save(ref bgFile, true, ImageFormat.Bmp);
                return _SystemParametersInfo(0x14, 0, bgFile, 0x03);
            }
            else return false;
        }

        public static void Invoke(Control Control, Action Action)
        {
            if (Control.InvokeRequired)
                Control.Invoke(Action);
            else Action();
        }
    }
}