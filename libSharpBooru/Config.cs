using System;
using System.IO;
using System.Collections.Generic;

namespace TA.SharpBooru
{
    public class Config
    {
        private Dictionary<string, string> _Config;

        public Config(string Path)
        {
            string[] lines = File.ReadAllLines(Path);
            _Config = new Dictionary<string, string>();

            foreach (string line in lines)
                if (!string.IsNullOrWhiteSpace(line))
                    if (line.Contains("="))
                    {
                        int indexOf = line.IndexOf('=');
                        string key = line.Substring(0, indexOf);
                        string value = line.Substring(indexOf + 1);
                        _Config.Add(key, value);
                    }
        }

        public T Get<T>(string Key)
        {
            string conf = _Config[Key];
            return (T)Convert.ChangeType(conf, typeof(T));
        }
    }
}
