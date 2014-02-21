using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class BooruSCR : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private int W, H;
        private Random R;

        private BooruClient _Booru;
        private List<ulong> _IDs;
        private ImageManager _ImgManager;
        private List<FallingBooruTexture> _Textures;
        private double _BackgroundHue;
        private Color _BackgroundColor;
        private Options _Options;
        private BitmapFontRenderer _Font;
        private string _ProductNameAndVersion;
        private Vector2 _CursorPosition;
        private Texture2D _CursorTexture;

        public BooruSCR(Options Options)
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            W = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            H = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            R = new Random();
            _Options = Options;
        }

        protected override void Initialize()
        {
            _Booru = new BooruClient();
            _Textures = new List<FallingBooruTexture>();
            _Font = new BitmapFontRenderer(GraphicsDevice);
            _BackgroundHue = R.NextDouble() * 360;

            if (_Options.Debug)
            {
                string productName = ScreensaverHelper.GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                _ProductNameAndVersion = string.Format("{0} V{1}", productName, version);
            }

            graphics.PreferredBackBufferWidth = W;
            graphics.PreferredBackBufferHeight = H;
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = !_Options.NoVSync;
            graphics.ApplyChanges();
            graphics.ToggleFullScreen();

            if (_Options.FPSLimit > 0)
                TargetElapsedTime = new TimeSpan(10000000L / _Options.FPSLimit);
            else IsFixedTimeStep = false;

            IsMouseVisible = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _Booru.Connect(_Options.Server);

            _IDs = _Booru.Search(_Options.Search ?? string.Empty);
            int deleteCount = _IDs.Count - _Options.ImageLimit;
            for (int i = 0; i < deleteCount; i++)
                _IDs.RemoveAt(R.Next(0, _IDs.Count));

            using (MemoryStream ms = new MemoryStream(Properties.Resources.cursor))
                _CursorTexture = Texture2D.FromStream(GraphicsDevice, ms);

            _ImgManager = new ImageManager(R, GraphicsDevice, _Booru, _IDs, _Options.ImageSize, _Options.UseImages);
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

        protected override void UnloadContent()
        {
            _ImgManager.Dispose();
            _Booru.Dispose();
            _Font.Dispose();
            _CursorTexture.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            lock (_Textures)
                for (int i = _Textures.Count - 1; !(i < 0); i--)
                {
                    FallingBooruTexture texture = _Textures[i];
                    texture.Update(gameTime);
                    if (texture.Finished)
                    {
                        _Textures.RemoveAt(i);
                        AddNewFBT();
                    }
                }

            _BackgroundHue += 1 / 20;
            if (_BackgroundHue > 360)
                _BackgroundHue -= 360;
            _BackgroundColor = ScreensaverHelper.ColorFromHSV(_BackgroundHue, 0.9, 0.15);

            MouseState mState = Mouse.GetState();
            _CursorPosition = new Vector2(mState.X - (float)_CursorTexture.Width / 2, mState.Y - (float)_CursorTexture.Height / 2);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_BackgroundColor);
            spriteBatch.Begin();

            lock (_Textures)
                foreach (FallingBooruTexture texture in _Textures)
                    texture.Draw(spriteBatch);

            if (_Options.Debug)
            {
                _Font.Draw(spriteBatch, new Vector2(0, 0), _ProductNameAndVersion);
                _Font.Draw(spriteBatch, new Vector2(0, 16), "ElapsedMS: {0} FPS: {1}", gameTime.ElapsedGameTime.TotalMilliseconds, 1 / gameTime.ElapsedGameTime.TotalSeconds);
                _Font.Draw(spriteBatch, new Vector2(0, 32), "Loaded Imgs: {0}", _ImgManager.TextureCount);
            }

            spriteBatch.Draw(_CursorTexture, _CursorPosition, Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void AddNewFBT() { _Textures.Add(new FallingBooruTexture(R, _ImgManager.GetRandomTexture(), 90, 2.5, 30, new Vector2(W, H))); }
    }
}