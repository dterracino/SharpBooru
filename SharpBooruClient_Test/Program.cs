using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TA.SharpBooru.Client;
using TA.SharpBooru;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace SharpBooruClient_Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            Booru booru = new Booru("localhost", 2400, "guest", "guest");
            booru.Connect();

        }
    }
}
