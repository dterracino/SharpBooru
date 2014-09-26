using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Mono.Unix;

namespace TA.SharpBooru
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            UnixEndPoint endPoint = new UnixEndPoint(args[0]);
            using (Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, 0))
            {
                socket.Connect(endPoint);

                using (var post = new BooruPost())
                using (var image = BooruImage.FromFile(args[1]))
                {
                    Console.Write("Tags: ");
                    foreach (var tag in Console.ReadLine().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        post.Tags.Add(new BooruTag(tag));

                    Console.Write("Source: ");
                    post.Source = Console.ReadLine().Trim();

                    Console.Write("Description: ");
                    post.Description = Console.ReadLine().Trim();

                    Console.Write("Rating: ");
                    post.Rating = Convert.ToByte(Console.ReadLine());

                    Console.Write("Private: ");
                    post.Private = Convert.ToBoolean(Console.ReadLine());

                    using (ReaderWriter rw_out = new ReaderWriter(new NetworkStream(socket)))
                    {
                        rw_out.DisposeStreams = true;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (ReaderWriter rw = new ReaderWriter(ms))
                            {
                                rw.Write(args[2], true);
                                rw.Write(args[3], true);
                            }
                            Request(rw_out, RequestCode.Login, ms.ToArray());
                        }

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (ReaderWriter rw = new ReaderWriter(ms))
                            {
                                post.ToWriter(rw);
                                post.Tags.ToWriter(rw);
                                image.ToWriter(rw);
                            }
                            using (MemoryStream result = new MemoryStream(Request(rw_out, RequestCode.Add_Post, ms.ToArray())))
                            using (ReaderWriter result_rw = new ReaderWriter(result))
                                Console.WriteLine("OK - ID = " + result_rw.ReadULong());
                        }
                    }
                }
            }
            return 0;
        }

        private static byte[] Request(ReaderWriter RW, RequestCode RQ, byte[] Payload)
        {
            RW.Write((ushort)RQ);
            RW.Write(Payload, true);
            RW.Flush();
            if (RW.ReadBool())
                return RW.ReadBytes();
            else throw new Exception(RW.ReadString());
        }
    }
}
