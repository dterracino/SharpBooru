using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using TA.SharpBooru.BooruAPIs;

namespace TA.SharpBooru.Client.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            using (Booru booru = new Booru("localhost", 2400, "guest", "guest")) //TODO Parse from args[]
            {
                booru.Connect();
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("BOORU> ");
                    Console.ForegroundColor = ConsoleColor.White;
                    string cmdLine = Console.ReadLine();
                    string[] splittedCmdLine = cmdLine.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splittedCmdLine.Length > 0)
                        try
                        {
                            if (!HandleCommand(booru, splittedCmdLine))
                                break;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(ex.Message);
                        }
                }
                return 0;
            }
        }

        public static bool HandleCommand(Booru booru, string[] args)
        {
            for (int i = 0; i < 2; i++)
                args[i] = args[i].ToLower();
            switch (args[0])
            {
                case "image":
                    {
                        ulong id = Convert.ToUInt64(args[2]);
                        using (BooruImage bImage = booru.GetImage(id))
                            switch (args[1])
                            {
                                case "get": bImage.Save(args[3]); break;
                                case "view":
                                case "open":
                                    {
                                        string tempFile = Helper.GetTempFile();
                                        tempFile = string.Format("{0}.{1}", tempFile, bImage.ImageFormatExtension);
                                        bImage.Save(tempFile);
                                        System.Diagnostics.Process.Start(tempFile);
                                    } break;
                            }
                    } break;
                case "post":
                    switch (args[1])
                    {
                        case "get":
                            {
                                ulong id = Convert.ToUInt64(args[2]);
                                BooruPost post = booru.GetPost(id);
                                Console.WriteLine("Post #{0} uploaded {1} by {2}:", post.ID, post.CreationDate, post.Owner);
                                Console.WriteLine("Viewed {0} times / Edited {1} times", post.ViewCount, post.EditCount);
                                Console.WriteLine("Rating {0}, Image size {1} x {2}", post.Rating, post.Width, post.Height);
                                Console.WriteLine("Source: {0}", post.Source);
                                Console.WriteLine("Post is {0}, Comment: {1}", post.Private ? "private" : "public", post.Comment);
                                Console.Write("Tags:"); post.Tags.ForEach(x => Console.Write(" {0}", x.Tag));
                                Console.WriteLine();
                            } break;
                        case "add": throw new NotImplementedException();
                        case "import":
                            string url = args[2];
                            List<BooruAPIPost> apiPosts = BooruAPI.SearchPostsPerURL(url);
                            //TODO Implement CLI post import
                            break;
                        case "delete": throw new NotImplementedException();
                        case "edit": throw new NotImplementedException();
                    } break;
                case "tag":
                    {
                        ulong id = Convert.ToUInt64(args[2]);
                        switch (args[1])
                        {
                            case "edit": throw new NotImplementedException();
                            case "delete": booru.DeleteTag(id); break;
                        } break;
                    }
                case "admin":
                    switch (args[1])
                    {
                        case "kill": booru.ForceKillServer(); break;
                        case "save": booru.SaveServerBooru(); break;
                    } break;
                case "exit":
                case "close":
                case "quit": return false;
            }
            return true;
        }
    }
}