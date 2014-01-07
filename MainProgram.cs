using System;

namespace TA.SharpBooru
{
    public class MainProgram
    {
        public static int Main(string[] args)
        {
            Console.Title = "SharpBooru";
            if (args.Length > 1)
            {
                string mode = args[0];

                string[] oldArgs = args;
                args = new string[oldArgs.Length];
                Array.Copy(oldArgs, 1, args, 0, args.Length);
                
                if (!string.IsNullOrWhiteSpace(mode))
                {
                    mode = mode.Trim().ToLower();
                    if (mode == "server")
                        return RunServer(args);
                    else if (mode == "client" || mode == "gui")
                        return RunClient(args, true);
                    else if (mode == "cli")
                        return RunClient(args, false);
                    else if (mode == "clistandalone")
                        return RunStandalone(args, false);
                }
            }
            return RunClient(args, true);
            //return RunStandalone(args, true); //Default mode
        }

        private static int RunServer(string[] args)
        {
            return Server.Program.subMain(args);
        }

        private static int RunClient(string[] args, bool gui)
        {
            if (gui)
                return Client.GUI.Program.subMain(args);
            else return Client.CLI.Program.subMain(args);
        }

        private static int RunStandalone(string[] args, bool gui)
        {
            throw new NotImplementedException();
        }
    }
}
