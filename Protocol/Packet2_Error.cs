using System.IO;

namespace TA.SharpBooru.Protocol
{
    public class Packet2_Error : Packet
    {
        public enum ErrorCodes : ushort
        {
            Success = 0x0000,
            ProtocolVersionMismatch = 0x0001
        }

        public bool DisconnectNow;
        public ushort ErrorCode;

        public Packet2_Error()
            : base(2) { }

        public Packet2_Error(BinaryReader Reader)
            : base(2)
        {
            DisconnectNow = Reader.ReadBoolean();
            ErrorCode = Reader.ReadUInt16();
        }
    }
}