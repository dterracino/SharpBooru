﻿using System;
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
        private Cursor _Cursor;
        private BackgroundBooruTexture _Background;
        private ulong _BackgroundID;

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

            _Cursor = new Cursor(ScreensaverHelper.Texture2DFromBytes(GraphicsDevice, Properties.Resources.cursor), 1f);
            _Background = new BackgroundBooruTexture(W, H);

            _ImgManager = new ImageManager(R, GraphicsDevice, _Booru, _IDs, _Options.ImageSize, _Options.UseImages);
            _ImgManager.NewTextureLoaded += () =>
                {
                    lock (_Textures)
                        if (_Textures.Count < _Options.ImageCount || _Options.ImageCount < 1)
                            AddNewFBT();
                };
            //_ImgManager.LoadingFinished += _Booru.Disconnect;

            _ImgManager.Start();
            AddNewFBT();
        }

        protected override void UnloadContent()
        {
            _ImgManager.Dispose();
            _Booru.Dispose();
            _Font.Dispose();
            _Background.Dispose();
            _Cursor.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
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

            if (!_Background.IsBackgroundAvailable)
            {
                _BackgroundHue += 1 / 20;
                if (_BackgroundHue > 360)
                    _BackgroundHue -= 360;
                _BackgroundColor = ScreensaverHelper.ColorFromHSV(_BackgroundHue, 0.9, 0.15);
            }

            MouseState mState = Mouse.GetState();
            _Cursor.Update(gameTime, mState);

            if (mState.MiddleButton == ButtonState.Pressed)
            {
                _BackgroundID = _IDs[R.Next(0, _IDs.Count)];
                _Background.SetBooruImage(GraphicsDevice, _Booru.GetImage(_BackgroundID));
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!_Background.IsBackgroundAvailable)
                GraphicsDevice.Clear(_BackgroundColor);

            spriteBatch.Begin();

            _Background.Draw(spriteBatch, Color.Gray);

            lock (_Textures)
                foreach (FallingBooruTexture texture in _Textures)
                    texture.Draw(spriteBatch);

            if (_Options.Debug)
            {
                _Font.Draw(spriteBatch, new Vector2(0, 0), _ProductNameAndVersion);
                _Font.Draw(spriteBatch, new Vector2(0, 16), "ElapsedMS: {0} FPS: {1}", gameTime.ElapsedGameTime.TotalMilliseconds, 1 / gameTime.ElapsedGameTime.TotalSeconds);
                _Font.Draw(spriteBatch, new Vector2(0, 32), "Loaded Imgs: {0}", _ImgManager.TextureCount);
                _Font.Draw(spriteBatch, new Vector2(0, 48), "Background ID: {0}", _Background.IsBackgroundAvailable ? _BackgroundID.ToString() : "HSV");
            }

            _Cursor.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void AddNewFBT() { _Textures.Add(new FallingBooruTexture(R, _ImgManager.GetRandomTexture(), 90, 2.5, 30, new Vector2(W, H))); }
    }
}