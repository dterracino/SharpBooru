using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class ScreensaverHelper
    {
        public static Color ColorFromHSV(double h, double S, double V)
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
            return new Color(Clamp((int)(R * 255.0)), Clamp((int)(G * 255.0)), Clamp((int)(B * 255.0)));
        }

        private static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        private static extern uint _MessageBox(IntPtr hWnd, string text, string caption, int options);
        //see http://www.pinvoke.net/default.aspx/Enums/MessageBoxOptions.html for options

        public static bool HandleException(Exception Ex)
        {
            if (Helper.IsWindows())
                _MessageBox(IntPtr.Zero, Ex.Message, Ex.GetType().Name, 0x10);
            else if (Helper.IsConsole())
                Console.WriteLine("{0}: {1}", Ex.GetType().Name, Ex.Message);
            else return false;
            return true;
        }
    }
}