using System;
using System.Collections.Generic;
using ProtoBuf;

namespace TA.SharpBooru
{
    [ProtoContract]
    public class BooruInfo
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public string Creator;
        [ProtoMember(3)]
        public string Description;
        [ProtoMember(4)]
        public Dictionary<string, string> Configuration;
    }
}
