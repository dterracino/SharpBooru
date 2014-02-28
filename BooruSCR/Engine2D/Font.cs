using System;
using System.Text;
using System.Drawing;

namespace TA.Engine2D
{
    public class Font : IDisposable
    {
        private Image _Image;

        public Font()
        {
            _Image = new Image(Texture.FromBytes(TA.SharpBooru.Client.ScreenSaver.Properties.Resources.ascii));
            _Image.SourceWidth = _Image.Width = 8;
            _Image.SourceHeight = _Image.Height = 16;
        }

        public void Dispose() { _Image.Dispose(); }

        public int Measure(string Text) { return 8 * Text.Length; }

        public void Draw(float X, float Y, string Text, params object[] Args) { Draw(X, Y, Color.White, Text, Args); }

        public void Draw(float X, float Y, Color Color, string Text, params object[] Args)
        {
            Text = string.Format(Text, Args);
            byte[] bytes = Encoding.ASCII.GetBytes(Text);
            _Image.X = X;
            _Image.Y = Y;
            for (int i = 0; i < bytes.Length; i++)
            {
                _Image.SourceX = (bytes[i] & 0x0F) * 8;
                _Image.SourceY = (bytes[i] >> 4) * 16;
                _Image.Draw();
                _Image.X += 8;
            }
        }
    }
}