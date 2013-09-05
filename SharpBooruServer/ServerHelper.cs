using System;
using System.IO;
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

        public static void SetUID(string Username)
        {
            if (Username == null)
                throw new ArgumentNullException("Username");
            else if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username");
            else if (!Helper.IsUnix())
                throw new PlatformNotSupportedException("Not running Unix");
            else if (Syscall.getuid() != 0)
                throw new SecurityException("Not running as root");
            else
            {
                Passwd passwordStruct = Syscall.getpwnam(Username);
                if (passwordStruct == null)
                    throw new UnixIOException(string.Format("User {0} not found", Username));
                else if (Syscall.setuid(passwordStruct.pw_uid) != 0)
                    throw new Exception("SetUID failed");
            }
        }

        public static void ChOwn(string Path, string Username)
        {
            if (Path == null)
                throw new ArgumentNullException("Path");
            else if (!File.Exists(Path))
                throw new FileNotFoundException(Path);
            else if (Username == null)
                throw new ArgumentNullException("Username");
            else if (string.IsNullOrWhiteSpace("Username"))
                throw new ArgumentException("Username");
            else if (!Helper.IsUnix())
                throw new PlatformNotSupportedException("Not running Unix");
            else if (Syscall.getuid() != 0)
                throw new SecurityException("Not running as root");
            else
            {
                Passwd passwordStruct = Syscall.getpwnam(Username);
                if (passwordStruct == null)
                    throw new UnixIOException(string.Format("User {0} not found", Username));
                else if (Syscall.chown(Path, passwordStruct.pw_uid, passwordStruct.pw_gid) != 0)
                    throw new Exception("ChOwn failed");
            }
        }
    }
}