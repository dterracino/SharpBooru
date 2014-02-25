using System;
using OpenTK.Input;
using TA.Engine2D;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class FallingImage : Image
    {
        public bool Finished { get; private set; }

        private float _RotateSpeed;
        private float _FallSpeed;
        private float _Limit;
        private int _ScrHeight;
        //private float _Scale;
        private float _SpreadDegrees;

        private float _Rad;
        private bool _MouseRightButtonDown;

        public event Action RightClick;

        public FallingImage(Random R, Texture Texture, float FallSpeed, float RotateSpeed, float MaxSpreadDegree, int ScreenWidth, int ScreenHeight)
            : base(Texture)
        {
            OriginX = Width / 2 - 0.5f;
            OriginY = Height / 2 - 0.5f;
            X = ScreenWidth * (float)R.NextDouble();
            _Limit = (float)Math.Sqrt(Math.Pow(OriginX, 2) + Math.Pow(OriginY, 2)) * 1.25f + 10;
            Y = 0 - _Limit - (float)R.NextDouble() * 30;
            _RotateSpeed = RotateSpeed * GetVariator(R);
            if (R.Next(0, 2) > 0)
                _RotateSpeed *= -1;
            _FallSpeed = FallSpeed * GetVariator(R);
            _ScrHeight = ScreenHeight;
            _Rad = (float)(2 * Math.PI * R.NextDouble());
            //_Scale = (float)(R.NextDouble() * 0.5 + 0.75);
            _SpreadDegrees = MaxSpreadDegree * (float)R.NextDouble() - MaxSpreadDegree / 2;
        }

        private float GetVariator(Random R) { return (float)R.NextDouble() + 0.5f; }

        public void Update(double ElapsedMilliseconds)
        {
            if (!Finished)
            {
                Rad += (float)(ElapsedMilliseconds * _RotateSpeed / 180 * Math.PI);
                Y += (float)ElapsedMilliseconds * _FallSpeed;
                if (_SpreadDegrees > 0)
                    X += (float)(ElapsedMilliseconds * _FallSpeed * Math.Tan(Math.PI / 180 * _SpreadDegrees));
                Finished = Y - _Limit > _ScrHeight;
            }
            /*
            if (!Finished)
            {
                MouseState state = Mouse.GetState();
                Vector2 scaledSize = new Vector2((float)(_Texture.Width * _Scale), (float)(_Texture.Height * _Scale));
                Color = ScreensaverHelper.MouseInRotatedRectangle(state, new Vector2((float)_X, (float)_Y), scaledSize, _Rad) ? Color.Gray : Color.White;
                float maxSize = Math.Max(scaledSize.X, scaledSize.Y);
                if (maxSize < 40 || (_MouseInside && state.LeftButton == ButtonState.Pressed))
                    _Scale -= 0.01f;
                Finished = !(_Scale > 0f);
                if (!_MouseRightButtonDown && state.RightButton == ButtonState.Pressed && _MouseInside && RightClick != null)
                    RightClick();
                _MouseRightButtonDown = state.RightButton == ButtonState.Pressed;
            }
            */
        }

        public new void Draw()
        {
            if (!Finished)
                base.Draw();
        }

        public new void Dispose()
        {
            Finished = true;
            base.Dispose();
        }
    }
}