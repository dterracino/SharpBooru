using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TA.SharpBooru.NetIO
{
    /*   Packet Layout
     *    _________________ ________________ _____________________ _______________
     *   |                 |---------------------Packet class---------------------|
     *   |                 |                |                     |               |
     *   |  RequestID (4)  |  PacketID (2)  |  PayloadLength (4)  |  Payload (n)  |
     *   |_________________|________________|_____________________|_______________|
     * 
     *   RequestID is sent first. All values are LSB first (use ReaderWriter class).
     *   Every request has an answer, if no data is replied, use Packet0_Success.
     *   The request and answer have the same RequestID.
     *
     *   PacketID's < 16 are reserved for internal protocol use.
     */

    public abstract class Packet : IDisposable
    {
        public abstract ushort PacketID { get; }

        protected abstract void ToWriter(ReaderWriter Writer);
        protected abstract void FromReader(ReaderWriter Reader);

        /// <summary>Write the packet to a stream</summary>
        /// <param name="Stream">The stream</param>
        /// <param name="RequestID">The RequestID to send, use 0 for a random ID</param>
        /// <returns>The RequestID to send, or null for a random ID</returns>
        public uint ToStream(Stream Stream, uint RequestID = 0)
        {
            uint __requestID = RequestID < 1 ? (uint)Helper.Random.Next() : RequestID;
            using (ReaderWriter packetWriter = new ReaderWriter(Stream))
            {
                packetWriter.Write(__requestID);
                packetWriter.Write(PacketID);
                using (MemoryStream bodyStream = new MemoryStream())
                {
                    using (ReaderWriter bodyWriter = new ReaderWriter(bodyStream))
                        ToWriter(bodyWriter);
                    packetWriter.Write(bodyStream.ToArray(), true);
                }
            }
            return __requestID;
        }

        public abstract void Dispose();
    }
}