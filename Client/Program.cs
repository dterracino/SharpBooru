using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using TA.SharpBooru.BooruAPIs;
using CommandLine;
using Mono.Unix;

namespace TA.SharpBooru
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var pResult = Parser.Default.ParseArguments(args, new Type[]
            {
                typeof(AddOptions),
                typeof(AddUrlOptions),
                typeof(DelPostOptions)
            });

            if (!pResult.Errors.Any())
            {
                try
                {
                    var commonOptions = (Options)pResult.Value;
                    UnixEndPoint endPoint = new UnixEndPoint(commonOptions.Socket);

                    using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0))
                    {
                        socket.Connect(endPoint);
                        using (NetworkStream ns = new NetworkStream(socket, true))
                        {
                            if (!string.IsNullOrWhiteSpace(commonOptions.Username))
                                Request(ns, RequestCode.Login, (rw) =>
                                    {
                                        rw.Write(commonOptions.Username, true);
                                        rw.Write(commonOptions.Password ?? string.Empty, true);
                                    }, (rw) =>
                                    {
                                        //void
                                    });

                            if (commonOptions.GetType() == typeof(AddOptions))
                            {
                                var options = (AddOptions)commonOptions;
                                using (var post = new BooruPost())
                                using (var image = BooruImage.FromFile(options.ImagePath))
                                {
                                    foreach (var tag in options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                        post.Tags.Add(new BooruTag(tag));
                                    post.Source = options.Source;
                                    post.Description = options.Description;
                                    post.Rating = options.Rating;
                                    post.Private = options.Private;
                                    Console.Write("Adding post... ");
                                    ulong id = AddPost(ns, post, post.Tags, image);
                                    Console.WriteLine(id);
                                }
                            }
                            else if (commonOptions.GetType() == typeof(AddUrlOptions))
                            {
                                var options = (AddUrlOptions)commonOptions;
                                var apiPosts = BooruAPI.SearchPostsPerURL(options.URL);
                                Console.WriteLine("Importing " + apiPosts.Count + " posts");
                                for (int i = 0; i < apiPosts.Count; i++)
                                {
                                    Console.Write("Downloading image {0} of {1}... ", i + 1, apiPosts.Count);
                                    apiPosts[i].DownloadImage();
                                    Console.WriteLine("OK");
                                }
                                for (int i = 0; i < apiPosts.Count; i++)
                                    using (BooruAPIPost post = apiPosts[i])
                                    {
                                        foreach (var tag in options.Tags.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                            post.Tags.Add(new BooruTag(tag));
                                        if (options.Description != null)
                                            post.Description = options.Description;
                                        post.Rating = options.Rating;
                                        post.Private = options.Private;
                                        Console.Write("Importing post {0} of {1}... ", i + 1, apiPosts.Count);
                                        ulong id = AddPost(ns, post, post.Tags, post.Image);
                                        Console.WriteLine(id);
                                    }
                            }
                            else if (commonOptions.GetType() == typeof(DelPostOptions))
                            {
                                var options = (DelPostOptions)commonOptions;
                                Request(ns, RequestCode.Delete_Post, (rw) =>
                                    {
                                        rw.Write(options.ID);
                                    }, (rw) =>
                                    {
                                        //void
                                    });
                            }
                        }
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().Name);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                }
            }
            return 1;
        }

        private static ulong AddPost(NetworkStream NS, BooruPost Post, BooruTagList Tags, BooruImage Image)
        {
            ulong postID = 0;
            Request(NS, RequestCode.Add_Post, (rw) =>
            {
                Post.ToWriter(rw);
                Tags.ToWriter(rw);
                Image.ToWriter(rw);
            }, (rw) =>
            {
                postID = rw.ReadULong();
            });
            return postID;
        }

        private static void Request(NetworkStream NS, RequestCode RQ, Action<ReaderWriter> ReqCB, Action<ReaderWriter> RespCB)
        {
            using (ReaderWriter rw = new ReaderWriter(NS))
            {
                rw.Write((ushort)RQ);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ReaderWriter req_rw = new ReaderWriter(ms))
                        ReqCB(req_rw);
                    rw.Write(ms.ToArray(), true);
                }
                rw.Flush();
                if (rw.ReadBool())
                    using (MemoryStream ms = new MemoryStream(rw.ReadBytes()))
                    using (ReaderWriter resp_rw = new ReaderWriter(ms))
                        RespCB(resp_rw);
                else throw new Exception(rw.ReadString());
            }
        }
    }
}
