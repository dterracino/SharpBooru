using System;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections.Specialized;
using HttpMultipartParser;

namespace TA.SharpBooru.Client.WebServer
{
    public class WebserverHelper
    {
        private const string DOCTYPE = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">";

        public static void WriteMinimalHeader(Context Context, string Title)
        {
            string header = DOCTYPE + @"<html><head><title>{0} - {1}</title><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""></head><body><div>";
            Context.OutWriter.Write(header, Title, Context.Booru.BooruInfo.BooruName);
        }

        public static void WriteHeader(Context Context, string Title, string AdditionalHeadContent)
        {
            if (AdditionalHeadContent == null)
                WriteHeader(Context, Title);
            else WriteHeader(Context, Title, new List<string>() { AdditionalHeadContent });
        }

        public static void WriteHeader(Context Context, string Title) { WriteHeader(Context, Title, string.Empty); }
        public static void WriteHeader(Context Context, string Title, List<string> AdditionalHeadContent)
        {
            Dictionary<string, string> links = new Dictionary<string, string>()
            {
                { "Index", "/" },
                //{ "Random", "/post?id=-1" },
                { "GitHub", "http://www.github.com/teamalpha5441/SharpBooru" }
                //{ "Info", "/info" }
            };
            /*
            if (Context.User.Perm_Upload)
                links.Add("Upload", "/upload");
            if (Context.User.Perm_Admin)
                links.Add("Admin", "/admin");
            */
            string booruName = Context.Booru.BooruInfo.BooruName;

            Context.OutWriter.Write("{0}<html><head><title>{1} - {2}</title>", DOCTYPE, Title, booruName);
            Context.OutWriter.Write("<link rel=\"stylesheet\" type=\"text/css\" href=\"style\">");
            Context.OutWriter.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
            if (AdditionalHeadContent != null)
                AdditionalHeadContent.ForEach(x => Context.OutWriter.Write(x));
            Context.OutWriter.Write("</head><body><div class=\"main\">");
            Context.OutWriter.Write("<div class=\"header\"><div class=\"innerheader\">");
            Context.OutWriter.Write("<span class=\"hl\"><a href=\"/\">{0}</a></span><br>", booruName);
            foreach (KeyValuePair<string, string> link in links)
            {
                Context.OutWriter.Write("<a href=\"{0}\">{1}</a>", link.Value, link.Key);
                if (!link.Equals(links.Last()))
                    Context.OutWriter.Write(" | ");
            }
            /*
            Context.OutWriter.Write("</div><div class=\"login\">");
            if (Context.LoggedIn)
            {
                Context.OutWriter.Write("<form action=\"/login_logout\" method=\"GET\">");
                Context.OutWriter.Write("<input style=\"float: right\" type=\"submit\" value=\"Logout\"></form>");
            }
            else
            {
                Context.OutWriter.Write("<form method=\"POST\" action=\"/login_logout\">");
                Context.OutWriter.Write("<input class=\"login\" type=\"text\" name=\"username\">");
                Context.OutWriter.Write(" <input class=\"login\" type=\"password\" name=\"password\">");
                Context.OutWriter.Write(" <input style=\"float: left\" type=\"submit\" value=\"Login\"></form>");
            }
            */
            string serverMOTD = ""; //TODO Get server MOTD
            if (!string.IsNullOrWhiteSpace(serverMOTD))
            {
                Context.OutWriter.Write("</div><div class=\"motd\"><b>Notice: </b>");
                Context.OutWriter.Write(serverMOTD);
            }
            Context.OutWriter.Write("</div></div><div class=\"body\">");
        }

        public static void WriteFooter(Context Context) { Context.OutWriter.Write("</div></div></body></html>"); }

        public static void WriteMinimalFooter(Context Context) { Context.OutWriter.Write("</div></body></html>"); }

        //TODO Get server thumbs size
        public static string GetStyle(Context Context) { return string.Format(Properties.Resources.style_css, 120); }

        public static void WriteTableHeader(Context Context, string TableClass = null)
        {
            string classString = " class=\"" + TableClass + "\"";
            Context.OutWriter.Write("<table{0}><tr><td class=\"nav\">", TableClass == null ? string.Empty : classString);
        }

        public static void WriteTableFooter(Context Context) { Context.OutWriter.Write("</td></tr></table>"); }

        public static void WriteTableMiddle(Context Context) { Context.OutWriter.Write("</td><td style=\"padding-left: 14px\">"); }

        public static void WriteSearchTextBox(Context Context, string Target, string Value = null)
        {
            Context.OutWriter.Write("<form action=\"{0}\" method=\"GET\">", Target);
            Context.OutWriter.Write("<input class=\"search\" type=\"text\" name=\"tags\" value=\"{0}\">", Value ?? string.Empty);
            Context.OutWriter.Write("</form>");
        }

        public static DictionaryEx ParseParameters(string Query)
        {
            NameValueCollection nvParams = HttpUtility.ParseQueryString(Query);
            DictionaryEx dictEx = new DictionaryEx();
            foreach (string key in nvParams.AllKeys)
                dictEx.Add(key, nvParams[key]);
            return dictEx;
        }

        public static void WriteBackButton(Context Context, bool WriteBrBr = false, string CustomTest = null)
        {
            if (WriteBrBr)
                Context.OutWriter.Write("<br><br>");
            string referrerString = null;
            try
            {
                Uri referrerURI = Context.InnerContext.Request.UrlReferrer;
                if (referrerURI != null)
                    referrerString = referrerURI.ToString();
            }
            catch { }
            if (string.IsNullOrWhiteSpace(referrerString)) //Referrer refreshes page, so it's preferred
                referrerString = "javaScript:window.history.back();"; //Solution if referrer doesn't work
            Context.OutWriter.Write("<a href=\"{1}\">{0}</a>", CustomTest ?? "Back", referrerString);
        }

        private static Dictionary<int, string> _HTTPCodeDescriptionDictionary;
        public static string GetHTTPCodeDescription(int HTTPCode)
        {
            if (_HTTPCodeDescriptionDictionary == null)
                _HTTPCodeDescriptionDictionary = new Dictionary<int, string>()
                {
                    { 200, "OK" },

                    { 301, "Moved Permanently" },
                    { 302, "Found" },
                    { 303, "See Other" },
                    { 304, "Not Modified" },
                    { 305, "Use Proxy" },
                    { 307, "Temporary Redirect" },

                    { 400, "Bad Request" },
                    { 401, "Unauthorized" },
                    { 403, "Access Denied" }, //Found
                    { 404, "Not Found" },
                    { 405, "Method Not Allowed" },
                    { 410, "Gone" },
                    { 421, "Too Many Connections" }, //There are too many connections from your internet address
                    { 424, "Failed Dependency" },
                    { 429, "Too Many Request" },

                    { 500, "Internal Server Error" },
                    { 501, "Not Implemented" },
                    { 503, "Service Unavailable" },
                    { 507, "Insufficient Storage" },
                    { 508, "Bandwith Limit Exceeded" }
                };
            if (_HTTPCodeDescriptionDictionary.ContainsKey(HTTPCode))
                return _HTTPCodeDescriptionDictionary[HTTPCode];
            else return ((HttpStatusCode)HTTPCode).ToString();
        }

        public static void ParseMultipartFormData(Stream Stream, out DictionaryEx POST, out List<Context.POSTFile> Files)
        {
            MultipartFormDataParser parser = new MultipartFormDataParser(Stream);
            POST = new DictionaryEx();
            if (parser.Parameters != null)
                foreach (ParameterPart paramPart in parser.Parameters.Values)
                    POST.Add(paramPart.Name, paramPart.Data);
            Files = new List<Context.POSTFile>();
            if (parser.Files != null)
                foreach (FilePart filePart in parser.Files)
                    Files.Add(new Context.POSTFile() { Name = filePart.Name, FileName = filePart.FileName, Data = filePart.Data });
        }

        public static void WriteSubSection(Context Context, string Name, string Content, params object[] Args)
        {
            WriteSubSectionHeader(Context, Name);
            Context.OutWriter.Write(Content, Args);
            WriteSubSectionFooter(Context);
        }

        public static void WriteSubSectionHeader(Context Context, string Name) { Context.OutWriter.Write("<div class=\"sub\"><h2>{0}</h2><p>", Name); }

        public static void WriteSubSectionFooter(Context Context) { Context.OutWriter.Write("</p></div>"); }

        public static string MD5(string String, Encoding Encoding = null) { return MD5((Encoding ?? Encoding.Unicode).GetBytes(String)); }
        public static string MD5(byte[] Bytes)
        {
            StringBuilder sb = new StringBuilder(32);
            using (MD5CryptoServiceProvider MD5Provider = new MD5CryptoServiceProvider())
                Array.ForEach(
                    MD5Provider.ComputeHash(Bytes),
                    x => sb.AppendFormat("{0:x2}", x));
            return sb.ToString();
        }
    }
}
