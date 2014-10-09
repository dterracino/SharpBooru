using System;
using System.Threading;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class SThreadPool : IDisposable
    {
        private Thread[] _Threads;
        private AutoResetEvent _Event;
        private Queue<Action> _Queue;
        private bool _IsRunning;

        public bool IsRunning { get { return _IsRunning; } }

        public SThreadPool(ushort ThreadCount)
        {
            _IsRunning = true;
            _Threads = new Thread[ThreadCount];
            _Event = new AutoResetEvent(false);
            _Queue = new Queue<Action>(128);
            for (ushort i = 0; i < ThreadCount; i++)
            {
                _Threads[i] = new Thread(_ThreadMethod);
                _Threads[i].Name = "SThreadPool";
                _Threads[i].Start();
            }
        }

        public void Queue(Action Action)
        {
            if (Action == null)
                throw new ArgumentNullException("Action");
            if (!_IsRunning)
                throw new ArgumentException("ThreadPool isn't running anymore");
            lock (_Queue)
            {
                _Queue.Enqueue(Action);
                if (_Queue.Count < 2)
                    _Event.Set();
            }
        }

        public void Stop()
        {
            if (_IsRunning)
            {
                _IsRunning = false;
                lock (_Queue)
                    _Queue.Clear();
            }
        }

        public void Dispose()
        {
            Stop();
            _Threads = null;
            _Event.Dispose();
        }

        private void _ThreadMethod()
        {
            while (_IsRunning)
            {
                Action currentAction = null;
                lock (_Queue)
                {
                    if (_Queue.Count > 0)
                        currentAction = _Queue.Dequeue();
                    else currentAction = null;
                }
                if (currentAction == null)
                    _Event.WaitOne();
                else try { currentAction(); }
                    catch { }
            }
        }
    }
}
