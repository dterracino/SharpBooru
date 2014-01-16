using System;
using System.IO;

namespace TA.SharpBooru.NetIO
{
   public class Packet1_Exception:Packet
   {
       public Exception Exception{get;set;}

       public override ushort PacketID { get { return 1; } }
       public override uint PayloadLength { get { return (uint)Exception.Message.Length * 2; } }

        public override void FromReader(BinaryReader Reader)
        {

        }

        public override void ToWriter(BinaryWriter Writer)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
