﻿using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public static class SyscallEx
    {
        [DllImport("libc", EntryPoint = "setpriority")]
        private static extern int _setpriority(int which, int who, int prio);

        private static void CheckRoot()
        {
            if (Syscall.getuid() != 0)
                throw new SecurityException("Not running as root");
        }

        public static void setuid(string Username)
        {
            CheckRoot();
            if (Username == null)
                throw new ArgumentNullException("Username");
            else if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username");
            else
            {
                Passwd passwordStruct = Syscall.getpwnam(Username);
                if (passwordStruct == null)
                    throw new UnixIOException(string.Format("User {0} not found", Username));
                else if (Syscall.setuid(passwordStruct.pw_uid) != 0)
                    throw new Exception("setuid failed");
            }
        }

        public static void chown(string Path, string Username)
        {
            CheckRoot();
            //if (!File.Exists(Path))
            //    throw new FileNotFoundException(Path);
            if (Username == null)
                throw new ArgumentNullException("Username");
            else if (string.IsNullOrWhiteSpace("Username"))
                throw new ArgumentException("Username");
            else
            {
                Passwd passwordStruct = Syscall.getpwnam(Username);
                if (passwordStruct == null)
                    throw new UnixIOException(string.Format("User {0} not found", Username));
                else if (Syscall.chown(Path, passwordStruct.pw_uid, passwordStruct.pw_gid) != 0)
                    throw new Exception("chown failed");
            }
        }

        public static void chmod(string Path, FilePermissions Mode)
        {
            CheckRoot();
            if (Syscall.chmod(Path, Mode) != 0)
                throw new Exception("chmod failed");
        }

        public static void setpriority(int PID, int Priority)
        {
            if (_setpriority(0, PID, Priority) != 0)
                throw new Exception("setpriority failed");
        }
    }
}
