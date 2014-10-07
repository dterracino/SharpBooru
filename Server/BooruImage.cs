using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;

namespace TA.SharpBooru
{
    public class BooruImage : BooruResource, IDisposable, ICloneable
    {
        private Bitmap _Bitmap;
        private byte[] _Bytes;

        public byte[] Bytes { get { return _Bytes; } }

        public Bitmap Bitmap
        {
            get
            {
                if (_Bitmap != null)
                    try { int isDisposedWidth = _Bitmap.Width; }
                    catch (ObjectDisposedException) { Dispose(); }
                if (_Bitmap == null)
                    _Bitmap = new Bitmap(new MemoryStream(_Bytes));
                return _Bitmap;
            }
        }

        private BooruImage() { }

        private static BooruImage FromStream(Stream Stream) //, Action<float> ProgessCallback = null)
        {
            BooruImage img = new BooruImage();
            //if (ProgessCallback != null)
            //    ProgessCallback(0f);
            if (!(Stream is MemoryStream))
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[16 * 1024];
                    while (true)
                    {
                        int read = Stream.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                            ms.Write(buffer, 0, read);
                        else break;
                    }
                    img._Bytes = ms.ToArray();
                }
            else img._Bytes = (Stream as MemoryStream).ToArray();
            //if (ProgessCallback != null)
            //    ProgessCallback(1f);
            return img;
        }

        public static BooruImage FromBitmap(Bitmap Bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap.Save(ms, GetImageFormat(Bitmap));
                ms.Position = 0;
                return FromStream(ms);
            }
        }

        public static BooruImage FromURL(string URL)
        {
            if (Helper.CheckURL(URL))
                try
                {
                    WebClient client = new WebClient();
                    return BooruImage.FromBytes(client.DownloadData(URL));
                }
                catch { }
            return null;
        }

        public static BooruImage FromFile(string File)
        {
            if (System.IO.File.Exists(File))
                try
                {
                    using (FileStream fileStream = System.IO.File.Open(File, FileMode.Open, FileAccess.Read, FileShare.Read))
                        return FromStream(fileStream);
                }
                catch { }
            return null;
        }

        public static BooruImage FromFileOrURL(string FileOrURL)
        {
            if (File.Exists(FileOrURL))
                return FromFile(FileOrURL);
            else if (Helper.CheckURL(FileOrURL))
                return FromURL(FileOrURL);
            else return null;
        }

        public static BooruImage FromBytes(byte[] Bytes) { return new BooruImage() { _Bytes = Bytes }; }

        private static ImageFormat GetImageFormat(Bitmap B)
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
            {
                try { _Bitmap.Dispose(); }
                catch { }
                _Bitmap = null;
            }
        }

        public object Clone() { return new BooruImage() { _Bytes = _Bytes.Clone() as byte[] }; }

        public void Save(Stream Stream, ImageFormat Format = null)
        {
            if (Format == null)
                Stream.Write(_Bytes, 0, _Bytes.Length);
            else Bitmap.Save(Stream, Format ?? ImageFormat);
        }
        public void Save(string File, ImageFormat Format = null)
        {
            using (FileStream fStream = new FileStream(File, FileMode.Create, FileAccess.Write, FileShare.Read))
                Save(fStream, Format);
        }
        public void Save(ref string File, bool AppendExtension = true, ImageFormat Format = null)
        {
            ImageFormat imgFormat = Format ?? ImageFormat;
            if (AppendExtension)
                File += "." + (new ImageFormatConverter()).ConvertToString(imgFormat).ToLower();
            Save(File, imgFormat);
        }
        public void Save(string File, long Quality)
        {
            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
                if (encoder.FormatID == ImageFormat.Jpeg.Guid)
                {
                    EncoderParameters myEncoderParams = new EncoderParameters(1);
                    myEncoderParams.Param[0] = new EncoderParameter(Encoder.Quality, Quality);
                    using (FileStream fStream = new FileStream(File, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        Bitmap.Save(fStream, encoder, myEncoderParams);
                        return;
                    }
                }
            throw new Exception("JPEG encoder not found");
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
                if (CheckPixels)
                {
                    Bitmap B1 = Bitmap;
                    Bitmap B2 = bImg.Bitmap;
                    if (B1.Size.Equals(B2.Size))
                    {
                        BitmapData bd1 = B1.LockBits(new Rectangle(0, 0, B1.Width, B1.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        BitmapData bd2 = B2.LockBits(new Rectangle(0, 0, B2.Width, B2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        bool is_different = Helper.MemoryCompare(bd1.Scan0, bd2.Scan0, B1.Width * B1.Height * 4);
                        B1.UnlockBits(bd1);
                        B2.UnlockBits(bd2);
                        return !is_different;
                    }
                    else using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
                            if (md5Provider.ComputeHash(Bytes).Equals(md5Provider.ComputeHash(bImg.Bytes)))
                                return true;
                }
            }
            return false;
        }

        public BooruImage CreateThumbnail(int SideLength, bool Square)
        {
            Size size = Bitmap.Size;
            Size th_size = new Size(SideLength, SideLength);
            double num = Math.Min((double)th_size.Width / size.Width, (double)th_size.Height / size.Height);
            Size resultSize = new Size((int)(size.Width * num), (int)(size.Height * num));
            if (Square)
            {
                //TODO Maybe use floats instead of int division?
                Point resultPoint = new Point((th_size.Width - resultSize.Width) / 2, (th_size.Height - resultSize.Height) / 2);
                Bitmap th = new Bitmap(th_size.Width, th_size.Height);
                using (Graphics g = CreateAAGraphics(th))
                    g.DrawImage(Bitmap, resultPoint.X, resultPoint.Y, resultSize.Width, resultSize.Height);
                return BooruImage.FromBitmap(th);
            }
            else
            {
                Bitmap th = new Bitmap(resultSize.Width, resultSize.Height);
                using (Graphics g = CreateAAGraphics(th))
                    g.DrawImage(Bitmap, 0f, 0f, resultSize.Width, resultSize.Height);
                return BooruImage.FromBitmap(th);
            }
        }

        private Graphics CreateAAGraphics(Bitmap Bitmap)
        {
            Graphics g = Graphics.FromImage(Bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            return g;
        }

        public byte[] CalculateImageHash()
        {
            byte sideLen = 12;
            byte[] hash = new byte[sideLen * sideLen];
            using (Bitmap hB = new Bitmap(sideLen, sideLen))
            {
                using (Graphics g = CreateAAGraphics(hB))
                {
                    g.Clear(Color.Gray);
                    g.DrawImage(Bitmap, 0, 0, sideLen, sideLen);
                }
                BitmapData bd = hB.LockBits(new Rectangle(0, 0, sideLen, sideLen), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* ptr = (byte*)bd.Scan0.ToPointer();
                    for (int i = 0; i < hash.Length; i++)
                        hash[i] = (byte)((ptr[i * 3] >> 6) | (ptr[i * 3 + 1] >> 3 & 28) | (ptr[i * 3 + 2] & 224));
                }
                hB.UnlockBits(bd);
            }
            return hash;
        }

        public static int CompareImageHashes(byte[] Hash1, byte[] Hash2)
        {
            if (Hash1.Length != Hash2.Length)
                throw new ArgumentException("Hashes must have the same length");
            int diffcount = 0;
            for (int i = 0; i < Hash1.Length; i++)
            {
                byte[] rgb1 = _GetRGBFromSingleByte(Hash1[i]);
                byte[] rgb2 = _GetRGBFromSingleByte(Hash2[i]);
                diffcount += Math.Abs((short)rgb1[0] - rgb2[0]);
                diffcount += Math.Abs((short)rgb1[1] - rgb2[1]);
                diffcount += Math.Abs((short)rgb1[2] - rgb2[2]);
            }
            return diffcount;
        }

        private static byte[] _GetRGBFromSingleByte(byte Byte)
        {
            return new byte[3]
            {
                (byte)(Byte & 3),
                (byte)(Byte >> 2 & 7),
                (byte)(Byte >> 5)
            };
        }

        public override void ToWriter(ReaderWriter Writer) { Writer.Write(Bytes, true); }

        /*
        public void ToWriter(BinaryWriter Writer, Action<float> ProgressCallback)
        {
            Writer.Write(Bytes.Length);
            if (ProgressCallback != null)
            {
                int chunkSize = 1024 * 5;
                int chunkCount = Bytes.Length / chunkSize;
                for (int i = 0; i < chunkCount; i++)
                {
                    Writer.Write(Bytes, i * chunkSize, chunkSize);
                    ProgressCallback(i * (float)chunkSize / Bytes.Length);
                }
                Writer.Write(Bytes, chunkCount * chunkSize, Bytes.Length % chunkSize);
                ProgressCallback(1f);
            }
            else Writer.Write(Bytes);
        }
        */

        public static BooruImage FromReader(ReaderWriter Reader) { return BooruImage.FromBytes(Reader.ReadBytes()); }
    }
}
