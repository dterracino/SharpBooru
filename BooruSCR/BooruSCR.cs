using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TA.Engine2D;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class BooruSCR : GameWindow
    {
        private Random R;
        private BooruClient _Booru;
        private List<ulong> _IDs;
        private ImageManager _ImgManager;
        private List<FallingImage> _Textures;
        private double _BackgroundHue;
        private Options _Options;
        //private BitmapFontRenderer _Font;
        private string _ProductNameAndVersion;

        private Image _Cursor;
        private Image _Background;
        private ulong _BackgroundID;

        public BooruSCR(Options Options)
            : base()
        {
            R = new Random();
            _Options = Options;
        }

        protected override void OnLoad(EventArgs e) { PreLoop(); }
        protected override void OnUnload(EventArgs e) { PostLoop(); }
        protected override void OnUpdateFrame(FrameEventArgs e) { Update(e.Time); }
        protected override void OnRenderFrame(FrameEventArgs e) { Draw(e.Time); }

        private void PreLoop()
        {
            _Textures = new List<FallingImage>();
            //_Font = new BitmapFontRenderer(GraphicsDevice);
            _BackgroundHue = R.NextDouble() * 360;

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
                _IDs.RemoveAt(R.Next(0, _IDs.Count));

            _Cursor = new Image(Texture.FromBytes(Properties.Resources.cursor));

            _ImgManager = new ImageManager(R, _Booru, _IDs, _Options.ImageSize, _Options.UseImages);
            _ImgManager.NewTextureLoaded += () =>
                {
                    lock (_Textures)
                        if (_Textures.Count < _Options.ImageCount || _Options.ImageCount < 1)
                            AddNewFBT();
                };
            _ImgManager.LoadingFinished += _Booru.Disconnect;

            _ImgManager.Start();
            AddNewFBT();
        }

        private void PostLoop()
        {
            _ImgManager.Dispose();
            _Booru.Dispose();
            //_Font.Dispose();
            _Background.Dispose();
            _Cursor.Dispose();
        }

        private void Update(double Elapsed)
        {
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(Key.Escape))
                Exit();

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

            MouseState mState = OpenTK.Input.Mouse.GetState();
            _Cursor.X = mState.X - _Cursor.Width / 2;
            _Cursor.Y = mState.Y - _Cursor.Height / 2;

            if (mState.MiddleButton == ButtonState.Pressed)
            {
                _BackgroundID = _IDs[R.Next(0, _IDs.Count)];
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

        private void Draw(double Elapsed)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_Background != null)
                _Background.Draw();

            lock (_Textures)
                foreach (FallingImage texture in _Textures)
                    texture.Draw();

            /*
            if (_Options.Debug)
            {
                _Font.Draw(spriteBatch, new Vector2(0, 0), _ProductNameAndVersion);
                _Font.Draw(spriteBatch, new Vector2(0, 16), "ElapsedMS: {0} FPS: {1}", gameTime.ElapsedGameTime.TotalMilliseconds, 1 / gameTime.ElapsedGameTime.TotalSeconds);
                _Font.Draw(spriteBatch, new Vector2(0, 32), "Loaded Imgs: {0}", _ImgManager.TextureCount);
                _Font.Draw(spriteBatch, new Vector2(0, 48), "Background ID: {0}", _Background.IsBackgroundAvailable ? _BackgroundID.ToString() : "HSV");
            }
            */

            _Cursor.Draw();

            SwapBuffers();
        }

        private void AddNewFBT() { _Textures.Add(new FallingImage(R, _ImgManager.GetRandomTexture(), 0.09f, 0.025f, 0.03f, Width, Height)); }
    }
}