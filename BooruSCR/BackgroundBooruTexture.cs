using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class BackgroundBooruTexture : IDisposable
    {
        private Vector2 _Screen;

        private Rectangle _DrawRectangle;
        private Texture2D _Texture;

        public bool IsBackgroundAvailable { get { return _Texture != null; } }

        public BackgroundBooruTexture(float W, float H) { _Screen = new Vector2(W, H); }

        public void Dispose()
        {
            if (_Texture != null)
                _Texture.Dispose();
        }

        public void Draw(SpriteBatch SB, Color Color)
        {
            if (_Texture != null)
                SB.Draw(_Texture, _DrawRectangle, Color);
        }

        public void SetBooruImage(GraphicsDevice GD, BooruImage Image)
        {
            Dispose();
            using (MemoryStream ms = new MemoryStream(Image.Bytes))
                _Texture = Texture2D.FromStream(GD, ms);
            float num = Math.Max(_Screen.X / _Texture.Width, _Screen.Y / _Texture.Height);
            Vector2 resultSize = new Vector2(_Texture.Width * num, _Texture.Height * num);
            _DrawRectangle = new Rectangle(
                0 - (int)((resultSize.X - _Screen.X) / 2 + 0.5),
                0 - (int)((resultSize.Y - _Screen.Y) / 2 + 0.5), 
                (int)resultSize.X, (int)resultSize.Y);
        }
    }
}