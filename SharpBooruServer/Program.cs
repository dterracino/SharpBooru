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
            if (args.Length != 1)
                throw new ArgumentException("Server needs one argument");
            Booru sBooru = Booru.ReadFromDisk(args[0]);
            Logger sLogger = new Logger(Console.Out, Helper.IsPOSIX());
            X509Certificate sCertificate = new X509Certificate("C:\\server.pfx", "sharpbooru");
            BooruServer server = new BooruServer(sBooru, sLogger, sCertificate);

            server.Start();
            Console.ReadKey();
            server.Booru.SaveToDisk();
            server.Stop();
        }
    }
}