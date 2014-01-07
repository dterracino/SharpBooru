using System;
using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru
{
    public class Options
    {
        [Option('m', "mode", Required = false, DefaultValue = "gui", HelpText = "The run mode: gui|cli|server|webserver")]
        public string _Mode { get; set; }
        public RunMode Mode
        {
            get
            {
                RunMode mode;
                if (System.Enum.TryParse<RunMode>(_Mode, true, out mode))
                    return mode;
                else return RunMode.GUI; //Default mode
            }
            set { _Mode = System.Enum.GetName(typeof(RunMode), value); }
        }

        [Option('s', "server", Required = false, DefaultValue = "127.0.0.1", HelpText = "All clients - Server IP or hostname + port")]
        public string Server { get; set; }

        [Option('u', "username", Required = false, DefaultValue = "guest", HelpText = "All clients - The username for auto login; Server only - The username for SetUID")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "guest", HelpText = "All clients - The password for auto login")]
        public string Password { get; set; }

        [Option('c', "command", Required = false, HelpText = "CLI only - The command to execute automatically")]
        public string Command { get; set; }

        [Option("port", Required = false, HelpText = "Server and WebServer - The listener port")]
        public ushort Port { get; set; }

        [Option('l', "location", Required = false, DefaultValue = null, HelpText = "Server only - The location of the booru")]
        public string Location { get; set; }

        /*
        [Option('c', "certificate", Required = false, HelpText = "The .pfx server certificate")]
        public string Certificate { get; set; }

        [Option('w', "certificate-password", Required = false, DefaultValue = "sharpbooru", HelpText = "The password for the certificate")]
        public string CertificatePassword { get; set; }
        */

        [HelpOption]
        public string GetUsage() { return HelpText.AutoBuild(this); }

        public enum RunMode { GUI, CLI, Server, WebServer }
    }
}