using System;
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

            graphics.PreferredBackBufferWidth = W;
            graphics.PreferredBackBufferHeight = H;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            graphics.ToggleFullScreen();

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

            _ImgManager = new ImageManager(R, GraphicsDevice, _Booru, _IDs, 400);
            _ImgManager.NewTextureLoaded += () =>
                {
                    lock (_Textures)
                        if (_Textures.Count < 17)
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

            _BackgroundHue += 0.08;
            if (_BackgroundHue > 360)
                _BackgroundHue -= 360;
            _BackgroundColor = ScreensaverHelper.ColorFromHSV(_BackgroundHue, 0.9, 0.15);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_BackgroundColor);
            spriteBatch.Begin();

            lock (_Textures)
                foreach (FallingBooruTexture texture in _Textures)
                    texture.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void AddNewFBT() { _Textures.Add(new FallingBooruTexture(R, _ImgManager.GetRandomTexture(), 80, 2, new Vector2(W, H))); }
    }
}