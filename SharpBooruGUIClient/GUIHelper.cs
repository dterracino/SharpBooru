using System;
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

        [DllImport("user32.dll", EntryPoint = "EnableWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _EnableWindow(IntPtr hWnd, bool bEnable);

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

        public static ToolTip CreateToolTip(Control Control, string Text)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(Control, Text);
            return tt;
        }

        public static void SetFormCentered(Form Parent, Form Child)
        {
            Point childLocation = new Point(
                Parent.Location.X + (Parent.Width - Child.Width) / 2,
                Parent.Location.Y + (Parent.Height - Child.Height) / 2);
            Child.Load += (sender, e) => { Child.Location = childLocation; };
        }

        public static bool SetWallpaper(BooruImage Bitmap, bool DeleteTempFileOnSuccess = false)
        {
            if (Helper.IsWindows())
            {
                string tempFile = Helper.GetTempFile();
                Bitmap.Save(ref tempFile, true, ImageFormat.Bmp);
                return _SystemParametersInfo(0x14, 0, tempFile, 0x03);
            }
            else return false;
        }

        public static bool EnableWindow(Form Form, bool Enable)
        {
            if (Helper.IsWindows())
            {
                IntPtr handle = IntPtr.Zero;
                MethodInvoker getHandleInvoker =() => { handle = Form.Handle; };
                if (Form.InvokeRequired)
                    Form.Invoke(getHandleInvoker); 
                else getHandleInvoker();
                return _EnableWindow(handle, Enable);
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