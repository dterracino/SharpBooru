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
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.TextureRectangle);
            GL.BindTexture(TextureTarget.TextureRectangle, _Texture.ID);
            GL.Begin(PrimitiveType.Quads);
            vertex(SourceX, SourceY, X, Y, Z);
            vertex(SourceX + SourceWidth, SourceY, X + Width, Y, Z);
            vertex(SourceX + SourceWidth, SourceY + SourceHeight, X + Width, Y + Height, Z);
            vertex(SourceX, SourceY + SourceHeight, X, Y + Height, Z);
            GL.End();
        }

        private void vertex(float s, float t, float x, float y, float z)
        {
            GL.TexCoord2(s, t);
            if (Color != Color.White)
                GL.Color3(Color);
            if (Rad != 0)
                RotatePoint(ref x, ref y);
            GL.Vertex3(x, y, z);
        }

        public void RotatePoint(ref float X, ref float Y)
        {
            double u = Rad + Math.Atan2(Y - OriginY, X - OriginX);
            double d = Math.Sqrt(Math.Pow(X - OriginX, 2) + Math.Pow(Y - OriginY, 2));
            X = OriginX + (float)(d * Math.Cos(u));
            Y = OriginY + (float)(d * Math.Sin(u));
        }

        public bool IsMouseInside(int MouseX, int MouseY)
        {
            float[] xs = new float[4] { X, X + Width, X + Width, X };
            float[] ys = new float[4] { Y, Y, Y + Height, Y + Height };
            PointF[] points = new PointF[4];
            for (byte i = 0; i < 4; i++)
            {
                if (Rad != 0)
                    RotatePoint(ref xs[i], ref ys[i]);
                points[i] = new PointF(xs[i], ys[i]);
            }
            return IsPointInPolygon(new Point(MouseX, MouseY), points);
        }

        public static bool IsPointInPolygon(Point M, PointF[] Polygon)
        {
            PointF p1, p2;
            bool inside = false;
            if (Polygon.Length < 3)
                return inside;

            PointF oldPoint = Polygon[Polygon.Length - 1];
            for (int i = 0; i < Polygon.Length; i++)
            {
                if (Polygon[i].X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = Polygon[i];
                }
                else
                {
                    p1 = Polygon[i];
                    p2 = oldPoint;
                }
                if ((Polygon[i].X < M.X) == (M.X <= oldPoint.X) && (M.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (M.X - p1.X))
                    inside = !inside;
                oldPoint = Polygon[i];
            }
            return inside;
        }
    }
}