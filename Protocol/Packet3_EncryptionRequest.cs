using System.IO;

namespace TA.SharpBooru.Protocol
{
    public class Packet3_EncryptionRequest : Packet
    {
        public enum Encryptions : byte
        {
            Unencrypted = 0x0000,
            AES = 0x0001
        }

        public Encryptions Encryption;

        public Packet3_EncryptionRequest()
            : base(3) { }

        public Packet3_EncryptionRequest(BinaryReader Reader)
            : base(3)
        {
            Encryption
        }
    }
}