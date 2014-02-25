using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace TA.Engine2D
{
    public class Texture : IDisposable
    {
        private int _ID;
        private int _Width;
        private int _Height;
        private bool _Disposed = false;

        public int ID
        {
            get
            {
                CheckDisposed();
                return _ID;
            }
        }

        public int Width
        {
            get
            {
                CheckDisposed();
                return _Width;
            }
        }

        public int Height
        {
            get
            {
                CheckDisposed();
                return _Height;
            }
        }

        private Texture(int ID, int W, int H)
        {
            _ID = ID;
            _Width = W;
            _Height = H;
        }

        public static Texture FromScan0(IntPtr Scan0, int Width, int Height)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureRectangle, textureID);
            Texture texture = new Texture(textureID, Width, Height);
            GL.TexImage2D(TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, Scan0);
            GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            return texture;
        }

        public static Texture FromBitmap(Bitmap Bitmap)
        {
            BitmapData bitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Texture texture = FromScan0(bitmapData.Scan0, Bitmap.Width, Bitmap.Height);
            Bitmap.UnlockBits(bitmapData);
            return texture;
        }

        public static Texture FromStream(Stream Stream)
        {
            using (Bitmap bitmap = new Bitmap(Stream))
                return FromBitmap(bitmap);
        }

        public static Texture FromFile(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromStream(fs);
        }

        public static Texture FromBytes(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
                return FromStream(ms);
        }

        private void CheckDisposed()
        {
            if (_Disposed)
                throw new ObjectDisposedException("Texture");
        }

        public void Dispose()
        {
            if (!_Disposed)
                GL.DeleteTexture(_ID);
        }
    }
}