using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru.Client.GUI
{
    public class Options
    {
        [Option('s', "server", Required = true, HelpText = "Server IP or hostname + port")]
        public string Server { get; set; }

        [Option('u', "username", Required = false, DefaultValue = "guest", HelpText = "The username for auto login")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = "guest", HelpText = "The password for auto login")]
        public string Password { get; set; }

        [Option('c', "console", Required = false, DefaultValue = false, HelpText = "Use the console mode (no GUI)")]
        public bool Console { get; set; }

        [HelpOption('h', "help")]
        public string GetUsage() { return HelpText.AutoBuild(this); }
    }
}