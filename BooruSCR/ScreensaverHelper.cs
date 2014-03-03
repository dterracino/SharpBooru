using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class ScreensaverHelper
    {
        public static Microsoft.Xna.Framework.Color ColorFromHSV(double h, double S, double V)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
                R = G = B = 0;
            else if (S <= 0)
                R = G = B = V;
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0: R = V; G = tv; B = pv; break;
                    case 1: R = qv; G = V; B = pv; break;
                    case 2: R = pv; G = V; B = tv; break;
                    case 3: R = pv; G = qv; B = V; break;
                    case 4: R = tv; G = pv; B = V; break;
                    case 5: R = V; G = pv; B = qv; break;
                    case 6: R = V; G = tv; B = pv; break;
                    case -1: R = V; G = pv; B = qv; break;
                    default: R = G = B = V; break;
                }
            }
            return new Microsoft.Xna.Framework.Color(Clamp((int)(R * 255.0)), Clamp((int)(G * 255.0)), Clamp((int)(B * 255.0)));
        }

        private static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        private static extern uint _MessageBox(IntPtr hWnd, string text, string caption, int options);
        //See http://www.pinvoke.net/default.aspx/Enums/MessageBoxOptions.html for options

        public static bool HandleException(Exception Ex)
        {
            if (Helper.IsWindows())
                _MessageBox(IntPtr.Zero, Ex.Message, Ex.GetType().Name, 0x10);
            else if (Helper.IsConsole())
                Console.WriteLine("{0}: {1}", Ex.GetType().Name, Ex.Message);
            else return false;
            return true;
        }

        public static string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }

        public static Vector2 RotatePoint(Vector2 Point, Vector2 Center, double Rad)
        {
            double u = Rad + Math.Atan2(Point.Y - Center.Y, Point.X - Center.X);
            double d = Math.Sqrt(Math.Pow(Point.X - Center.X, 2) + Math.Pow(Point.Y - Center.Y, 2));
            return new Vector2((float)(Center.X + d * Math.Cos(u)), (float)(Center.Y + d * Math.Sin(u)));
        }

        public static bool IsPointInPolygon(Vector2 Point, Vector2[] Polygon)
        {
            Vector2 p1, p2;
            bool inside = false;
            if (Polygon.Length < 3)
                return inside;
            Vector2 oldPoint = Polygon[Polygon.Length - 1];
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
                if ((Polygon[i].X < Point.X) == (Point.X <= oldPoint.X) && (Point.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (Point.X - p1.X))
                    inside = !inside;
                oldPoint = Polygon[i];
            }
            return inside;
        }

        public static bool MouseInRotatedRectangle(MouseState Mouse, Vector2 RectangleCenter, Vector2 RectangleSize, double Radiant)
        {
            float xMnsW = RectangleCenter.X - RectangleSize.X / 2; float xPlsW = RectangleCenter.X + RectangleSize.X / 2;
            float yMnsH = RectangleCenter.Y - RectangleSize.Y / 2; float yPlsH = RectangleCenter.Y + RectangleSize.Y / 2;
            Vector2[] points = new Vector2[4]
                {
                    new Vector2(xMnsW, yMnsH),
                    new Vector2(xPlsW, yMnsH),
                    new Vector2(xPlsW, yPlsH),
                    new Vector2(xMnsW, yPlsH)
                };
            if (Radiant != 0)
                for (byte i = 0; i < 4; i++)
                    points[i] = RotatePoint(points[i], RectangleCenter, Radiant);
            return IsPointInPolygon(new Vector2(Mouse.X, Mouse.Y), points);
        }

        public static Texture2D Texture2DFromBytes(GraphicsDevice GD, byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            using (Bitmap bitmap = new Bitmap(ms))
                return Texture2DFromBitmap(GD, bitmap);
        }

        public static Texture2D Texture2DFromBitmap(GraphicsDevice GD, Bitmap Bitmap)
        {
            var rect = new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
            BitmapData bd = Bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] data = new byte[Bitmap.Width * Bitmap.Height * 4];
            unsafe
            {
                byte* ptr = (byte*)bd.Scan0.ToPointer();
                for (int i = 0; i < data.Length; i += 4)
                {
                    data[i] = ptr[i + 2];
                    data[i + 1] = ptr[i + 1];
                    data[i + 2] = ptr[i];
                    data[i + 3] = ptr[i + 3];
                }
            }
            Bitmap.UnlockBits(bd);
            Texture2D texture = new Texture2D(GD, Bitmap.Width, Bitmap.Height, false, SurfaceFormat.Color);
            texture.SetData<byte>(data);
            return texture;
        }
    }
}