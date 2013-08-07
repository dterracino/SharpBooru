using System;
using System.Net;
using System.Collections.Generic;

namespace TEAM_ALPHA.SQLBooru.Server
{
    public class RequestSecurityManager
    {
        private List<IPAddress> _BlackList = new List<IPAddress>();
        private Dictionary<IPAddress, int> _OpenConnectionCounter = new Dictionary<IPAddress, int>();
        private Dictionary<IPAddress, List<DateTime>> _OverTimeCounter = new Dictionary<IPAddress, List<DateTime>>();
        private int _TotalOpenConnections = 0;

        private object _Lock = new object();
        private int _MaxOpenConnections = 16;
        private int _MaxConnectionsOverTime = 600;
        private int _OverTimeLifeTime = 60 * 1000;
        private int _MaxTotalOpenConnections = 100;

        public int MaxOpenConnections
        {
            get { return _MaxOpenConnections; }
            set
            {
                if (value > 1)
                    _MaxOpenConnections = value;
                else _MaxOpenConnections = 1;
            }
        }
        public int MaxConnectionsOverTime
        {
            get { return _MaxConnectionsOverTime; }
            set
            {
                if (value > 1)
                    _MaxConnectionsOverTime = value;
                else _MaxConnectionsOverTime = 1;
            }
        }
        public int OverTimeLifeTime
        {
            get { return _OverTimeLifeTime; }
            set
            {
                if (value > 1)
                    _OverTimeLifeTime = value;
                else _OverTimeLifeTime = 1;
            }
        }
        public int MaxTotalOpenConnections
        {
            get { return _MaxTotalOpenConnections; }
            set
            {
                if (value > 1)
                    _MaxTotalOpenConnections = value;
                else _MaxTotalOpenConnections = 1;
            }
        }
        public int TotalOpenConnections { get { return _TotalOpenConnections; } }
        public List<IPAddress> BlackList { get { return _BlackList; } }

        public RequestSecurityManager() { }
        public RequestSecurityManager(int MaxOpenConnections, int MaxTotalOpenConnections, int MaxConnectionsOverTime, int OverTimeLifeTime)
            : this()
        {
            this.MaxOpenConnections = MaxOpenConnections;
            this.MaxTotalOpenConnections = MaxTotalOpenConnections;
            this.MaxConnectionsOverTime = MaxConnectionsOverTime;
            this.OverTimeLifeTime = OverTimeLifeTime;
        }

        private void RefreshOverTimeCounter()
        {
            IPAddress[] ips = new IPAddress[_OverTimeCounter.Keys.Count];
            _OverTimeCounter.Keys.CopyTo(ips, 0);
            for (int i = ips.Length - 1; !(i < 0); i--)
            {
                for (int d = _OverTimeCounter[ips[i]].Count - 1; !(d < 0); d--)
                {
                    TimeSpan diff = DateTime.UtcNow - _OverTimeCounter[ips[i]][d];
                    if (diff.TotalMilliseconds > _OverTimeLifeTime)
                        _OverTimeCounter[ips[i]].RemoveAt(d);
                }
                if (_OverTimeCounter[ips[i]].Count < 1)
                    _OverTimeCounter.Remove(ips[i]);
            }
        }

        private int GetCode(IPAddress IP)
        {
            if (_BlackList.Contains(IP))
                return 403;
            if (_TotalOpenConnections > _MaxTotalOpenConnections)
                return 509;
            if (_OpenConnectionCounter.ContainsKey(IP))
                if (_OpenConnectionCounter[IP] > _MaxOpenConnections)
                    return 421;
            RefreshOverTimeCounter();
            if (_OverTimeCounter.ContainsKey(IP))
                if (_OverTimeCounter[IP].Count > _MaxConnectionsOverTime)
                    return 429;
            return 200;
        }

        public bool CheckAllowedConnection(IPAddress IP) { return CheckAllowedConnectionAndReturnHttpCode(IP) == 200; }
        public int CheckAllowedConnectionAndReturnHttpCode(IPAddress IP)
        {
            lock (_Lock)
            {
                int code = GetCode(IP);
                _TotalOpenConnections++;
                if (_OpenConnectionCounter.ContainsKey(IP))
                    _OpenConnectionCounter[IP]++;
                else _OpenConnectionCounter.Add(IP, 1);
                if (_OverTimeCounter.ContainsKey(IP))
                    _OverTimeCounter[IP].Add(DateTime.UtcNow);
                else
                {
                    List<DateTime> dateTimes = new List<DateTime>();
                    dateTimes.Add(DateTime.UtcNow);
                    _OverTimeCounter.Add(IP, dateTimes);
                }
                return code;
            }
        }

        public void RequestProcessed(IPAddress IP)
        {
            lock (_Lock)
            {
                if (_OpenConnectionCounter.ContainsKey(IP))
                {
                    _OpenConnectionCounter[IP]--;
                    if (_OpenConnectionCounter[IP] < 1)
                        _OpenConnectionCounter.Remove(IP);
                }
                if (_TotalOpenConnections > 0)
                    _TotalOpenConnections--;
            }
        }
    }
}