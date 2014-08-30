using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace TA.SharpBooru.Server.WebServer
{
    public class Context : IDisposable
    {
        //private const int EXPIRES_SECONDS = 60;
        public struct POSTFile
        {
            public string Name, FileName;
            public byte[] Data;
        }

        public class Parameters
        {
            private HttpListenerContext InnerContext;

            public Parameters(Context Context) { InnerContext = Context.InnerContext; }

            private DictionaryEx _GET = null;
            public DictionaryEx GET
            {
                get
                {
                    if (_GET == null)
                        _GET = WebserverHelper.ParseParameters(InnerContext.Request.Url.Query);
                    return _GET;
                }
            }

            private void ParsePOST()
            {
                _Files = new List<POSTFile>();
                Stream inStream = InnerContext.Request.InputStream;
                if (inStream == Stream.Null)
                    inStream = null;
                if (inStream == null)
                    _POST = new DictionaryEx();
                else
                {
                    string cType = InnerContext.Request.ContentType;
                    if (cType.StartsWith("application/x-www-form-urlencoded"))
                    {
                        using (StreamReader inReader = new StreamReader(inStream, InnerContext.Request.ContentEncoding))
                        {
                            string inData = inReader.ReadToEnd();
                            _POST = WebserverHelper.ParseParameters(inData);
                        }
                    }
                    else if (cType.StartsWith("multipart/form-data"))
                        WebserverHelper.ParseMultipartFormData(InnerContext.Request.InputStream, out _POST, out _Files);
                    else throw new NotImplementedException(string.Format("Content type {0} not supported", cType));
                }
            }

            private DictionaryEx _POST = null;
            public DictionaryEx POST
            {
                get
                {
                    if (_POST == null)
                        ParsePOST();
                    return _POST;
                }
            }

            private List<POSTFile> _Files;
            public List<POSTFile> Files
            {
                get
                {
                    if (_Files == null)
                        ParsePOST();
                    return _Files;
                }
            }
        }

        public bool IsClosed = false;
        public HttpListenerContext InnerContext;
        public BooruWebServer Server;

        public string Method { get { return InnerContext.Request.HttpMethod.ToUpper(); } }
        public IPAddress ClientIP { get { return InnerContext.Request.RemoteEndPoint.Address; } }
        public string RequestPath { get { return InnerContext.Request.Url.LocalPath ?? string.Empty; } }
        public string UserAgent { get { return InnerContext.Request.UserAgent ?? string.Empty; } }
        public ServerBooru Booru { get { return Server.Booru; } }

        public string MimeType
        {
            get { return InnerContext.Response.ContentType; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    InnerContext.Response.ContentType = value;
            }
        }

        public int HTTPCode
        {
            get { return InnerContext.Response.StatusCode; }
            set { InnerContext.Response.StatusCode = value; }
        }

        private StreamWriter _OutWriter = null;
        public StreamWriter OutWriter
        {
            get
            {
                if (_OutWriter == null)
                {
                    _OutWriter = new StreamWriter(
                        InnerContext.Response.OutputStream,
                        InnerContext.Response.ContentEncoding ?? Encoding.UTF8);
                    _OutWriter.NewLine = "\n";
                }
                return _OutWriter;
            }
        }

        /*
        private BooruUser _User = null;
        private bool _UserRead = false;
        public BooruUser User
        {
            get
            {
                if (!_UserRead)
                {
                    _User = Server.CookieManager.GetUser(InnerContext.Request.Cookies);
                    _UserRead = true;
                }
                return _User ?? Server.UserManager.GuestUser;
            }
            set
            {
                _User = value;
                _UserRead = true;
            }
        }
        */

        private Parameters _Params;
        public Parameters Params
        {
            get
            {
                if (_Params == null)
                    _Params = new Parameters(this);
                return _Params;
            }
        }

        //public bool LoggedIn { get { return User != Server.UserManager.GuestUser; } }

        public Context(BooruWebServer Server, HttpListenerContext HttpListenerContext)
        {
            this.Server = Server;
            this.InnerContext = HttpListenerContext;
            this.HTTPCode = 200;
            this.MimeType = "text/html";
            SetHeader("Server", string.Format("SharpBooruWebServer V{0} ", Helper.GetVersionMajor()));
            //SetHeader("Expires", (DateTime.Now + TimeSpan.FromSeconds(EXPIRES_SECONDS)).ToUniversalTime().ToString("r"));
            //SetHeader("Cache-Control", string.Format("max-age={0}", EXPIRES_SECONDS));
        }

        private void SetHeader(string Header, string Value)
        {
            foreach (string strHeader in InnerContext.Response.Headers)
                if (strHeader == Header)
                {
                    InnerContext.Response.Headers[strHeader] = Value;
                    return;
                }
            InnerContext.Response.AddHeader(Header, Value);
        }

        public void Flush()
        {
            if (_OutWriter != null)
                OutWriter.Flush();
        }

        public void Close() { Dispose(); }
        public void Dispose()
        {
            if (_OutWriter != null)
                try 
                {
                    OutWriter.Flush();
                    OutWriter.Close();
                }
                catch { }
            try { InnerContext.Response.Close(); }
            catch { }
            IsClosed = true;
        }
    }
}