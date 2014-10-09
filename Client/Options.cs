using System;
using System.Text;
using System.Reflection;
using CommandLine;

namespace TA.SharpBooru
{
    internal class Options
    {
        [VerbOption("add")]
        public AddOptions AddVerb { get; set; }
    }

    internal class CommonOptions
    {
        [Option('s', "socket", DefaultValue = "socket.sock", Required = false)]
        public string Socket { get; set; }

        [Option('u', "username", DefaultValue = null, Required = false)]
        public string Username { get; set; }

        [Option('p', "password", DefaultValue = null, Required = false)]
        public string Password { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var sb = new StringBuilder();
            string productName = GetAssemblyAttribute<AssemblyProductAttribute>(x => x.Product);
            string copyright = GetAssemblyAttribute<AssemblyCopyrightAttribute>(x => x.Copyright);
            sb.AppendLine(productName + " V" + Helper.GetVersion());
            sb.Append(copyright);
            sb.AppendLine();

            sb.AppendLine("Common options");
            sb.AppendLine(" -s --socket      UNIX domain socket [socket.sock]");
            sb.AppendLine(" -u --username    Login username");
            sb.AppendLine(" -p --password    Login password");
            sb.AppendLine("    --help        Show this help screen");
            sb.AppendLine();

            sb.AppendLine("Subcommand \"add\" options");
            sb.AppendLine(" -i --image       Image file");
            sb.AppendLine(" -t --tags        Tags describing the image");
            sb.AppendLine(" -q --source      Image source (URL/path) []");
            sb.AppendLine(" -d --description Description/comment []");
            sb.AppendLine(" -r --rating      Image content rating [7]");
            sb.AppendLine("    --private     Private [no]");
            sb.AppendLine();

            return sb.ToString();
        }

        private string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }

    internal class AddOptions : CommonOptions
    {
        [Option('i', "image", Required = true)]
        public string ImagePath { get; set; }

        [Option('t', "tags", Required = true)]
        public string Tags { get; set; }

        [Option('q', "source", DefaultValue = "", Required = false)]
        public string Source { get; set; }

        [Option('d', "description", DefaultValue = "", Required = false)]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = (byte)7, Required = false)]
        public byte Rating { get; set; }

        [Option('p', "private", DefaultValue = false, Required = false)]
        public bool Private { get; set; }
    }
}
