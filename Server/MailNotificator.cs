using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class MailNotificator
    {
        private Logger _Logger;

        private string _Host;
        private ushort _Port;
        private NetworkCredential _Cred;

        private MailAddress _From = null;
        private MailAddress _To = null;

        public MailNotificator(Logger Logger, string Host, ushort Port, string Username, string Password, MailAddress From, MailAddress To)
        {
            _Logger = Logger;

            _Host = Host;
            _Port = Port;
            _Cred = new NetworkCredential(Username, Password);

            _From = From;
            _To = To;
        }

        private SmtpClient ConnectClient()
        {
            SmtpClient client = new SmtpClient(_Host, _Port);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = _Cred;
            return client;
        }

        public bool SendMail(string Subject, string Body)
        {
            List<Attachment> tmpList = null;
            return SendMail(Subject, Body, tmpList);
        }

        public bool SendMail(string Subject, string Body, Attachment Attachment)
        {
            var tmpList = new List<Attachment>(1);
            tmpList.Add(Attachment);
            return SendMail(Subject, Body, tmpList);
        }

        public bool SendMail(string Subject, string Body, List<Attachment> Attachments)
        {
            try
            {
                using (var client = ConnectClient())
                using (var mail = new MailMessage())
                {
                    mail.From = _From;
                    mail.To.Add(_To);
                    mail.Subject = Subject;
                    mail.Body = Body;
                    if (Attachments != null)
                        foreach (var att in Attachments)
                            mail.Attachments.Add(att);
                    client.Send(mail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _Logger.LogException("SendMail", ex);
                return false;
            }
        }

        public bool NotificatePostAdded(ulong ID, BooruPost Post, BooruImage Thumb)
        {
            StringBuilder mailBody = new StringBuilder();
            mailBody.AppendLine("User " + Post.User + " added post " + ID);
            mailBody.AppendLine("https://eggy.hopto.org/booru/post.php?id=" + ID);
            using (var stream = new MemoryStream(Thumb.Bytes))
            using (var att = new Attachment(stream, "thumb.jpg", "image/jpeg"))
                return SendMail("Post " + ID + " added", mailBody.ToString(), att);
        }
    }
}
