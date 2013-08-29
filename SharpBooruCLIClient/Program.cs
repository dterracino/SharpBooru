using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;
using CommandLine;

namespace TA.SharpBooru.Client.CLI
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Options options = new Options();
            try
            {
                if (Parser.Default.ParseArguments(args, options))
                {
                    using (Booru booru = new Booru(options.EndPoint, options.Username, options.Password))
                    {
                        booru.Connect();
                        BooruConsole bConsole = new BooruConsole(booru);
                        bConsole.Start();
                    }
                    return 0;
                }
                else Console.WriteLine(options.GetUsage());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
            return 1;
        }
    }
}