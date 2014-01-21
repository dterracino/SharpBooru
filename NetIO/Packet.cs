using System;
using System.IO;
using System.Net;

namespace TA.SharpBooru.NetIO
{
    /*   Packet Layout
     *    _________________ ________________ _____________________ _______________
     *   |                 |                |                     |               |
     *   |  RequestID (4)  |  PacketID (2)  |  PayloadLength (4)  |  Payload (n)  |
     *   |_________________|________________|_____________________|_______________|
     * 
     *   RequestID is sent first. All values are LSB first (use ReaderWriter class).
     *   Every request has an answer, if no data is replied, use Packet0_Success.
     *   The request and answer have the same RequestID.
     */

    public abstract class Packet : IDisposable
    {
        public readonly uint RequestID = (uint)(Helper.Random.Next());

        public abstract ushort PacketID { get; }

        //public void FromBytes(byte[] Bytes) { FromStream(new MemoryStream(Bytes)); }
        public void FromStream(Stream Stream) { FromReader(new BinaryReader(Stream)); }
        public abstract void FromReader(BinaryReader Reader);

        /*
        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ToStream(ms);
                return ms.ToArray();
            }
        }
        */
        public void ToStream(Stream Stream) { ToWriter(new ReaderWriter(Stream)); }
        public abstract void ToWriter(ReaderWriter Writer);

        protected void WritePacketHeader(ReaderWriter Writer, uint PayloadLength)
        {
            Writer.Write(RequestID);
            Writer.Write(PacketID);
            Writer.Write(PayloadLength);
        }

        public abstract void Dispose();
    }
}