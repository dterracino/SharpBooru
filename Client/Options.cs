using System;
using System.Reflection;
using CommandLine;

namespace TA.SharpBooru
{
    internal class Options
    {
        [Option('s', "socket", DefaultValue = "socket.sock", Required = false, HelpText = "The UNIX socket")]
        public string Socket { get; set; }

        [Option('u', "username", Required = false, HelpText = "Your username")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "Your password")]
        public string Password { get; set; }

        /*
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
        */

        private string GetAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }

    [Verb("add", HelpText = "Add a post")]
    internal class AddOptions : Options
    {
        [Option('i', "image", Required = true, HelpText = "The posts image")]
        public string ImagePath { get; set; }

        [Option('t', "tags", Required = true, HelpText = "The posts tags")]
        public string Tags { get; set; }

        [Option("source", Required = false, HelpText = "The image source")]
        public string Source { get; set; }

        [Option("desc", Required = false, HelpText = "Comments/Description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = (byte)7, Required = false, HelpText = "The content rating")]
        public byte Rating { get; set; }

        [Option("private", DefaultValue = false, Required = false, HelpText = "Private")]
        public bool Private { get; set; }
    }
}
