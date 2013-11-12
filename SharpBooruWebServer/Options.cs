using System.Net;
using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru.Client.WebServer
{
    public class Options
    {
        [Option('s', "server", Required = false, DefaultValue = "127.0.0.1", HelpText = "Server IP or hostname + port")]
        public string Server { get; set; }

        [Option('u', "username", Required = false, DefaultValue = "guest", HelpText = "The username for auto login")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "guest", HelpText = "The password for auto login")]
        public string Password { get; set; }

        [Option('p', "port", Required = false, DefaultValue = (ushort)80, HelpText = "The port of the server")]
        public ushort Port { get; set; }

        [HelpOption]
        public string GetUsage() { return HelpText.AutoBuild(this); }
    }
}