using System.Net;
using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru.Client.GUI
{
    public class Options
    {
        [Option('s', "server", Required = false, HelpText = "Server IP or hostname + port")]
        public string Server { get; set; }

        [Option('u', "username", Required = false, HelpText = "The username for auto login")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "The password for auto login")]
        public string Password { get; set; }

        [HelpOption]
        public string GetUsage() { return HelpText.AutoBuild(this); }
    }
}