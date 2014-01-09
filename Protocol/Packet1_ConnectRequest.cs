using System.IO;

namespace TA.SharpBooru.Protocol
{
    public class Packet1_ConnectRequest : Packet
    {
        public ushort ProtocolVersion;

        public Packet1_ConnectRequest()
            : base(1) { }

        public Packet1_ConnectRequest(BinaryReader Reader)
            : base(1)
        {
            ProtocolVersion = Reader.ReadUInt16();
        }
    }
}