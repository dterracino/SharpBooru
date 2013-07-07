using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace TA.SharpBooru.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Booru booru = Booru.ReadFromDisk("D:\\___BOORU");

            X509Certificate cert = new X509Certificate("C:\\server.pfx", "sharpbooru");
            BooruServer server = new BooruServer(booru, cert);

            server.Start();
            Console.WriteLine("Server running...");
            Console.ReadKey();
            server.Booru.SaveToDisk();
            server.Stop();
        }
    }
}