using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace TA.SharpBooru.Server.WebServer
{
    public class DictionaryEx : Dictionary<string, object>
    {
        public T Get<T>(string Key) { return (T)Convert.ChangeType(this[Key], typeof(T)); }

        public bool IsSet<T>(string Key)
        {
            if (base.ContainsKey(Key))
                if (base[Key] != null)
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                    if (conv != null)
                        return conv.IsValid(base[Key]);
                }
            return false;
        }
    }
}