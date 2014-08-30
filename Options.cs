﻿using System;
using System.Net;
using System.Text;
using System.Reflection;
using CommandLine;

namespace TA.SharpBooru
{
    public class Options
    {
        [Option('m', "mode", Required = false, DefaultValue = "gui")]
        public string internalMode { get; set; }
        public RunMode Mode
        {
            get
            {
                RunMode mode;
                if (System.Enum.TryParse<RunMode>(internalMode, true, out mode))
                    return mode;
                else return RunMode.GUI;
            }
            set { internalMode = System.Enum.GetName(typeof(RunMode), value); }
        }

        [Option('s', "server", Required = false, DefaultValue = null)]
        public string internalServer { get; set; }
        public IPEndPoint Server
        {
            get
            {
                if (internalServer != null)
                    return Helper.GetIPEndPointFromString(internalServer, 2400);
                else return new IPEndPoint(IPAddress.Loopback, 2400);
            }
            set { internalServer = string.Format("{0}:{1}", value.Address, value.Port); }
        }
        public bool IsServerDefined { get { return internalServer != null; } }

        [Option('u', "username", Required = false, DefaultValue = null)]
        public string Username { get; set; }

        [Option('p', "password", Required = false, DefaultValue = null)]
        public string Password { get; set; }

        [Option('k', "keypair", Required = false, DefaultValue = null)]
        public string Keypair { get; set; }

        [Option('c', "command", Required = false, DefaultValue = null)]
        public string Command { get; set; }

        [Option('l', "location", Required = false, DefaultValue = null)]
        public string Location { get; set; }

        [Option("accept-fp", Required = false, DefaultValue = false)]
        public bool AcceptFingerprint { get; set; }

        [HelpOption('h')]
        public string GetUsage() 
        {
            StringBuilder sb = new StringBuilder();

            string productName = GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
            sb.AppendFormat("{0} V{1} PV{2}", productName, Helper.GetVersionMajor(), Helper.GetVersionMinor());
            sb.AppendLine();
            string copyright = GetAssemblyAttribute<AssemblyCopyrightAttribute>(x => x.Copyright);
            sb.Append(copyright);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("This program can run in multiple modes:");
            sb.AppendLine("  -m, --mode       Mode can be server, gui or cli");
            sb.AppendLine();
            sb.AppendLine("The other command line switches depend on the mode");
            sb.AppendLine();

            sb.AppendLine("Server Mode [-m server]");
            sb.AppendLine("  -l, --location   Location of the booru");
            sb.AppendLine();

            sb.AppendLine("GUI Mode [-m gui]");
            sb.AppendLine("  -s, --server     Server to connect to [localhost]");
            sb.AppendLine("  -u, --username   Username for auto login");
            sb.AppendLine("  -p, --password   Password for auto login");
            sb.AppendLine("  -k, --keypair    Keypair XML file for auto login");
            sb.AppendLine("      --accept-fp  Accept the server fingerprint");
            sb.AppendLine("All switches are optional");
            sb.AppendLine("A keypair file has priority over username and password");
            sb.AppendLine();

            sb.AppendLine("CLI Mode [-m cli]");
            sb.AppendLine("  -s, --server     Server to connect to [localhost]");
            sb.AppendLine("  -u, --username   Username for auto login");
            sb.AppendLine("  -p, --password   Password for auto login");
            sb.AppendLine("  -k, --keypair    Keypair XML file for auto login");
            sb.AppendLine("  -c, --command    Command to execute");
            sb.AppendLine("      --accept-fp  Accept the server fingerprint");
            sb.AppendLine("All switches are optional");
            sb.AppendLine("A keypair file has priority over username and password");
            sb.AppendLine();

            return sb.ToString();
        }

        private string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }

        public enum RunMode { GUI, CLI, Server }
    }
}