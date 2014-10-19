﻿using System;
using System.Drawing;
using System.Collections.Generic;

public static class ColorHelper
{
    private static Color[] _Colors = null;
    private static Dictionary<int, byte> _Cache = new Dictionary<int, byte>(8);

    private static void FillColorsArray()
    {
        if (_Colors == null)
            _Colors = new Color[256]
            {
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(128, 0, 0),
                Color.FromArgb(0, 128, 0),
                Color.FromArgb(128, 128, 0),
                Color.FromArgb(0, 0, 128),
                Color.FromArgb(128, 0, 128),
                Color.FromArgb(0, 128, 128),
                Color.FromArgb(192, 192, 192),
                Color.FromArgb(128, 128, 128),
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(255, 255, 0),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(255, 0, 255),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(0, 0, 95),
                Color.FromArgb(0, 0, 135),
                Color.FromArgb(0, 0, 175),
                Color.FromArgb(0, 0, 215),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(0, 95, 0),
                Color.FromArgb(0, 95, 95),
                Color.FromArgb(0, 95, 135),
                Color.FromArgb(0, 95, 175),
                Color.FromArgb(0, 95, 215),
                Color.FromArgb(0, 95, 255),
                Color.FromArgb(0, 135, 0),
                Color.FromArgb(0, 135, 95),
                Color.FromArgb(0, 135, 135),
                Color.FromArgb(0, 135, 175),
                Color.FromArgb(0, 135, 215),
                Color.FromArgb(0, 135, 255),
                Color.FromArgb(0, 175, 0),
                Color.FromArgb(0, 175, 95),
                Color.FromArgb(0, 175, 135),
                Color.FromArgb(0, 175, 175),
                Color.FromArgb(0, 175, 215),
                Color.FromArgb(0, 175, 255),
                Color.FromArgb(0, 215, 0),
                Color.FromArgb(0, 215, 95),
                Color.FromArgb(0, 215, 135),
                Color.FromArgb(0, 215, 175),
                Color.FromArgb(0, 215, 215),
                Color.FromArgb(0, 215, 255),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 255, 95),
                Color.FromArgb(0, 255, 135),
                Color.FromArgb(0, 255, 175),
                Color.FromArgb(0, 255, 215),
                Color.FromArgb(0, 255, 255),
                Color.FromArgb(95, 0, 0),
                Color.FromArgb(95, 0, 95),
                Color.FromArgb(95, 0, 135),
                Color.FromArgb(95, 0, 175),
                Color.FromArgb(95, 0, 215),
                Color.FromArgb(95, 0, 255),
                Color.FromArgb(95, 95, 0),
                Color.FromArgb(95, 95, 95),
                Color.FromArgb(95, 95, 135),
                Color.FromArgb(95, 95, 175),
                Color.FromArgb(95, 95, 215),
                Color.FromArgb(95, 95, 255),
                Color.FromArgb(95, 135, 0),
                Color.FromArgb(95, 135, 95),
                Color.FromArgb(95, 135, 135),
                Color.FromArgb(95, 135, 175),
                Color.FromArgb(95, 135, 215),
                Color.FromArgb(95, 135, 255),
                Color.FromArgb(95, 175, 0),
                Color.FromArgb(95, 175, 95),
                Color.FromArgb(95, 175, 135),
                Color.FromArgb(95, 175, 175),
                Color.FromArgb(95, 175, 215),
                Color.FromArgb(95, 175, 255),
                Color.FromArgb(95, 215, 0),
                Color.FromArgb(95, 215, 95),
                Color.FromArgb(95, 215, 135),
                Color.FromArgb(95, 215, 175),
                Color.FromArgb(95, 215, 215),
                Color.FromArgb(95, 215, 255),
                Color.FromArgb(95, 255, 0),
                Color.FromArgb(95, 255, 95),
                Color.FromArgb(95, 255, 135),
                Color.FromArgb(95, 255, 175),
                Color.FromArgb(95, 255, 215),
                Color.FromArgb(95, 255, 255),
                Color.FromArgb(135, 0, 0),
                Color.FromArgb(135, 0, 95),
                Color.FromArgb(135, 0, 135),
                Color.FromArgb(135, 0, 175),
                Color.FromArgb(135, 0, 215),
                Color.FromArgb(135, 0, 255),
                Color.FromArgb(135, 95, 0),
                Color.FromArgb(135, 95, 95),
                Color.FromArgb(135, 95, 135),
                Color.FromArgb(135, 95, 175),
                Color.FromArgb(135, 95, 215),
                Color.FromArgb(135, 95, 255),
                Color.FromArgb(135, 135, 0),
                Color.FromArgb(135, 135, 95),
                Color.FromArgb(135, 135, 135),
                Color.FromArgb(135, 135, 175),
                Color.FromArgb(135, 135, 215),
                Color.FromArgb(135, 135, 255),
                Color.FromArgb(135, 175, 0),
                Color.FromArgb(135, 175, 95),
                Color.FromArgb(135, 175, 135),
                Color.FromArgb(135, 175, 175),
                Color.FromArgb(135, 175, 215),
                Color.FromArgb(135, 175, 255),
                Color.FromArgb(135, 215, 0),
                Color.FromArgb(135, 215, 95),
                Color.FromArgb(135, 215, 135),
                Color.FromArgb(135, 215, 175),
                Color.FromArgb(135, 215, 215),
                Color.FromArgb(135, 215, 255),
                Color.FromArgb(135, 255, 0),
                Color.FromArgb(135, 255, 95),
                Color.FromArgb(135, 255, 135),
                Color.FromArgb(135, 255, 175),
                Color.FromArgb(135, 255, 215),
                Color.FromArgb(135, 255, 255),
                Color.FromArgb(175, 0, 0),
                Color.FromArgb(175, 0, 95),
                Color.FromArgb(175, 0, 135),
                Color.FromArgb(175, 0, 175),
                Color.FromArgb(175, 0, 215),
                Color.FromArgb(175, 0, 255),
                Color.FromArgb(175, 95, 0),
                Color.FromArgb(175, 95, 95),
                Color.FromArgb(175, 95, 135),
                Color.FromArgb(175, 95, 175),
                Color.FromArgb(175, 95, 215),
                Color.FromArgb(175, 95, 255),
                Color.FromArgb(175, 135, 0),
                Color.FromArgb(175, 135, 95),
                Color.FromArgb(175, 135, 135),
                Color.FromArgb(175, 135, 175),
                Color.FromArgb(175, 135, 215),
                Color.FromArgb(175, 135, 255),
                Color.FromArgb(175, 175, 0),
                Color.FromArgb(175, 175, 95),
                Color.FromArgb(175, 175, 135),
                Color.FromArgb(175, 175, 175),
                Color.FromArgb(175, 175, 215),
                Color.FromArgb(175, 175, 255),
                Color.FromArgb(175, 215, 0),
                Color.FromArgb(175, 215, 95),
                Color.FromArgb(175, 215, 135),
                Color.FromArgb(175, 215, 175),
                Color.FromArgb(175, 215, 215),
                Color.FromArgb(175, 215, 255),
                Color.FromArgb(175, 255, 0),
                Color.FromArgb(175, 255, 95),
                Color.FromArgb(175, 255, 135),
                Color.FromArgb(175, 255, 175),
                Color.FromArgb(175, 255, 215),
                Color.FromArgb(175, 255, 255),
                Color.FromArgb(215, 0, 0),
                Color.FromArgb(215, 0, 95),
                Color.FromArgb(215, 0, 135),
                Color.FromArgb(215, 0, 175),
                Color.FromArgb(215, 0, 215),
                Color.FromArgb(215, 0, 255),
                Color.FromArgb(215, 95, 0),
                Color.FromArgb(215, 95, 95),
                Color.FromArgb(215, 95, 135),
                Color.FromArgb(215, 95, 175),
                Color.FromArgb(215, 95, 215),
                Color.FromArgb(215, 95, 255),
                Color.FromArgb(215, 135, 0),
                Color.FromArgb(215, 135, 95),
                Color.FromArgb(215, 135, 135),
                Color.FromArgb(215, 135, 175),
                Color.FromArgb(215, 135, 215),
                Color.FromArgb(215, 135, 255),
                Color.FromArgb(215, 175, 0),
                Color.FromArgb(215, 175, 95),
                Color.FromArgb(215, 175, 135),
                Color.FromArgb(215, 175, 175),
                Color.FromArgb(215, 175, 215),
                Color.FromArgb(215, 175, 255),
                Color.FromArgb(215, 215, 0),
                Color.FromArgb(215, 215, 95),
                Color.FromArgb(215, 215, 135),
                Color.FromArgb(215, 215, 175),
                Color.FromArgb(215, 215, 215),
                Color.FromArgb(215, 215, 255),
                Color.FromArgb(215, 255, 0),
                Color.FromArgb(215, 255, 95),
                Color.FromArgb(215, 255, 135),
                Color.FromArgb(215, 255, 175),
                Color.FromArgb(215, 255, 215),
                Color.FromArgb(215, 255, 255),
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(255, 0, 95),
                Color.FromArgb(255, 0, 135),
                Color.FromArgb(255, 0, 175),
                Color.FromArgb(255, 0, 215),
                Color.FromArgb(255, 0, 255),
                Color.FromArgb(255, 95, 0),
                Color.FromArgb(255, 95, 95),
                Color.FromArgb(255, 95, 135),
                Color.FromArgb(255, 95, 175),
                Color.FromArgb(255, 95, 215),
                Color.FromArgb(255, 95, 255),
                Color.FromArgb(255, 135, 0),
                Color.FromArgb(255, 135, 95),
                Color.FromArgb(255, 135, 135),
                Color.FromArgb(255, 135, 175),
                Color.FromArgb(255, 135, 215),
                Color.FromArgb(255, 135, 255),
                Color.FromArgb(255, 175, 0),
                Color.FromArgb(255, 175, 95),
                Color.FromArgb(255, 175, 135),
                Color.FromArgb(255, 175, 175),
                Color.FromArgb(255, 175, 215),
                Color.FromArgb(255, 175, 255),
                Color.FromArgb(255, 215, 0),
                Color.FromArgb(255, 215, 95),
                Color.FromArgb(255, 215, 135),
                Color.FromArgb(255, 215, 175),
                Color.FromArgb(255, 215, 215),
                Color.FromArgb(255, 215, 255),
                Color.FromArgb(255, 255, 0),
                Color.FromArgb(255, 255, 95),
                Color.FromArgb(255, 255, 135),
                Color.FromArgb(255, 255, 175),
                Color.FromArgb(255, 255, 215),
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(8, 8, 8),
                Color.FromArgb(18, 18, 18),
                Color.FromArgb(28, 28, 28),
                Color.FromArgb(38, 38, 38),
                Color.FromArgb(48, 48, 48),
                Color.FromArgb(58, 58, 58),
                Color.FromArgb(68, 68, 68),
                Color.FromArgb(78, 78, 78),
                Color.FromArgb(88, 88, 88),
                Color.FromArgb(98, 98, 98),
                Color.FromArgb(108, 108, 108),
                Color.FromArgb(118, 118, 118),
                Color.FromArgb(128, 128, 128),
                Color.FromArgb(138, 138, 138),
                Color.FromArgb(148, 148, 148),
                Color.FromArgb(158, 158, 158),
                Color.FromArgb(168, 168, 168),
                Color.FromArgb(178, 178, 178),
                Color.FromArgb(188, 188, 188),
                Color.FromArgb(198, 198, 198),
                Color.FromArgb(208, 208, 208),
                Color.FromArgb(218, 218, 218),
                Color.FromArgb(228, 228, 228),
                Color.FromArgb(238, 238, 238)
            };
    }

    public static byte GetXTermIndexFromColor(Color C)
    {
        double bestDistance = 500;
        byte bestByte = 0;

        int argb = C.ToArgb();
        foreach (var pair in _Cache)
            if (pair.Key == argb)
                return pair.Value;

        FillColorsArray();
        for (ushort i = 0; i < _Colors.Length; i++)
        {
            double distance = ColorDistance(_Colors[i], C);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestByte = (byte)i;
            }
        }

        _Cache.Add(C.ToArgb(), bestByte);
        return bestByte;
    }

    private static double ColorDistance(Color C1, Color C2)
    {
        double rdiff = C1.R - C2.R;
        double gdiff = C1.G - C2.G;
        double bdiff = C1.B - C2.B;
        return Math.Sqrt(rdiff * rdiff + gdiff * gdiff + bdiff * bdiff);
    }
}
