using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace TA.SharpBooru
{
    public class Helper
    {
        private const string TEMP_FOLDER_NAME = "SharpBooru";
        private static readonly DateTime UNIX_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /*
        [DllImportAttribute("kernel32.dll", EntryPoint = "AllocConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetStdHandle")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        private const int STD_OUTPUT_HANDLE = -11;

        [DllImportAttribute("kernel32.dll", EntryPoint = "FreeConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "SetStdHandle")]
        private static extern void _SetStdHandle(UInt32 nStdHandle, IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _AttachConsole(UInt32 PID);

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _FreeConsole();

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);
        */

        [DllImport("user32.dll", EntryPoint = "EnableWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        private static extern int _MemoryCompareWindows(IntPtr b1, IntPtr b2, long count);
        [DllImport("libc.so", EntryPoint = "memcmp")]
        private static extern int _MemoryCompareMono(IntPtr b1, IntPtr b2, long count);

        public static bool MemoryCompare(IntPtr Ptr1, IntPtr Ptr2, long Count)
        {
            if (Ptr1 != null && Ptr2 != null)
                if (Ptr1 != IntPtr.Zero && Ptr2 != IntPtr.Zero)
                    if (Ptr1 == Ptr2)
                        return true;
                    else if (IsMono())
                        return _MemoryCompareMono(Ptr1, Ptr2, Count) == 0;
                    else return _MemoryCompareWindows(Ptr1, Ptr2, Count) == 0;
            return false;
        }

        /*
        public static bool EnableWindow(Form Form, bool Enable)
        {
            if (!IsMono())
            {
                IntPtr handle = IntPtr.Zero;
                MethodInvoker getHandleInvoker = new MethodInvoker(delegate { handle = Form.Handle; });
                if (Form.InvokeRequired) Form.Invoke(getHandleInvoker); else getHandleInvoker();
                return _EnableWindow(handle, Enable);
            }
            else return false;
        }

        public static bool AllocConsole()
        {
            if (!IsMono())
                if (_AllocConsole())
                {
                    _SetStdHandle(0xFFFFFFF5, new IntPtr(7));
                    return true;
                }
            return false;
        }

        public static bool FreeConsole()
        {
            if (!IsMono())
                return _FreeConsole();
            else return false;
        }

        public static bool AttachParentConsole()
        {
            if (!IsMono())
                return _AttachConsole(0x0ffffffff);
            return false;
        }
        
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int _SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static int SetListViewPadding(ListView listview, int leftPadding, int topPadding)
        {
            if (!IsMono())
            {
                const int LVM_FIRST = 0x1000;
                const int LVM_SETICONSPACING = LVM_FIRST + 53;
                int arg = (int)(((ushort)leftPadding) | (uint)((short)topPadding << 16));
                return _SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)arg);
            }
            else return -1;
        }
        */

        public static string DownloadTemporary(string URI)
        {
            try
            {
                string temporaryFile = GetTempFile();
                URI = Uri.UnescapeDataString(URI);
                using (WebClient client = new WebClient())
                    client.DownloadFile(URI, temporaryFile);
                return temporaryFile;
            }
            catch { return null; }
        }

        public static byte[] DownloadBytes(string URI)
        {
            try
            {
                URI = Uri.UnescapeDataString(URI);
                using (WebClient client = new WebClient())
                    return client.DownloadData(URI);
            }
            catch { return null; }
        }

        private static bool? _IsMono = null;
        public static bool IsMono()
        {
            if (!_IsMono.HasValue)
                _IsMono = Type.GetType("Mono.Runtime") != null;
            return _IsMono.Value;
        }

        public static bool IsPOSIX()
        {
            PlatformID pfid = Environment.OSVersion.Platform;
            return pfid == PlatformID.Unix || pfid == PlatformID.MacOSX;
        }

        public static bool IsConsole()
        {
            Stream stdin = Console.OpenStandardInput(1);
            if (stdin == null)
                return false;
            else if (stdin == Stream.Null)
                return false;
            else return stdin.CanWrite;
        }

        public static Color RandomColor() { return RandomColor(new Random()); }
        public static Color RandomColor(Random R)
        {
            byte[] bytes = new byte[3];
            R.NextBytes(bytes);
            return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
        }

        public static Color OppositeColor(Color Color, bool AlsoAlpha = false) { return Color.FromArgb(AlsoAlpha ? 255 - Color.A : 255, 255 - Color.R, 255 - Color.G, 255 - Color.B); }

        /*
        public static bool SetWallpaper(string Path, bool DeleteFileOnSuccess = false)
        {
            if (!IsMono())
            {
                bool success = _SystemParametersInfo(0x14, 0, Path, 0x03);
                if (DeleteFileOnSuccess)
                    File.Delete(Path);
                return success;
            }
            else return false;
        }

        public static bool SetWallpaper(BooruImage Bitmap, bool DeleteTempFileOnSuccess = false)
        {
            if (!IsMono())
            {
                string tempFile = BitmapFactory.SaveBitmap(Bitmap, GetTempFile(), true, System.Drawing.Imaging.ImageFormat.Bmp);
                return SetWallpaper(tempFile, DeleteTempFileOnSuccess);
            }
            else return false;
        }

        public static ToolTip CreateToolTip(Control Control, string Text)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(Control, Text);
            return tt;
        }
        */

        public static bool CheckURL(string URL, bool DeepCheck = false)
        {
            if (!Uri.IsWellFormedUriString(URL, UriKind.RelativeOrAbsolute))
                return false;
            else if (DeepCheck)
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
                    request.Method = "HEAD";
                    HttpWebResponse response = null;
                    request.BeginGetResponse((AsyncCallback)delegate(IAsyncResult result) { response = request.EndGetResponse(result) as HttpWebResponse; }, request);
                    System.Threading.Thread.Sleep(3000);
                    if (response == null)
                    {
                        request.Abort();
                        request.Method = "GET";
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    return response.StatusCode == HttpStatusCode.OK;
                }
                catch { return false; }
            else return true;
        }

        /*
        public static EventHandler SetFormCentered(Form Parent, Form Child)
        {
            Point childLocation = new Point(
                Parent.Location.X + (Parent.Width - Child.Width) / 2,
                Parent.Location.Y + (Parent.Height - Child.Height) / 2);
            EventHandler handler = (EventHandler)delegate { Child.Location = childLocation; };
            Child.Load += handler;
            return handler;
        }
        */

        public static string GetTempFile()
        {
            string tempFileName = RandomString(16) + ".tmp";
            string tempFolder = Path.Combine(Path.GetTempPath(), TEMP_FOLDER_NAME);
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            return Path.Combine(tempFolder, tempFileName);
        }

        public static string RandomString(int Len) { return RandomString(new Random(), Len); }
        public static string RandomString(Random R, int Len)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Len; i++)
                builder.Append((char)('a' + R.Next(0, 26)));
            return builder.ToString();
        }

        public static int CleanTempFolder()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), TEMP_FOLDER_NAME);
            if (Directory.Exists(tempPath))
            {
                string[] fileNames = Directory.GetFiles(tempPath);
                int deletedCount = 0;
                foreach (string file in fileNames)
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch { }
                return deletedCount;
            }
            else return 0;
        }

        public static string MD5(string UnicodeString)
        {
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] stringBytes = Encoding.Unicode.GetBytes(UnicodeString);
            byte[] md5Bytes = md5Provider.ComputeHash(stringBytes);
            StringBuilder sb = new StringBuilder();
            Array.ForEach<byte>(md5Bytes, x => sb.Append(x.ToString("X2")));
            return sb.ToString();
        }

        public static DateTime UnixTimeToDateTime(uint UnixTime) { return UNIX_TIME.AddSeconds(UnixTime).ToLocalTime(); }

        public static uint DateTimeToUnixTime(DateTime DateTime) { return (uint)(DateTime.ToUniversalTime() - UNIX_TIME).TotalSeconds; }
    }
}