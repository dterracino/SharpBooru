using System.IO;

namespace TA.SharpBooru.Protocol
{
    public class Packet
    {
        public readonly ushort PacketID { get; set; }

        public Packet(ushort ID) { PacketID = ID; }

        public static Packet ReadPacket(BinaryReader Reader)
        {
            ushort _pID = Reader.ReadUInt16();
            switch (_pID)
            {
                case 1: return new Packet1_ConnectRequest(Reader);
                case 2: return new Packet2_Error(Reader);
            }
        }

        /*
         
C -> S		Packet1_ConnectRequest ( ProtoVersion )

Version stimmt:
 S -> C      Packet2_Error ( false, Success )
Version stimmt nicht:
 S -> C      Packet2_Error ( true, ProtocolVersionMismatch )

S -> C		Packet 3 - EncryptionRequest [ SupportedEncryptions [] ]
C -> S		Packet 3 - EncryptionResponse [ Encryption or empty if not supported ]

Disconnect wenn nicht supported:
S -> C		Packet 4 - ErrorPacket [ Boolean (DisconnectNow), String Error ]

S -> C		Packet 4 - Encryption 
 
        */
    }
}