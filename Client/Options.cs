﻿using System;
using System.Reflection;
using CommandLine;

namespace TA.SharpBooru
{
    internal class Options
    {
        [Option("username", Required = false, HelpText = "Your username")]
        public string Username { get; set; }

        [Option("password", Required = false, HelpText = "Your password")]
        public string Password { get; set; }
    }

    [Verb("add", HelpText = "Adds a post")]
    internal class AddOptions : Options
    {
        [Option('i', "image", Required = true, HelpText = "The posts image")]
        public string ImagePath { get; set; }

        [Option('t', "tags", Required = true, HelpText = "The posts tags")]
        public string Tags { get; set; }

        [Option('s', "source", Required = false, HelpText = "The image source")]
        public string Source { get; set; }

        [Option('d', "description", Required = false, HelpText = "The description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = 7, Required = false, HelpText = "The content rating")]
        public int Rating { get; set; }

        [Option('p', "private", Required = false, HelpText = "Private")]
        public bool? Private { get; set; }
    }

    [Verb("addurl", HelpText = "Imports a post via booru URL")]
    internal class AddUrlOptions : Options
    {
        [Option('u', "url", Required = true, HelpText = "The URL to import")]
        public string URL { get; set; }

        [Option("custom-image", Required = false, HelpText = "The custom image to import")]
        public string CustomImagePath { get; set; }

        [Option("all-tags", DefaultValue = false, Required = false, HelpText = "Add all tags, not only already known")]
        public bool AllTags { get; set; }

        [Option("tags-no-delta", DefaultValue = false, Required = false, HelpText = "--tags defines all tags, not only delta")]
        public bool TagsNoDelta { get; set; }

        [Option('t', "tags", Required = false, HelpText = "Additional tags (delta)")]
        public string Tags { get; set; }

        [Option('d', "description", Required = false, HelpText = "The description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = 7, Required = false, HelpText = "The content rating")]
        public int Rating { get; set; }

        [Option('p', "private", Required = false, HelpText = "Private setting")]
        public bool? Private { get; set; }
    }

    [Verb("del", HelpText = "Deletes a post")]
    internal class DelOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }
    }

    [Verb("get", HelpText = "Gets a post")]
    internal class GetOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }

        [Option("no-color", Required = false, HelpText = "Disable ANSI color output")]
        public bool NoColor { get; set; }
    }

    [Verb("edit", HelpText = "Edits a post")]
    internal class EditOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post ID")]
        public ulong ID { get; set; }

        [Option("tags-no-delta", DefaultValue = false, Required = false, HelpText = "--tags defines all tags, not only delta")]
        public bool TagsNoDelta { get; set; }

        [Option('t', "tags", Required = false, HelpText = "The posts tags delta")]
        public string Tags { get; set; }

        [Option('s', "source", Required = false, HelpText = "The new image source")]
        public string Source { get; set; }

        [Option('d', "description", Required = false, HelpText = "Then new description")]
        public string Description { get; set; }

        [Option('r', "rating", DefaultValue = -1, Required = false, HelpText = "The new content rating")]
        public int Rating { get; set; }

        [Option('p', "private", Required = false, HelpText = "New private setting")]
        public bool? Private { get; set; }
    }

    [Verb("editimg", HelpText = "Edits a posts image")]
    internal class EditImgOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post/image ID")]
        public ulong ID { get; set; }

        [Option('e', "editor", Required = true, HelpText = "The image editor program")]
        public string Editor { get; set; }
    }

    [Verb("getimg", HelpText = "Gets a posts image")]
    internal class GetImgOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post/image ID")]
        public ulong ID { get; set; }

        [Option('p', "path", Required = true, HelpText = "The image path (w/o extension)")]
        public string Path { get; set; }
    }

    [Verb("setimg", HelpText = "Sets a posts image")]
    internal class SetImgOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post/image ID")]
        public ulong ID { get; set; }

        [Option('p', "path", Required = true, HelpText = "The image path")]
        public string Path { get; set; }
    }

    [Verb("setimgurl", HelpText = "Download an image and set it as a posts image")]
    internal class SetImgUrlOptions : Options
    {
        [Option('i', "id", Required = true, HelpText = "The post/image ID")]
        public ulong ID { get; set; }

        [Option('u', "url", Required = true, HelpText = "The image URL")]
        public string URL { get; set; }
    }

    [Verb("gc", HelpText = "Start the server garbage collector")]
    internal class GCOptions : Options { }
}
