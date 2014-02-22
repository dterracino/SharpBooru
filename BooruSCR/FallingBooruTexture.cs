using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        private double _X;
        private double _Limit;
        private int _ScrHeight;
        private float _Scale;
        private double _SpreadDegrees;

        private double _Y;
        private double _Rad;
        private bool _MouseInside;

        public FallingBooruTexture(Random R, Texture2D Texture, double FallSpeed, double RotateSpeed, double MaxSpreadDegree, Vector2 Screen)
        {
            _Texture = Texture;
            _Origin = new Vector2(_Texture.Width / 2 - 0.5f, _Texture.Height / 2 - 0.5f);
            _X = Screen.X * R.NextDouble();
            _Limit = Math.Sqrt(_Origin.X * _Origin.X + _Origin.Y * _Origin.Y) * 1.25 + 10;
            _Y = 0 - _Limit - R.NextDouble() * 30;
            _RotateSpeed = RotateSpeed * GetVariator(R);
            if (R.Next(0, 2) > 0)
                _RotateSpeed *= -1;
            _FallSpeed = FallSpeed * GetVariator(R);
            _ScrHeight = (int)Screen.Y;
            _Rad = 2 * Math.PI * R.NextDouble();
            _Scale = (float)(R.NextDouble() * 0.5 + 0.75);
            _SpreadDegrees = MaxSpreadDegree * R.NextDouble() - MaxSpreadDegree / 2;
        }

        private double GetVariator(Random R) { return R.NextDouble() + 0.5; }

        public void Update(GameTime gameTime)
        {
            if (!Finished)
            {
                double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
                _Rad += elapsed * _RotateSpeed / 180 * Math.PI;
                _Y += elapsed * _FallSpeed;
                if (_SpreadDegrees > 0)
                    _X += elapsed * _FallSpeed * Math.Tan(Math.PI / 180 * _SpreadDegrees);
                Finished = _Y - _Limit > _ScrHeight;
            }
            if (!Finished)
            {
                MouseState state = Mouse.GetState();
                Vector2 scaledSize = new Vector2((float)(_Texture.Width * _Scale), (float)(_Texture.Height * _Scale));
                _MouseInside = ScreensaverHelper.MouseInRotatedRectangle(Mouse.GetState(), new Vector2((float)_X, (float)_Y), scaledSize, _Rad);
                float maxSize = Math.Max(scaledSize.X, scaledSize.Y);
                if (maxSize < 40 || (_MouseInside && state.LeftButton == ButtonState.Pressed))
                    _Scale -= 0.01f;
                Finished = !(_Scale > 0f);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!Finished)
            {
                Vector2 position = new Vector2((float)_X, (float)_Y);
                Color color = _MouseInside ? Color.Gray : Color.White;
                sb.Draw(_Texture, position, null, color, (float)_Rad, _Origin, _Scale, SpriteEffects.None, 0f);
            }
        }

        public void Dispose() { Finished = true; }
    }
}