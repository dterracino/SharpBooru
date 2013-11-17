using System;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class WeakCache<T> where T : class
    {
        private Dictionary<ulong, WeakReference> _Dict = new Dictionary<ulong, WeakReference>();
        private object _Lock = new object();

        public void Add(ulong ID, T Object)
        {
            lock (_Lock)
                _Dict.Add(ID, new WeakReference(Object));
        }

        public void Remove(ulong ID)
        {
            lock (_Lock)
                _Dict.Remove(ID);
        }

        public T this[ulong ID]
        {
            get
            {
                lock (_Lock)
                {
                    if (_Dict.ContainsKey(ID))
                    {
                        T wObj = _Dict[ID].Target as T;
                        if (wObj != null)
                            return wObj;
                        else _Dict.Remove(ID);
                    }
                    return null;
                }
            }
            set
            {
                Remove(ID);
                Add(ID, value);
            }
        }
    }
}