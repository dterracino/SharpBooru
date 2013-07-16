using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Drawing;

namespace TEAM_ALPHA.Controls
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct Overlay
    {
        [FieldOffset(0)]
        public uint u32;
        [FieldOffset(0)]
        public byte u8_0;
        [FieldOffset(1)]
        public byte u8_1;
        [FieldOffset(2)]
        public byte u8_2;
        [FieldOffset(3)]
        public byte u8_3;
    }

    internal class ZoomItem
    {
        public int ZoomRate { get; set; }
        public string ZoomName { get; set; }

        public ZoomItem(int? zoomRate, string zoomName)
        {
            ZoomRate = zoomRate ?? Common.ORIGINALSCALEPERCENT;
            ZoomName = zoomName ?? Common.ORIGINALMENUITEMNAME;
        }

        public ZoomItem(string zoomName)
            : this(null, zoomName)
        {
        }

        public ZoomItem(int zoomRate)
            : this(zoomRate, zoomRate.ToString())
        {
        }
    }

    internal class Common
    {

        public const int MAXSCALEPERCENT = 1000;
        public const int ORIGINALSCALEPERCENT = 100;
        public const int MINSCALEPERCENT = 10;

        public const int PICTUREWIDTHMAX = 10000;
        public const int PICTUREHEIGHTMAX = 10000;
        public const int PICTUREWIDTHMIN = 100;
        public const int PICTUREHEIGHTMIN = 100;

        public static Color BGCOLOUR = Color.LightGray;

        public const double ZOOMINRATIO = 1.01;
        public const double ZOOMOUTRATIO = 1 / ZOOMINRATIO;

        public const int GENERALERROR = -1;
        public const int SUCCESSBUTNOTDONE = 0;
        public const int GENERALSUCCESS = 1;

        // The name of show original ToolStripMenuItem
        public const string ORIGINALMENUITEMNAME = "ShowOriginalToolStripMenuItem";
        // The name of fit width ToolStripMenuItem
        public const string FITWIDTHMENUITEMNAME = "FitWidthScaleToolStripMenuItem";
        // The name of fit height ToolStripMenuItem
        public const string FITHEIGHTMENUITEMNAME = "FitHeightScaleToolStripMenuItem";
    }
}