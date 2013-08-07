using System;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace TEAM_ALPHA.SQLBooru.Server.VirtualFileSystem
{
    public class VFSAdminPanelFile : VFSFile
    {
        private class AdminPanelCommand
        {
            public string Name;
            public Dictionary<string, Type> Parameters;
            public Func<Dictionary<string, object>, Context, bool> Action;
        }

        private List<AdminPanelCommand> CommandList = new List<AdminPanelCommand>();

        public VFSAdminPanelFile(string Name)
        {
            this.Name = Name;
            CommandList.Add(new AdminPanelCommand()
            {
                Name = "ChangeProp",
                Parameters = new Dictionary<string, Type>()
                    {
                        { "Property", typeof(string) },
                        { "NewValue", typeof(string) }
                    },
                Action = (p, c) =>
                    {
                        c.Booru.UpdateProperty((Booru.Property)Enum.Parse(typeof(Booru.Property), (string)p["Property"]), p["NewValue"]);
                        return true;
                    }
            });
            CommandList.Add(new AdminPanelCommand()
            {
                Name = "UserAdd",
                Parameters = new Dictionary<string, Type>()
                    {
                        { "Username", typeof(string) },
                        { "Password", typeof(string) }
                    },
                Action = (p, c) =>
                    {
                        if (!c.Server.UserManager.Exists((string)p["Username"]))
                        {
                            c.Server.UserManager.AddUser((string)p["Username"], (string)p["Password"]);
                            return true;
                        }
                        else return false;
                    }
            });
            /*
            CommandList.Add(new AdminPanelCommand()
            {
                Name = "UserEditPerm",
                Parameters = new Dictionary<string, Type>()
                    {
                        { "Username", typeof(string) },
                        { "Permission", typeof(string) },
                        { "NewValue", typeof(bool) }
                    },
                Action = (p, c) =>
                    {
                        BooruUser user = c.Server.UserManager.Load((string)p["Username"], null, true);
                        if (user != null)
                        {
                            bool permSet = user.Perm_Set((string)p["Permission"], (bool)p["NewValue"]);
                            if (permSet)
                                c.Server.UserManager.Save(user);
                            return permSet;
                        }
                        else return false;
                    }
            });
            */
            CommandList.Add(new AdminPanelCommand()
            {
                Name = "EmergencyKill",
                Parameters = new Dictionary<string, Type>(),
                Action = (p, c) =>
                    {
                        Process.GetCurrentProcess().Kill();
                        return true;
                    }
            });
        }

        public override void Execute(Context Context)
        {
            if (Context.User.Perm_Admin)
            {
                if (Context.Params.GET.IsSet<string>("method"))
                {
                    foreach (AdminPanelCommand method in CommandList)
                        if (method.Name.ToLower() == Context.Params.GET.Get<string>("method"))
                        {
                            Dictionary<string, object> methodParams = new Dictionary<string, object>();
                            if (Context.Params.GET.Count - 1 != method.Parameters.Count)
                            {
                                Context.HTTPCode = 500;
                                return;
                            }
                            else
                            {
                                foreach (KeyValuePair<string, Type> paramDecl in method.Parameters)
                                {
                                    string paramName = string.Format("arg_{0}", paramDecl.Key.ToLower());
                                    if (Context.Params.GET.IsSet<string>(paramName))
                                    {
                                        string paramValue = Context.Params.GET.Get<string>(paramName);
                                        TypeConverter conv = TypeDescriptor.GetConverter(paramDecl.Value);
                                        if (conv != null)
                                            if (conv.IsValid(paramValue))
                                            {
                                                methodParams.Add(paramDecl.Key, Convert.ChangeType(paramValue, paramDecl.Value));
                                                continue;
                                            }
                                    }
                                    Context.HTTPCode = 500;
                                    return;
                                }
                                bool methodResult = method.Action(methodParams, Context);
                                ServerHelper.WriteHeader(Context, string.Format("Admin command {0}", method.Name));
                                Context.OutWriter.Write("Command execution {0}", methodResult ? "successfull" : "failed");
                                ServerHelper.WriteBackButton(Context, true);
                                ServerHelper.WriteFooter(Context);
                                return;
                            }
                        }
                    Context.HTTPCode = 404;
                }
                else
                {
                    ServerHelper.WriteHeader(Context, "Admin Panel");
                    Context.OutWriter.Write("<b>Admin Panel</b><br><br>");
                    foreach (AdminPanelCommand method in CommandList)
                    {
                        Context.OutWriter.Write(method.Name);
                        Context.OutWriter.Write("<form method=\"GET\" action=\"\"><table>");
                        Context.OutWriter.Write("<input type=\"hidden\" name=\"method\" value=\"{0}\">", method.Name.ToLower());
                        foreach (KeyValuePair<string, Type> paramDecl in method.Parameters)
                            Context.OutWriter.Write("<tr><td>{0}</td><td> <input type=\"text\" name=\"arg_{1}\"></td></tr>", paramDecl.Key, paramDecl.Key.ToLower());
                        Context.OutWriter.Write("<tr><td></td><td style=\"text-align:right\"><input type=\"submit\" value=\"OK\"></td></tr>");
                        Context.OutWriter.Write("</table></form>");
                        if (!method.Equals(CommandList.Last()))
                            Context.OutWriter.Write("<br><br>");
                    }
                    ServerHelper.WriteFooter(Context);
                }
            }
            else Context.HTTPCode = 403;
        }
    }
}