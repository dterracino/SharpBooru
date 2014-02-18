using System;
using System.Net;
using System.Text;
using System.Reflection;
using CommandLine;

namespace TA.SharpBooru.Client.ScreenSaver
{
    public class Options
    {
        [Option('s', "server", Required = false, DefaultValue = null)]
        public string internalServer { get; set; }
        public IPEndPoint Server
        {
            get
            {
                if (internalServer != null)
                    return TA.SharpBooru.Helper.GetIPEndPointFromString(internalServer, 2400);
                else return new IPEndPoint(IPAddress.Loopback, 2400);
            }
            set { internalServer = string.Format("{0}:{1}", value.Address, value.Port); }
        }
        public bool IsServerDefined { get { return internalServer != null; } }

        [Option('u', "username", Required = false, DefaultValue = null)]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = null)]
        public string Password { get; set; }

        [Option('q', "search", Required = false, DefaultValue = null)]
        public string Search { get; set; }

        [Option('l', "dl-limit", Required = false, DefaultValue = 500)]
        public int ImageLimit { get; set; }

        [Option('f', "fps-limit", Required = false, DefaultValue = 60)]
        public int FPSLimit { get; set; }

        [Option('v', "no-vsync", Required = false, DefaultValue = true)]
        public bool NoVSync { get; set; }

        [Option('i', "use-images", Required = false, DefaultValue = false)]
        public bool UseImages { get; set; }

        [Option('d', "debug", Required = false, DefaultValue = false)]
        public bool Debug { get; set; }

        [Option('m', "image-size", Required = false, DefaultValue = 300)]
        public int ImageSize { get; set; }

        [HelpOption('h')]
        public string GetUsage()
        {
            StringBuilder sb = new StringBuilder();

            string productName = ScreensaverHelper.GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            sb.AppendFormat("{0} V{1}", productName, version);
            sb.AppendLine();
            string copyright = ScreensaverHelper.GetAssemblyAttribute<AssemblyCopyrightAttribute>(x => x.Copyright);
            sb.Append(copyright);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("  -s, --server <srv>[:prt] Server to connect to [localhost]");
            sb.AppendLine("  -u, --username <un>      Username for auto login");
            sb.AppendLine("  -p, --password <pw>      Password for auto login");
            sb.AppendLine("  -q  --search <str>       Search string [All posts]");
            sb.AppendLine("  -l  --image-limit <n>    Download max. n images [500]");
            //sb.AppendLine("  -f  --fps-limit <n>      Limit the FPS to n, (0 = unlimited) [60]");
            //sb.AppendLine("  -v  --no-vsync           Disable VSync");
            sb.AppendLine("  -i  --use-images         Use images instead of thumbnails");
            sb.AppendLine("  -d  --debug              Show debug information");
            sb.AppendLine("  -m  --image-size <n>     Image size [300]");
            sb.AppendLine("All switches are optional");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}