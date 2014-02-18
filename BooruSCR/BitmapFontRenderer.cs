using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class BitmapFontRenderer : IDisposable
    {
        private Texture2D _Texture;

        public BitmapFontRenderer(GraphicsDevice GD)
        {
            using (MemoryStream ms = new MemoryStream(Properties.Resources.ascii))
                _Texture = Texture2D.FromStream(GD, ms);
        }

        public void Dispose() { _Texture.Dispose(); }

        public int Measure(string Text) { return 8 * Text.Length; }

        public void Draw(SpriteBatch SB, Vector2 Position, string Text, params object[] Args) { Draw(SB, Position, Color.White, Text, Args); }

        public void Draw(SpriteBatch SB, Vector2 Position, Color Color, string Text, params object[] Args)
        {
            Text = string.Format(Text, Args);
            byte[] bytes = Encoding.ASCII.GetBytes(Text);
            for (int i = 0; i < bytes.Length; i++)
            {
                Rectangle srcRect = new Rectangle((bytes[i] & 0x0F) * 8, (bytes[i] >> 4) * 16, 8, 16);
                Vector2 position = new Vector2(Position.X + i * 8, Position.Y);
                SB.Draw(_Texture, position, srcRect, Color);
            }
        }
    }
}