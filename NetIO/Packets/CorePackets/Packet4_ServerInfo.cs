using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace TA.SharpBooru.NetIO.Packets.CorePackets
{
    public class Packet4_ServerInfo : Packet
    {
        public string ServerName { get; set; }
        public string AdminContact { get; set; }
        public byte[] Modulus { get; set; }
        public byte[] Exponent { get; set; }
        public bool Encryption { get; set; }

        public override ushort PacketID { get { return 4; } }

        protected override void ToWriter(ReaderWriter Writer)
        {
            Writer.Write(ServerName, true);
            Writer.Write(AdminContact, true);
            Writer.Write(Encryption);
            Writer.Write(Modulus, true);
            Writer.Write(Exponent, true);
        }

        protected override void FromReader(ReaderWriter Reader)
        {
            ServerName = Reader.ReadString();
            AdminContact = Reader.ReadString();
            Encryption = Reader.ReadBool();
            Modulus = Reader.ReadBytes();
            Exponent = Reader.ReadBytes();
        }

        public override void Dispose() { }
    }
}
