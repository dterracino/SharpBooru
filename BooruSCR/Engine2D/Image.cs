using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace TA.Engine2D
{
    public class Image : IDisposable
    {
        private Texture _Texture;
        public Texture Texture { get { return _Texture; } }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float OriginX { get; set; }
        public float OriginY { get; set; }
        public float Rad { get; set; }
        public float SourceX { get; set; }
        public float SourceY { get; set; }
        public float SourceWidth { get; set; }
        public float SourceHeight { get; set; }
        public Color Color { get; set; }

        public Image(Texture Texture)
        {
            _Texture = Texture;
            X = Y = OriginX = OriginY = Rad = SourceX = SourceY = 0;
            Width = SourceWidth = Texture.Width;
            Height = SourceHeight = Texture.Height;
            Color = Color.White;
        }

        public void Dispose() { _Texture.Dispose(); }

        public void Draw() { Draw(0f); }
        public void Draw(float Z)
        {
            GL.BindTexture(TextureTarget.TextureRectangle, _Texture.ID);
            GL.Enable(EnableCap.TextureRectangle);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(SourceX, SourceY);
            vertex3(X, Y, Z);
            GL.TexCoord2(SourceX + SourceWidth, SourceY);
            vertex3(X + Width, Y, Z);
            GL.TexCoord2(SourceX + SourceWidth, SourceY + SourceHeight);
            vertex3(X + Width, Y + Height, Z);
            GL.TexCoord2(SourceX, SourceY + SourceHeight);
            vertex3(X, Y + Height, Z);
            GL.End();
        }

        private void tint()
        {
            if (Color != Color.White)
                GL.Color3(Color);
        }

        private void vertex3(float X, float Y, float Z)
        {
            if (Rad != 0)
            {
                double u = Rad + Math.Atan2(Y - OriginY, X - OriginX);
                double d = Math.Sqrt(Math.Pow(X - OriginX, 2) + Math.Pow(Y - OriginY, 2));
                GL.Vertex3(OriginX + (float)(d * Math.Cos(u)), OriginY + (float)(d * Math.Sin(u)), Z);
            }
            else GL.Vertex3(X, Y, Z);
        }
    }
}