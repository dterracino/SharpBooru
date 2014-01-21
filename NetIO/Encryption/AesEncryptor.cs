using System.IO;
using System.Security.Cryptography;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class AesEncryptor : SymmetricEncryptor
    {
        public AesEncryptor(Stream Stream)
            : base(new AesManaged(), Stream) { }
    }
}