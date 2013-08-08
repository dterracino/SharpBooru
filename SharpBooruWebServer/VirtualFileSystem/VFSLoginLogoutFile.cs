using System;
using System.IO;
using System.Net;

namespace TA.SharpBooru.Client.WebServer.VFS
{
    public class VFSLoginLogoutFile : VFSFile
    {
        public VFSLoginLogoutFile(string Name) { this.Name = Name; }

        public override void Execute(Context Context)
        {
            if (Context.LoggedIn)
            {
                Context.Server.CookieManager.DeleteUser(Context.User);
                Context.InnerContext.Response.RedirectLocation = Context.InnerContext.Request.UrlReferrer.ToString();
                Context.HTTPCode = 302;
            }
            else if ((Context.Params.GET.IsSet<string>("username") && Context.Params.GET.IsSet<string>("password"))
                     || (Context.Params.POST.IsSet<string>("username") && Context.Params.POST.IsSet<string>("password")))
            {
                string lUsername = Context.Params.GET.IsSet<string>("username") ? Context.Params.GET.Get<string>("username") : Context.Params.POST.Get<string>("username");
                string lPassword = Context.Params.GET.IsSet<string>("password") ? Context.Params.GET.Get<string>("password") : Context.Params.POST.Get<string>("password");
                BooruUser bUser = Context.Server.UserManager.LoadUser(lUsername, lPassword);
                if (bUser != null)
                {
                    Cookie sCookie = Context.Server.CookieManager.GetCookie(bUser);
                    Context.InnerContext.Response.RedirectLocation = Context.InnerContext.Request.UrlReferrer.ToString();
                    Context.InnerContext.Response.SetCookie(sCookie);
                    Context.HTTPCode = 302;
                    Context.User = bUser;
                }
                else Context.HTTPCode = 403;
            }
            else Context.HTTPCode = 403;
        }
    }
}