﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Reflection;

namespace TA.SharpBooru
{
    public class Helper
    {
        private const string TEMP_FOLDER_NAME = "SharpBooru";
        private static readonly DateTime UNIX_TIME = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly Random Random = new Random();

        /*
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetStdHandle")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        private const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll", EntryPoint = "SetStdHandle")]
        private static extern void _SetStdHandle(UInt32 nStdHandle, IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _AttachConsole(UInt32 PID);

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool _FreeConsole();
        */

        [DllImport("user32.dll", EntryPoint = "EnableWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("msvcrt.dll", EntryPoint = "memcmp")]
        private static extern int _MemoryCompareWindows(IntPtr b1, IntPtr b2, long count);
        [DllImport("libc.so", EntryPoint = "memcmp")]
        private static extern int _MemoryCompareUnix(IntPtr b1, IntPtr b2, long count);

        public static bool MemoryCompare(IntPtr Ptr1, IntPtr Ptr2, long Count)
        {
            if (Ptr1 != IntPtr.Zero && Ptr2 != IntPtr.Zero)
                if (Ptr1 == Ptr2)
                    return true;
                else if (IsWindows())
                    return _MemoryCompareWindows(Ptr1, Ptr2, Count) == 0;
                else if (IsUnix())
                    return _MemoryCompareUnix(Ptr1, Ptr2, Count) == 0;
                else unsafe
                    {
                        byte* rPtr1 = (byte*)Ptr1.ToPointer();
                        byte* rPtr2 = (byte*)Ptr2.ToPointer();
                        for (long i = 0; i < Count; i++)
                            if (rPtr1[i] != rPtr2[i])
                                return false;
                        return true;
                    }
            return false;
        }

        public static bool MemoryCompare(byte[] Bytes1, byte[] Bytes2)
        {
            if (Bytes1 == Bytes2)
                return true;
            else if (Bytes1 == null || Bytes2 == null)
                return false;
            else if (Bytes1.LongLength != Bytes2.LongLength)
                return false;
            else if (Bytes1.LongLength < 1)
                return true;
            else unsafe
                {
                    fixed (byte* rPtr1 = Bytes1)
                    fixed (byte* rPtr2 = Bytes2)
                        return MemoryCompare((IntPtr)rPtr1, (IntPtr)rPtr2, Bytes1.LongLength);
                }
        }

        /*
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

        /*
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
        */

        private static bool? _IsMono = null;
        public static bool IsMono()
        {
            if (!_IsMono.HasValue)
                _IsMono = Type.GetType("Mono.Runtime") != null;
            return _IsMono.Value;
        }

        public static bool IsUnix() { return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX; }

        public static bool IsWindows() { return Environment.OSVersion.Platform == PlatformID.Win32NT; }

        public static bool IsConsole()
        {
            Stream stdin = Console.OpenStandardInput(1);
            if (stdin == null)
                return false;
            else if (stdin == Stream.Null)
                return false;
            else return stdin.CanWrite;
        }

        public static Color RandomColor() { return RandomColor(Random); }
        public static Color RandomColor(Random R)
        {
            byte[] bytes = new byte[3];
            R.NextBytes(bytes);
            return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
        }

        public static Color OppositeColor(Color Color, bool AlsoAlpha = false) { return Color.FromArgb(AlsoAlpha ? 255 - Color.A : 255, 255 - Color.R, 255 - Color.G, 255 - Color.B); }

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

        public static string GetTempFile()
        {
            string tempFileName = RandomString(16) + ".tmp";
            string tempFolder = Path.Combine(Path.GetTempPath(), TEMP_FOLDER_NAME);
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            return Path.Combine(tempFolder, tempFileName);
        }

        public static string RandomString(int Len) { return RandomString(Random, Len); }
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

        public static byte[] MD5OfString(string UnicodeString)
        {
            byte[] data = Encoding.Unicode.GetBytes(UnicodeString);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                return md5.ComputeHash(data);
        }

        public static byte[] MD5OfData(byte[] Data)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                return md5.ComputeHash(Data);
        }

        public static byte[] MD5OfFile(string Path)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            using (FileStream file = File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return md5.ComputeHash(file);
        }

        public static string BytesToString(byte[] Data)
        {
            StringBuilder sb = new StringBuilder();
            Array.ForEach<byte>(Data, x => sb.Append(x.ToString("X2")));
            return sb.ToString().ToLower();
        }

        public static DateTime UnixTimeToDateTime(uint UnixTime) { return UNIX_TIME.AddSeconds(UnixTime).ToLocalTime(); }

        public static uint DateTimeToUnixTime(DateTime DateTime) { return (uint)(DateTime.ToUniversalTime() - UNIX_TIME).TotalSeconds; }

        //Based on Mitch's answer http://stackoverflow.com/questions/2727609
        public static IPEndPoint GetIPEndPointFromString(string EndPointString, int DefaultPort = 2400)
        {
            if (string.IsNullOrWhiteSpace(EndPointString))
                throw new ArgumentException("Endpoint descriptor may not be empty");
            if (DefaultPort < IPEndPoint.MinPort || DefaultPort > IPEndPoint.MaxPort)
                throw new ArgumentException("Invalid default port: " + DefaultPort);
            string[] values = EndPointString.Split(new char[1] { ':' });
            IPAddress ipaddy;
            int port = -1;
            if (values.Length <= 2) // IPv4 or hostname
            {
                if (values.Length == 1)
                    port = DefaultPort;
                else port = GetPortFromString(values[1]);
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = GetIPAddressFromHostname(values[0]);
            }
            else if (values.Length > 2)
                throw new ProtocolViolationException("IPv6 isn't supported");
            else throw new FormatException("Invalid endpoint ipaddress: " + EndPointString);
            if (port < IPEndPoint.MinPort)
                throw new ArgumentException("No port specified: ", EndPointString);
            return new IPEndPoint(ipaddy, port);
        }

        public static int GetPortFromString(string Port)
        {
            int port;
            if (!int.TryParse(Port, out port) || port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new FormatException("Invalid port: " + Port);
            return port;
        }

        public static IPAddress GetIPAddressFromHostname(string Hostname)
        {
            IPAddress[] hosts = Dns.GetHostAddresses(Hostname);
            if (hosts == null)
                throw new NullReferenceException("Hosts list is null");
            else
            {
                var validHosts = new List<IPAddress>();
                foreach (var host in hosts)
                    if (host.AddressFamily == AddressFamily.InterNetwork)
                        validHosts.Add(host);
                if (validHosts.Count < 1)
                    throw new ArgumentException("Host not found: " + Hostname);
                else if (validHosts.Count < 2)
                    return validHosts[0];
                else return validHosts[Random.Next(0, validHosts.Count)];
            }
        }

        public static ushort GetVersionMajor() { return (ushort)Assembly.GetExecutingAssembly().GetName().Version.Major; }

        private static ushort? _VersionMinor = null;
        public static ushort GetVersionMinor()
        {
            if (!_VersionMinor.HasValue)
                _VersionMinor = (ushort)Assembly.GetExecutingAssembly().GetName().Version.Minor;
            return _VersionMinor.Value;
        }
    }
}