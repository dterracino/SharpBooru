using System;
using System.Reflection;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TA.Engine2D;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class BooruSCR : Game
    {
        private BooruClient _Booru;
        private List<ulong> _IDs;
        private ImageManager _ImgManager;
        private List<FallingImage> _Textures;
        private double _BackgroundHue;
        private Options _Options;
        private Font _Font;
        private string _ProductNameAndVersion;

        private Image _Cursor;
        private Image _Background;
        private ulong _BackgroundID;

        public BooruSCR(Options Options)
            : base()
        { _Options = Options; }

        protected override void PreLoop()
        {
            VSync = _Options.NoVSync ? VSyncMode.Off : VSyncMode.On;
            TargetUpdateFrequency = 1000;
            TargetRenderFrequency = 1000;

            _Textures = new List<FallingImage>();
            _Font = new Font();
            _BackgroundHue = Helper.Random.NextDouble() * 360;

            if (_Options.Debug)
            {
                string productName = ScreensaverHelper.GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                _ProductNameAndVersion = string.Format("{0} V{1}", productName, version);
            }

            //CursorVisible = false;

            _Booru = new BooruClient();
            _Booru.Connect(_Options.Server);

            _IDs = _Booru.Search(_Options.Search ?? string.Empty);
            int deleteCount = _IDs.Count - _Options.ImageLimit;
            for (int i = 0; i < deleteCount; i++)
                _IDs.RemoveAt(Helper.Random.Next(0, _IDs.Count));

            _Cursor = new Image(Texture.FromBytes(Properties.Resources.cursor));

            _ImgManager = new ImageManager(_Booru, _IDs, (int)(_Options.ImageSize * 1.5d + 0.5d), _Options.UseImages);
            _ImgManager.NewTextureLoaded += () =>
                {
                    lock (_Textures)
                        if (_Textures.Count < _Options.ImageCount || _Options.ImageCount < 1)
                            AddNewFBT();
                };
            //_ImgManager.LoadingFinished += _Booru.Disconnect;
            _ImgManager.Start();
        }

        protected override void PostLoop()
        {
            _ImgManager.Dispose();
            _Booru.Dispose();
            _Font.Dispose();
            if (_Background != null)
                _Background.Dispose();
            _Cursor.Dispose();
        }

        protected override void Update(double Elapsed)
        {
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(Key.Escape))
                Exit();

            _ImgManager.CreateTextures();

            lock (_Textures)
                for (int i = _Textures.Count - 1; !(i < 0); i--)
                {
                    FallingImage texture = _Textures[i];
                    texture.Update(Elapsed);
                    if (texture.Finished)
                    {
                        _Textures.RemoveAt(i);
                        AddNewFBT();
                    }
                }

            if (_Background == null)
            {
                _BackgroundHue += 1 / 20;
                if (_BackgroundHue > 360)
                    _BackgroundHue -= 360;
                GL.ClearColor(ScreensaverHelper.ColorFromHSV(_BackgroundHue, 0.9, 0.15));
            }

            _Cursor.X = Mouse.X- _Cursor.Width / 2;
            _Cursor.Y = Mouse.Y - _Cursor.Height / 2;

            if (Mouse[MouseButton.Middle])
            {
                _BackgroundID = _IDs[Helper.Random.Next(0, _IDs.Count)];
                if (_Background != null)
                    _Background.Dispose();
                _Background = new Image(Texture.FromBytes(_Booru.GetImage(_BackgroundID).Bytes));
                float num = Math.Max((float)Width / _Background.Texture.Width, (float)Height / _Background.Texture.Height);
                _Background.Width = _Background.Texture.Width * num;
                _Background.Height = _Background.Texture.Height * num;
                _Background.X = 0 - (_Background.Width - Width) / 2;
                _Background.Y = 0 - (_Background.Height - Height) / 2;
                _Background.Color = System.Drawing.Color.Gray;
            }
        }

        protected override void Draw(double Elapsed)
        {
            if (_Background != null)
                _Background.Draw();

            lock (_Textures)
                foreach (FallingImage texture in _Textures)
                    texture.Draw();

            if (_Options.Debug)
            {
                _Font.Draw(0, 0, _ProductNameAndVersion);
                _Font.Draw(0, 16, "UPS: {0}", UpdateFrequency);
                _Font.Draw(0, 32, "FPS: {0}", RenderFrequency);
                _Font.Draw(0, 48, "Loaded Imgs: {0}", _ImgManager.TextureCount);
                _Font.Draw(0, 64, "Background ID: {0}", _Background != null ? _BackgroundID.ToString() : "HSV");
            }

            _Cursor.Draw();
        }

        private void AddNewFBT() { _Textures.Add(new FallingImage(Helper.Random, _ImgManager.GetRandomTexture(Helper.Random), 0.09f, 0.025f, 0.03f, Width, Height)); }
    }
}