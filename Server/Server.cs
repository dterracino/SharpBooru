﻿using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class Server : IDisposable
    {
        private List<Thread> _RunningThreads = new List<Thread>();
        private object _ListLock = new object();
        private bool _IsRunning = true;
        private ServerBooru _Booru;
        private Logger _Logger;

        public Server(ServerBooru Booru, Logger Logger)
        {
            _Booru = Booru;
            _Logger = Logger;
        }

        public void AddAcceptedSocket(Socket Socket)
        {
            Thread thread = new Thread(new ThreadStart(() => _ClientHandlerStage1(Socket)));
            thread.Name = "ClientHandler";
            thread.Start();
            lock (_ListLock)
                _RunningThreads.Add(thread);
        }

        public void Dispose()
        {
            _IsRunning = false;
            lock (_ListLock)
                foreach (Thread thread in _RunningThreads)
                    thread.Abort();
        }

        private void _ClientHandlerStage1(Socket Socket)
        {
            try
            {
                using (var netStream = new NetworkStream(Socket))
                    _ClientHandlerStage2(netStream);
            }
            catch (Exception ex)
            {
                if (_IsRunning)
                    _Logger.LogException("ClientHandlerStage1", ex);
            }
            finally { Socket.Dispose(); }
        }

        private void _ClientHandlerStage2(Stream Stream)
        {
            using (ReaderWriter rw = new ReaderWriter(Stream))
            {
                BooruUser user = _Booru.DefaultUser;
                while (_IsRunning)
                {
                    RequestCode requestCode = (RequestCode)rw.ReadUShort();
                    if (requestCode != RequestCode.Disconnect)
                    {
                        byte[] payload = rw.ReadBytes();
                        _Logger.LogLine("Client request: RQ = {0}, {1} bytes payload", Enum.GetName(typeof(RequestCode), requestCode), payload.Length);
                        using (var inputMs = new MemoryStream(payload))
                        using (var outputMs = new MemoryStream())
                        {
                            try
                            {
                                using (var rw2 = new ReaderWriter(inputMs, outputMs))
                                    _ClientHandlerStage3((RequestCode)requestCode, rw2, ref user);
                                rw.Write(true);
                                rw.Write(outputMs.ToArray(), true);
                            }
                            catch (Exception ex)
                            {
                                _Logger.LogException("ClientRequest", ex);
                                rw.Write(false);
                                rw.Write(ex.Message, true);
                            }
                            rw.Flush();
                        }
                    }
                    else
                    {
                        _Logger.LogLine("Client disconnected gracefully");
                        break;
                    }
                }
            }
        }

        private void _ClientHandlerStage3(RequestCode RQ, ReaderWriter RW, ref BooruUser User)
        {
            switch (RQ)
            {
                default: throw new NotSupportedException("Unknown request code");

                case RequestCode.Get_Post:
                    {
                        ulong id = RW.ReadULong();
                        using (var post = _Booru.GetPost(User, id, false))
                            post.ToWriter(RW);
                    } break;

                case RequestCode.Get_Thumb:
                    {
                        ulong id = RW.ReadULong();
                        using (var thumb = _Booru.GetThumbnail(User, id))
                            thumb.ToWriter(RW);
                    } break;

                case RequestCode.Get_Image:
                    {
                        ulong id = RW.ReadULong();
                        using (var image = _Booru.GetImage(User, id))
                            image.ToWriter(RW);
                    } break;

                case RequestCode.Get_Tag:
                    {
                        ulong id = RW.ReadULong();
                        _Booru.GetTag(User, id).ToWriter(RW);
                    } break;

                case RequestCode.Get_Info:
                    _Booru.BooruInfo.ToWriter(RW);
                    break;

                case RequestCode.Get_AllTags:
                    {
                        List<string> tags = _Booru.GetAllTags();
                        RW.Write((uint)tags.Count);
                        foreach (string tag in tags)
                            RW.Write(tag, true);
                    } break;

                case RequestCode.Get_PostTags:
                    {
                        ulong id = RW.ReadULong();
                        using (var post = _Booru.GetPost(User, id, false))
                            post.Tags.ToWriter(RW);
                    } break;

                case RequestCode.Get_User:
                    User.ToWriter(RW);
                    break;

                case RequestCode.Search_String: //User limitations?
                    {
                        string pattern = RW.ReadString();
			List<ulong> ids = _Booru.Search(User, pattern);
                        RW.Write((uint)ids.Count);
                        foreach (ulong id in ids)
                            RW.Write(id);
                    } break;

                case RequestCode.Search_Image:
                    throw new NotImplementedException(); //TODO Implement

                case RequestCode.Login:
                    {
                        string username = RW.ReadString();
                        string password = RW.ReadString();
                        User = _Booru.Login(User, username, password);
                    } break;

                case RequestCode.Logout:
                    User = _Booru.DefaultUser;
                    break;

                case RequestCode.Add_Post:
                    using (var post = BooruPost.FromReader(RW))
                    {
                        post.Tags = BooruTagList.FromReader(RW);
                        post.Image = BooruImage.FromReader(RW);
                        ulong id = _Booru.AddPost(User, post);
                        RW.Write(id);
                    } break;
            }
        }
    }
}
