using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class FallingBooruTexture
    {
        public bool Finished { get; private set; }

        private Texture2D _Texture;
        private double _RotateSpeed;
        private double _FallSpeed;
        private Vector2 _Origin;
        private float _X;
        private double _Limit;
        private int _ScrHeight;
        private float _Scale;

        private double _Y;
        private double _Rad;

        public FallingBooruTexture(Random R, Texture2D Texture, double FallSpeed, double RotateSpeed, Vector2 Screen)
        {
            _Texture = Texture;
            _Origin = new Vector2(_Texture.Width / 2 - 0.5f, _Texture.Height / 2 - 0.5f);
            _X = (float)(Screen.X * R.NextDouble());
            _Limit = Math.Sqrt(_Origin.X * _Origin.X + _Origin.Y * _Origin.Y) + 10;
            _Y = 0 - _Limit - R.NextDouble() * 30;
            _RotateSpeed = RotateSpeed * GetVariator(R);
            if (R.Next(0, 2) > 0)
                _RotateSpeed *= -1;
            _FallSpeed = FallSpeed * GetVariator(R);
            _ScrHeight = (int)Screen.Y;
            _Rad = 2 * Math.PI * R.NextDouble();
            _Scale = (float)(R.NextDouble() * 0.5 + 0.75);
        }

        private double GetVariator(Random R) { return R.NextDouble() + 0.5; }

        public void Update(GameTime gameTime)
        {
            if (!Finished)
            {
                double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
                _Rad += elapsed * _RotateSpeed / 180 * Math.PI;
                _Y += elapsed * _FallSpeed;
                Finished = _Y - _Limit > _ScrHeight;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!Finished)
                sb.Draw(_Texture, new Vector2(_X, (float)_Y), null, Color.White, (float)_Rad, _Origin, _Scale, SpriteEffects.None, 0f);
        }

        public void Dispose() { Finished = true; }
    }
}