using System;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class CountCache<T> where T : class, IDisposable, ICloneable
    {
        private List<ulong> _Keys = new List<ulong>();
        private List<T> _Objs = new List<T>();
        private object _Lock = new object();

        public ushort MaxObjectCount = 0;

        public CountCache(ushort MaxObjectCount) { this.MaxObjectCount = MaxObjectCount; }

        public void Add(ulong ID, T Object)
        {
            lock (_Lock)
            {
                if (!_Keys.Contains(ID))
                {
                    _Keys.Add(ID);
                    _Objs.Add(Object.Clone() as T);
                    if (_Keys.Count > MaxObjectCount)
                    {
                        //maybe remove least used instead of oldest
                        _Keys.RemoveAt(0);
                        _Objs[0].Dispose();
                        _Objs.RemoveAt(0);
                    }
                }
            }
        }

        public void Remove(ulong ID)
        {
            lock (_Lock)
            {
                if (_Keys.Contains(ID))
                {
                    int index = _Keys.IndexOf(ID);
                    _Objs[index].Dispose();
                    _Objs.RemoveAt(index);
                    _Keys.Remove(ID);
                }
            }
        }

        public void Clear()
        {
            lock (_Lock)
            {
                _Keys.Clear();
                foreach (IDisposable idObj in _Objs)
                    idObj.Dispose();
                _Objs.Clear();
            }
        }

        public T this[ulong ID]
        {
            get
            {
                try
                {
                    lock (_Lock)
                        return _Objs[_Keys.IndexOf(ID)].Clone() as T;
                }
                catch { return null; }
            }
            set
            {
                Remove(ID);
                Add(ID, value);
            }
        }
    }
}