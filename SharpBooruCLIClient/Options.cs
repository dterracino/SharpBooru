using System.Net;
using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru.Client.CLI
{
    public class Options
    {
        [Option('s', "server", Required = true, HelpText = "Server IP or hostname + port")]
        public string Server { get; set; }

        public IPEndPoint EndPoint { get { return Helper.GetIPEndPointFromString(Server); } }

        [Option('u', "username", Required = false, DefaultValue = "guest", HelpText = "The username for auto login")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "guest", HelpText = "The password for auto login")]
        public string Password { get; set; }

        [Option('c', "command", Required = false, HelpText = "The command to execute automatically")]
        public string Command { get; set; }

        [HelpOption]
        public string GetUsage() { return HelpText.AutoBuild(this); }
    }
}