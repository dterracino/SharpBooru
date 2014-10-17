using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Mono.Unix;
using CommandLine;

namespace TA.SharpBooru
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var pResult = Parser.Default.ParseArguments(args, new Type[1]
            {
                typeof(AddOptions)
            });

            if (pResult.Errors.Any())
            {

            }
            else
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

                                    ulong postID = 0;
                                    Request(ns, RequestCode.Add_Post, (rw) =>
                                        {
                                            post.ToWriter(rw);
                                            post.Tags.ToWriter(rw);
                                            image.ToWriter(rw);
                                        }, (rw) =>
                                        {
                                            postID = rw.ReadULong();
                                        });
                                    Console.WriteLine("OK - PostID = " + postID);
                                }
                            }
                        }
                        return 0;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            return 1;
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
