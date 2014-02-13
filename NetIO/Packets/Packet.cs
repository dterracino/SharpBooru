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
     *   RequestID is sent first. Every request has an answer, if no data is replied, use Packet0_Success.
     *   The request and answer have the same RequestID.
     *
     *   PacketID's < 16 are reserved for internal protocol use.
     *   
     */

    public abstract class Packet : IDisposable
    {
        public abstract ushort PacketID { get; }

        protected abstract void ToWriter(ReaderWriter Writer);
        protected abstract void FromReader(ReaderWriter Reader);

        public abstract void Dispose();

        public void PacketToWriter(ReaderWriter Writer, bool Flush = true)
        {
            Writer.Write(PacketID);
            using (MemoryStream bodyStream = new MemoryStream())
            {
                using (ReaderWriter bodyWriter = new ReaderWriter(bodyStream))
                    ToWriter(bodyWriter);
                Writer.Write(bodyStream.ToArray(), true);
            }
            if (Flush)
                Writer.Flush();
        }

        private Packet FromReaderReturn(ReaderWriter Reader)
        {
            FromReader(Reader);
            return this;
        }

        public static Packet PacketFromReader(ReaderWriter Reader)
        {
            ushort packetID = Reader.ReadUShort();
            using (MemoryStream bodyStream = new MemoryStream(Reader.ReadBytes()))
            using (ReaderWriter bodyReader = new ReaderWriter(bodyStream))
                switch (packetID)
                {
                    //CorePackets
                    case 0: return (new Packet0_Success()).FromReaderReturn(bodyReader);
                    case 1: return (new Packet1_Exception()).FromReaderReturn(bodyReader);
                    case 2: return (new Packet2_IsAlive()).FromReaderReturn(bodyReader);
                    case 3: return (new Packet3_Disconnect()).FromReaderReturn(bodyReader);
                    case 4: return (new Packet4_ServerInfo()).FromReaderReturn(bodyReader);
                    case 5: return (new Packet5_Encryption()).FromReaderReturn(bodyReader);

                    //BooruPackets
                    case 16: return (new Packet16_Login()).FromReaderReturn(bodyReader);
                    case 17: return (new Packet17_GetResource()).FromReaderReturn(bodyReader);
                    case 18: return (new Packet18_GetAllTags()).FromReaderReturn(bodyReader);
                    case 19: return (new Packet19_DeleteResource()).FromReaderReturn(bodyReader);
                    case 20: return (new Packet20_EditResource()).FromReaderReturn(bodyReader);
                    case 21: return (new Packet21_AddResource()).FromReaderReturn(bodyReader);
                    case 22: return (new Packet22_Search()).FromReaderReturn(bodyReader);
                    case 23: return (new Packet23_Resource()).FromReaderReturn(bodyReader);
                    case 24: return (new Packet24_StringList()).FromReaderReturn(bodyReader);
                    case 25: return (new Packet25_ULongList()).FromReaderReturn(bodyReader);
                    case 26: return (new Packet26_SearchImg()).FromReaderReturn(bodyReader);
                    case 27: return (new Packet27_AddAlias()).FromReaderReturn(bodyReader);
                }
            return null;
        }
    }
}