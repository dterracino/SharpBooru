using System;
using System.Security;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public static class ServerHelper
    {
        public static void SetupSignal(Signum Signal, Action SignalAction)
        {
            Thread signalThread = new Thread(() =>
                {
                    using (UnixSignal uSignal = new UnixSignal(Signal))
                        uSignal.WaitOne(Timeout.Infinite);
                    SignalAction();
                }) { IsBackground = true };
            signalThread.Start();
        }

        public static void SetUID(string UserName)
        {
            if (UserName == null)
                throw new ArgumentNullException("UserName");
            else if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentException("UserName");
            else if (!Helper.IsUnix())
                throw new PlatformNotSupportedException("Not running Unix");
            else if (Syscall.getuid() != 0)
                throw new SecurityException("Not running as root");
            else
            {
                Passwd passwordStruct = Syscall.getpwnam(UserName);
                if (passwordStruct == null)
                    throw new UnixIOException(string.Format("User {0} not found", UserName));
                else if (Syscall.setuid(passwordStruct.pw_uid) != 0)
                    throw new Exception("SetUID failed");
            }
        }
    }
}