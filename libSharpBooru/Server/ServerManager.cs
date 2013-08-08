using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amib.Threading;

namespace TA.SharpBooru.Server
{
    public class ServerManager
    {
        private bool _Running;
        private Logger _Logger;
        private List<Server> _ServerList;
        private List<Thread> _ThreadList;

        public ServerManager(Logger Logger, List<Server> ServerList)
        {
            _Running = false;
            _Logger = Logger;
            _ServerList = ServerList;
            _ThreadList = new List<Thread>();
        }

        public void Start()
        {
            if (_Running)
                throw new Exception("Servers already running");

            for (int i = 0; i < _ServerList.Count; i++)
            {
                if (_Logger != null)
                    _Logger.LogLine("Starting {0}...", _ServerList[i].ServerName);
                try
                {
                    _ServerList[i].Start();

                }
                catch (Exception ex)
                {
                    _Logger.LogException(_ServerList[i].ServerName, ex);
                    _ThreadList[i] = null;
                }
            }
        }

        public void Stop()
        {
            if (!_Running)
                throw new Exception("Servers not running");

            foreach (Server srv in _ServerList)
            {
                if (_Logger != null)
                    _Logger.LogLine("Stopping {0}...", srv.ServerName);
                try { srv.Stop(); }
                catch (Exception ex) { _Logger.LogException(srv.ServerName, ex); }
            }
        }
    }
}
