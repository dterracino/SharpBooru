using System;
using System.IO;
using LitJson;

namespace TA.SharpBooru.Client.WebServer
{
    [Serializable]
    public class ServerConfig
    {
        public string ListenerPrefix = "http://*:80/";
        public string User = null;
        public bool EnableRSM = true;
        public MySQLWrapper.DBConnectionInfo ConnectionInfo;

        public ServerConfig() { ConnectionInfo = new MySQLWrapper.DBConnectionInfo(); }

        public static ServerConfig LoadFromFile(string ConfigFile) { return JsonMapper.ToObject<ServerConfig>(File.ReadAllText(ConfigFile)); }

        public void SaveToFile(string ConfigFile) { File.WriteAllText(ConfigFile, JsonMapper.ToJson(this)); }
    }
}