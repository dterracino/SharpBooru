using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using TA.Engine2D;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class ImageManager : IDisposable
    {
        public event Action NewTextureLoaded;
        public event Action LoadingFinished;

        private Random _R;
        private List<Texture> _Textures;
        private List<ulong> _IDs;
        private BooruClient _Booru;
        private double _MaxSideLength;
        private Thread _Thread;
        private bool _IsRunning;
        private bool _UseImages;

        public int TextureCount { get { return _Textures.Count; } }

        public ImageManager(Random R, BooruClient Booru, List<ulong> IDs, double MaxSideLength, bool UseImages)
        { 
            _R = R;
            _IDs = IDs;
            _Booru = Booru;
            _MaxSideLength = MaxSideLength;
            _Thread = new Thread(_ThreadMethod);
            _Textures = new List<Texture>();
            _UseImages = UseImages;
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
            {
                using (BooruImage image = _UseImages ? _Booru.GetImage(_IDs[i]) : _Booru.GetThumbnail(_IDs[i]))
                    try
                    {
                        Texture texture = Texture.FromBytes(ScaleDown(image));
                        lock (_Textures)
                            _Textures.Add(texture);
                        if (NewTextureLoaded != null)
                            NewTextureLoaded();
                    }
                    catch (Exception ex)
                    {
                        if (_IsRunning)
                            ScreensaverHelper.HandleException(ex);
                    }
            }
            if (LoadingFinished != null && _IDs.Count == _Textures.Count)
                LoadingFinished();
        }

        private byte[] ScaleDown(BooruImage Image)
        {
            double sideLength = 0;
            lock (_R)
                sideLength = _MaxSideLength * (_R.NextDouble() + 0.5) + 0.5;
            using (BooruImage smallImage = Image.CreateThumbnail((int)sideLength, false))
                return smallImage.Bytes;
        }

        public void Dispose()
        {
            Stop();
            foreach (Texture texture in _Textures)
                texture.Dispose();
        }

        public Texture GetRandomTexture()
        {
            while (true)
            {
                Texture texture = TryGetRandomTexture();
                if (texture != null)
                    return texture;
                else Thread.Sleep(50);
            }
        }

        public Texture TryGetRandomTexture()
        {
            lock (_Textures)
                lock (_R)
                    if (_Textures.Count > 0)
                        return _Textures[_R.Next(_Textures.Count)];
                    else return null;
        }
    }
}