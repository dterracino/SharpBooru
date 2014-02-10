using System;
using System.IO;
using TA.SharpBooru.NetIO.Packets.CorePackets;
using TA.SharpBooru.NetIO.Packets.BooruPackets;

namespace TA.SharpBooru.NetIO.Packets
{
    /*   --- Packet Layout ---
     * 
     *                      <-------------------C# Packet class------------------>
     *                                       <-------------Packet body----------->
     *    _________________ ________________ _____________________ _______________
     *   |                 |                |                     |               |
     *   |  RequestID (4)  |  PacketID (2)  |  PayloadLength (4)  |  Payload (n)  |
     *   |_________________|________________|_____________________|_______________|
     * 
     *   RequestID is sent first. All values are LSB first (use ReaderWriter class).
     *   Every request has an answer, if no data is replied, use Packet0_Success.
     *   The request and answer have the same RequestID.
     *
     *   PacketID's < 16 are reserved for internal protocol use.
     *   
     *   --- Protocol ---
     *   
     *    [ TCP Handshake ]
     *   S -> C    ProtocolVersion (ushort)
     *   C -> S    ProtocolSupported (bool)
     *    [ Switching to packet based communication ]
     *   S -> C    Packet3_Disconnect
     *    OR
     *   S -> C    Packet4_ServerInfo
     *    [ Client checks pubKey fingerprint ]
     *    [ Client generates random key ]
     *   C -> S    Packet5_Encryption
     *    OR
     *   C -> S    Packet3_Disconnect
     *    [ Server generates random and XOR's it with the key ]
     *   S -> C    Packet5_Encryption
     *    [ Switch to AES ]
     *    
     */

    public abstract class Packet : IDisposable
    {
        public abstract ushort PacketID { get; }

        protected abstract void ToWriter(ReaderWriter Writer);
        protected abstract void FromReader(ReaderWriter Reader);

        public abstract void Dispose();

        public void PacketToWriter(ReaderWriter Writer)
        {
            Writer.Write(PacketID);
            using (MemoryStream bodyStream = new MemoryStream())
            {
                using (ReaderWriter bodyWriter = new ReaderWriter(bodyStream))
                    ToWriter(bodyWriter);
                Writer.Write(bodyStream.ToArray(), true);
            }
        }

        private Packet FromReaderReturn(ReaderWriter Reader)
        {
            FromReader(Reader);
            return this;
        }

        public static Packet PacketFromReader(ReaderWriter Reader)
        {
            ushort packetID = Reader.ReadUShort();
            switch (packetID)
            {
                case 0: return (new Packet0_Success()).FromReaderReturn(Reader);
                case 1: return (new Packet1_Exception()).FromReaderReturn(Reader);
                case 2: return (new Packet2_IsAlive()).FromReaderReturn(Reader);
                case 3: return (new Packet3_Disconnect()).FromReaderReturn(Reader);
                case 4: return (new Packet4_ServerInfo()).FromReaderReturn(Reader);
                case 5: return (new Packet5_Encryption()).FromReaderReturn(Reader);

                case 17: return (new Packet17_GetResource()).FromReaderReturn(Reader);
                case 18: return (new Packet18_GetAllTags()).FromReaderReturn(Reader);
                case 19: return (new Packet19_DeleteResource()).FromReaderReturn(Reader);
                case 20: return (new Packet20_EditResource()).FromReaderReturn(Reader);
                case 21: return (new Packet21_AddResource()).FromReaderReturn(Reader);
                case 22: return (new Packet22_Search()).FromReaderReturn(Reader);
                case 23: return (new Packet23_Resource()).FromReaderReturn(Reader);
                case 24: return (new Packet24_StringList()).FromReaderReturn(Reader);
                case 25: return (new Packet25_ULongList()).FromReaderReturn(Reader);
            }
            return null;
        }
    }
}