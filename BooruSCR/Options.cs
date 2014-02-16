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

        [Option("search", Required = false, DefaultValue = null)]
        public string Search { get; set; }

        [HelpOption('h')]
        public string GetUsage()
        {
            StringBuilder sb = new StringBuilder();

            string productName = GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            sb.AppendFormat("{0} V{1}", productName, version);
            sb.AppendLine();
            string copyright = GetAssemblyAttribute<AssemblyCopyrightAttribute>(x => x.Copyright);
            sb.Append(copyright);
            sb.AppendLine();
            sb.AppendLine();
        
            sb.AppendLine("  -s, --server     Server to connect to [localhost]");
            sb.AppendLine("  -u, --username   Username for auto login");
            sb.AppendLine("  -p, --password   Password for auto login");
            sb.AppendLine("      --search     Search string");
            sb.AppendLine("All switches are optional");

            sb.AppendLine();
            return sb.ToString();
        }

        private string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }
}