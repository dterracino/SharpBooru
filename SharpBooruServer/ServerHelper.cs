using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace TA.SharpBooru.Server
{
    public class ServerHelper
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
    }
}
