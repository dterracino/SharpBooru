using CommandLine;
using CommandLine.Text;

namespace TA.SharpBooru.Server
{
    public class Options
    {
        [Option('l', "location", Required = true, HelpText = "The location of the booru")]
        public string Location { get; set; }

        [Option('p', "port", Required = false, DefaultValue = (ushort)2400, HelpText = "The port of the server")]
        public ushort Port { get; set; }

        [Option('u', "user", Required = false, DefaultValue = "nobody", HelpText = "Username used for SetUID")]
        public string User { get; set; }

        [Option('c', "certificate", Required = false, HelpText = "The .pfx server certificate (PFX password = 'sharpbooru')")]
        public string Certificate { get; set; }

        [HelpOption]
        public string GetUsage() { return HelpText.AutoBuild(this); }
    }
}