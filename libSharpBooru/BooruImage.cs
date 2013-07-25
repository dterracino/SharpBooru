using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace TA.SharpBooru
{
    public class BooruImage : IDisposable, ICloneable
    {
        private Bitmap _Bitmap;
        private byte[] _Bytes;

        public byte[] Bytes { get { return _Bytes; } }

        public Bitmap Bitmap
        {
            get
            {
                if (_Bitmap == null)
                    _Bitmap = new Bitmap(new MemoryStream(_Bytes));
                return _Bitmap;
            }
        }

        private void FromStream(Stream Stream)
        {
            if (Stream is MemoryStream)
                _Bytes = (Stream as MemoryStream).ToArray();
            else using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[16 * 1024];
                    while (true)
                    {
                        int read = Stream.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                            ms.Write(buffer, 0, read);
                        else break;
                    }
                    _Bytes = ms.ToArray();
                }
        }

        private BooruImage() { }

        public BooruImage(Stream Stream) { FromStream(Stream); }

        public BooruImage(Bitmap Bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap.Save(ms, GetImageFormat(Bitmap));
                ms.Position = 0;
                FromStream(ms);
            }
        }

        public BooruImage(string File)
        {
            using (FileStream fileStream = System.IO.File.Open(File, FileMode.Open, FileAccess.Read, FileShare.Read))
                FromStream(fileStream);
        }

        public static BooruImage FromURL(string URL)
        {
            if (Helper.CheckURL(URL))
                try { return new BooruImage(Helper.DownloadBytes(URL)); }
                catch { }
            return null;
        }

        public static BooruImage FromFile(string File)
        {
            if (System.IO.File.Exists(File))
                try { return new BooruImage(File); }
                catch { }
            return null;
        }

        public static BooruImage FromFileOrURL(string FileOrURL)
        {
            if (File.Exists(FileOrURL))
                try { return new BooruImage(FileOrURL); }
                catch { }
            else if (Helper.CheckURL(FileOrURL))
                try { return new BooruImage(Helper.DownloadBytes(FileOrURL)); }
                catch { }
            return null;
        }

        public BooruImage(byte[] Bytes) { _Bytes = Bytes; }

        private ImageFormat GetImageFormat(Bitmap B)
        {
            if (B.RawFormat == null)
                return ImageFormat.Png;
            else if (B.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ImageFormat.Png;
            else return B.RawFormat;
        }

        public ImageFormat ImageFormat
        {
            get
            {
                if (_Bitmap == null)
                {
                    if (CompareMagicNumber(Bytes, magicNumberPNG))
                        return ImageFormat.Png;
                    else if (CompareMagicNumber(Bytes, magicNumberGIF1) || CompareMagicNumber(Bytes, magicNumberGIF2))
                        return ImageFormat.Gif;
                    else if (CompareMagicNumber(Bytes, magicNumerJPEG))
                        return ImageFormat.Jpeg;
                }
                return GetImageFormat(Bitmap);
            }
        }

        public string ImageFormatExtension { get { return (new ImageFormatConverter()).ConvertToString(ImageFormat).ToLower(); } }

        public void Dispose()
        {
            if (_Bitmap != null)
                try { _Bitmap.Dispose(); }
                catch { }
        }

        public object Clone() { return new BooruImage() { _Bytes = this._Bytes }; }

        public void Save(string File, ImageFormat Format = null) { Bitmap.Save(File, Format ?? ImageFormat); }
        public void Save(Stream Stream, ImageFormat Format = null) { Bitmap.Save(Stream, Format ?? ImageFormat); }
        public void Save(ref string File, bool AppendExtension = true, ImageFormat Format = null)
        {
            ImageFormat imgFormat = Format ?? ImageFormat;
            if (AppendExtension)
                File += "." + (new ImageFormatConverter()).ConvertToString(imgFormat).ToLower();
            using (FileStream fs = System.IO.File.Open(File, FileMode.Create, FileAccess.Write, FileShare.Read))
                Bitmap.Save(fs, imgFormat);
        }

        public string MimeType
        {
            get
            {
                if (CompareMagicNumber(Bytes, magicNumberPNG))
                    return "image/png";
                else if (CompareMagicNumber(Bytes, magicNumberGIF1) || CompareMagicNumber(Bytes, magicNumberGIF2))
                    return "image/gif";
                else if (CompareMagicNumber(Bytes, magicNumerJPEG))
                    return "image/jpeg";
                else return "image/unknown";
            }
        }

        private bool CompareMagicNumber(byte[] bytes, byte[] magic_number)
        {
            if (bytes.Length < magic_number.Length)
                return false;
            for (int i = 0; i < magic_number.Length; i++)
                if (bytes[i] != magic_number[i])
                    return false;
            return true;
        }

        private static byte[] magicNumberPNG = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
        private static byte[] magicNumberGIF1 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
        private static byte[] magicNumberGIF2 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };
        private static byte[] magicNumerJPEG = new byte[] { 0xff, 0xd8 };

        public override int GetHashCode() { return Bytes.GetHashCode(); }

        public override bool Equals(object obj)
        {
            if (obj is BooruImage)
                return Equals(obj as BooruImage, false);
            else return false;
        }

        public bool Equals(BooruImage bImg, bool CheckPixels)
        {
            if (bImg != null)
            {
                MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
                if (md5Provider.ComputeHash(Bytes).Equals(md5Provider.ComputeHash(bImg.Bytes)))
                    return true;
                else if (CheckPixels)
                {
                    Bitmap B1 = Bitmap;
                    Bitmap B2 = bImg.Bitmap;
                    if (B1.Size.Equals(B2.Size))
                    {
                        BitmapData bd1 = B1.LockBits(new Rectangle(0, 0, B1.Width, B1.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        BitmapData bd2 = B2.LockBits(new Rectangle(0, 0, B2.Width, B2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        bool is_different = false;
                        int count = B1.Width * B1.Height * 4;
                        is_different = Helper.MemoryCompare(bd1.Scan0, bd2.Scan0, count);
                        B1.UnlockBits(bd1);
                        B2.UnlockBits(bd2);
                        return !is_different;
                    }
                }
            }
            return false;
        }

        public BooruImage CreateThumbnail(int SideLength)
        {
            Size size = Bitmap.Size, th_size = new Size(SideLength, SideLength);
            float num = Math.Min((float)th_size.Width / (float)size.Width, (float)th_size.Height / (float)size.Height);
            Rectangle result = new Rectangle();
            result.Width = (int)((float)size.Width * num);
            result.Height = (int)((float)size.Height * num);
            result.X = (th_size.Width - result.Width) / 2;
            result.Y = (th_size.Height - result.Height) / 2;
            Bitmap th = new Bitmap(th_size.Width, th_size.Height);
            using (Graphics g = Graphics.FromImage(th))
                g.DrawImage(Bitmap, result);
            return new BooruImage(th);
        }

        public void ToWriter(BinaryWriter Writer)
        {
            Writer.Write(Bytes.Length);
            Writer.Write(Bytes); 
        }

        public static BooruImage FromReader(BinaryReader Reader)
        {
            int length = Reader.ReadInt32();
            return new BooruImage(Reader.ReadBytes(length));
        }
    }
}