using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using System.Collections.Generic;
using TA.Engine2D;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class ImageManager : IDisposable
    {
        private class DecodedImage
        {
            public byte[] Bytes;
            public int Width, Height;
        }

        public event Action NewTextureLoaded;
        public event Action LoadingFinished;

        private List<DecodedImage> _DecodedImages;
        private List<Texture> _Textures;
        private List<ulong> _IDs;
        private BooruClient _Booru;
        private int _MaxSideLength;
        private Thread _Thread;
        private bool _IsRunning;
        private bool _UseImages;

        public int TextureCount { get { return _Textures.Count; } }

        public ImageManager(BooruClient Booru, List<ulong> IDs, int MaxSideLength, bool UseImages)
        {
            _IDs = IDs;
            _Booru = Booru;
            _MaxSideLength = MaxSideLength;
            _Thread = new Thread(_ThreadMethod);
            _Textures = new List<Texture>();
            _UseImages = UseImages;
            _DecodedImages = new List<DecodedImage>();
        }

        public void Start()
        {
            _IsRunning = true;
            _Thread.Start();
        }

        public void Stop()
        {
            _IsRunning = false;
            _Thread.Join();
        }

        private void _ThreadMethod()
        {
            for (int i = 0; i < _IDs.Count && _IsRunning; i++)
                using (BooruImage image = _UseImages ? _Booru.GetImage(_IDs[i]) : _Booru.GetThumbnail(_IDs[i]))
                    lock (_DecodedImages)
                        _DecodedImages.Add(ScaleDownAndDecode(image));
        }

        private DecodedImage ScaleDownAndDecode(BooruImage Image)
        {
            using (BooruImage smallImage = Image.CreateThumbnail(_MaxSideLength, false))
            {
                Rectangle rect = new Rectangle(0, 0, smallImage.Bitmap.Width, smallImage.Bitmap.Height);
                BitmapData bitmapData = smallImage.Bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte[] imageBytes = new byte[rect.Width * rect.Height * 4];
                unsafe
                {
                    byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                    for (int i = 0; i < imageBytes.Length; i++)
                        imageBytes[i] = ptr[i];
                }
                smallImage.Bitmap.UnlockBits(bitmapData);
                return new DecodedImage()
                {
                    Bytes = imageBytes,
                    Width = rect.Width,
                    Height = rect.Height
                };
            }
        }

        public void Dispose()
        {
            Stop();
            foreach (Texture texture in _Textures)
                texture.Dispose();
        }

        public Texture GetRandomTexture(Random R)
        {
            while (true)
            {
                Texture texture = TryGetRandomTexture(R);
                if (texture != null)
                    return texture;
                else Thread.Sleep(50);
            }
        }

        public Texture TryGetRandomTexture(Random R)
        {
            lock (_Textures)
                if (_Textures.Count > 0)
                    return _Textures[R.Next(_Textures.Count)];
                else return null;
        }

        public void CreateTextures()
        {
            if (_IsRunning)
            {
                lock (_DecodedImages)
                    if (_DecodedImages.Count > 0)
                    {
                        DecodedImage img = _DecodedImages[0];
                        _DecodedImages.RemoveAt(0); //TODO Use stack for this
                        Texture texture = Texture.FromBytes(img.Bytes, img.Width, img.Height);
                        lock (_Textures)
                            _Textures.Add(texture);
                        if (NewTextureLoaded != null)
                            NewTextureLoaded();
                    }
                if (_IDs.Count == _Textures.Count)
                {
                    _IsRunning = false;
                    if (LoadingFinished != null)
                        LoadingFinished();
                }
            }
        }
    }
}