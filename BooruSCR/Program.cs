using System;
using CommandLine;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            BooruSCR game = null;
            try
            {
                if (Helper.IsWindows() || Helper.IsUnix())
                {
                    Options options = new Options();
                    if (Parser.Default.ParseArguments(args, options))
                    {
                        game = new BooruSCR(options);
                        game.Run();
                        return 0;
                    }
                    else throw new Exception("Error while parsing arguments");
                }
                else throw new PlatformNotSupportedException();
            }
            catch (Exception ex)
            {
                ScreensaverHelper.HandleException(ex);
                return 1;
            }
            finally
            {
                if (game != null)
                    game.Dispose();
            }
        }
    }
}