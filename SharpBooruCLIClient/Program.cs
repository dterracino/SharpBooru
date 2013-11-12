using System;
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
                    using (ClientBooru booru = new ClientBooru(options.EndPoint, options.Username, options.Password))
                    {
                        booru.Connect();
                        BooruConsole bConsole = new BooruConsole(booru);
                        if (!string.IsNullOrWhiteSpace(options.Command))
                            bConsole.ExecuteCmdLine(options.Command);
                        else bConsole.StartInteractive();
                    }
                    return 0;
                }
                else return 1;
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
