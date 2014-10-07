using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class Cursor : IDisposable
    {
        private Texture2D _Texture;
        private float _Rad;
        private float _Scale;
        private Vector2 _Location;
        private Vector2 _Origin;

        public Cursor(Texture2D Texture, float Scale)
        {
            _Rad = (float)(Helper.Random.NextDouble() * 2 * Math.PI);
            _Texture = Texture;
            _Scale = Scale;
            _Origin = new Vector2((_Texture.Width - 1) / 2f, (_Texture.Height - 1) / 2f);
        }

        public void Dispose() { _Texture.Dispose(); }

        public void Update(GameTime gameTime, MouseState mouse)
        {
            _Location = new Vector2(mouse.X, mouse.Y);
            _Rad += 0.008f;
            if (_Rad > 2 * Math.PI + 1)
                _Rad -= (float)(2 * Math.PI);
        }

        public void Draw(SpriteBatch SB) { SB.Draw(_Texture, _Location, null, Color.White, _Rad, _Origin, _Scale, SpriteEffects.None, 1f); }
    }
}