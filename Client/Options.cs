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

        [Option("desc", Required = false, HelpText = "The description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = (byte)7, Required = false, HelpText = "The content rating")]
        public byte Rating { get; set; }

        [Option("private", DefaultValue = false, Required = false, HelpText = "Private")]
        public bool Private { get; set; }
    }

    [Verb("addurl", HelpText = "Import a post via booru URL")]
    internal class AddUrlOptions : Options
    {
        [Option("url", Required = true, HelpText = "The URL to import")]
        public string URL { get; set; }

        [Option('t', "tags", Required = true, HelpText = "The posts tags")]
        public string Tags { get; set; }

        [Option("desc", Required = false, HelpText = "The description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = (byte)7, Required = false, HelpText = "The content rating")]
        public byte Rating { get; set; }

        [Option("private", DefaultValue = false, Required = false, HelpText = "Private")]
        public bool Private { get; set; }
    }

    [Verb("del", HelpText = "Delete a post")]
    internal class DelOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }
    }

    [Verb("get", HelpText = "Get a post")]
    internal class GetOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }
    }

    [Verb("edit", HelpText = "Edit a post")]
    internal class EditOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }

        [Option("tags-no-delta", Required = false, HelpText = "--tags defines the new tags, not only delta")]
        public bool TagsNoDelta { get; set; }

        [Option('t', "tags", Required = false, HelpText = "The posts tags delta")]
        public string Tags { get; set; }

        [Option("source", Required = false, HelpText = "The new image source")]
        public string Source { get; set; }

        [Option("desc", Required = false, HelpText = "Then new description")]
        public string Description { get; set; }

        [Option('r', "rating", Required = false, HelpText = "The new content rating")]
        public byte? Rating { get; set; }

        [Option("private", Required = false, HelpText = "New Private")]
        public bool? Private { get; set; }
    }
}
