using System;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace TA.SharpBooru.Server
{
    public class Server : IDisposable
    {
        private SThreadPool _ThreadPool;
        private ServerBooru _Booru;
        private Logger _Logger;
        private MailNotificator _MN;

        public Server(ServerBooru Booru, Logger Logger, MailNotificator MailNotificator, ushort ThreadCount)
        {
            _ThreadPool = new SThreadPool(ThreadCount);
            _Booru = Booru;
            _Logger = Logger;
            _MN = MailNotificator;
        }

        public void AddConnectedClient(Stream Stream, X509Certificate2 Certificate)
        {
            Action action = () => _ClientHandlerStage1(Stream, Certificate);
            _ThreadPool.Queue(action);
        }

        public void Dispose() { _ThreadPool.Dispose(); }

        private void _ClientHandlerStage1(Stream Stream, X509Certificate2 Certificate)
        {
            try
            {
                if (Certificate != null)
                    using (SslStream ssl = new SslStream(Stream, true))
                    {
                        ssl.AuthenticateAsServer(Certificate);
                        _ClientHandlerStage2(Stream);
                    }
                else _ClientHandlerStage2(Stream);
            }
            catch (Exception ex)
            {
                if (_ThreadPool.IsRunning)
                    _Logger.LogException("ClientHandlerStage1", ex);
            }
            finally { Stream.Dispose(); }
        }

        private void _ClientHandlerStage2(Stream Stream)
        {
            using (ReaderWriter rw = new ReaderWriter(Stream))
            {
                BooruUser user = _Booru.DefaultUser;
                while (_ThreadPool.IsRunning)
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

                case RequestCode.Get_PostCount:
                    RW.Write(_Booru.GetPostCount());
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

                case RequestCode.Search_Tags:
                    {
                        string term = RW.ReadString();
			byte limit = RW.ReadByte();
                        _Booru.SearchTags(term, limit).ToWriter(RW);
                    } break;

                case RequestCode.Start_GC:
                    if (!User.IsAdmin)
                        throw new BooruException(BooruException.ErrorCodes.NoPermission);
                    else GC.Collect();
                    break;

                case RequestCode.Add_Post:
                    using (var post = BooruPost.FromReader(RW))
                    {
                        post.Tags = BooruTagList.FromReader(RW);
                        post.Image = BooruImage.FromReader(RW);
                        ulong id = _Booru.AddPost(User, post);
                        RW.Write(id);
                        using (var thumb = _Booru.GetThumbnail(null, id))
                            _MN.NotificatePostAdded(id, post, thumb);
                    } break;

                case RequestCode.Edit_Post:
                    {
                        using (var post = BooruPost.FromReader(RW))
                        {
                            post.Tags = BooruTagList.FromReader(RW);
                            _Booru.EditPost(User, post);
                        }
                    } break;

                case RequestCode.Edit_Image:
                    {
                        ulong id = RW.ReadULong();
                        using (var image = BooruImage.FromReader(RW))
                            _Booru.EditImage(User, id, image);
                    } break;

                case RequestCode.Delete_Post:
                    {
                        ulong id = RW.ReadULong();
                        _Booru.DeletePost(User, id);
                    } break;
            }
        }
    }
}
