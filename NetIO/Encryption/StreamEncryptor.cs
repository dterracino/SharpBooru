using System;
using System.IO;

namespace TA.SharpBooru.NetIO.Encryption
{
    public abstract class StreamEncryptor : IDisposable
    {
        public abstract Stream GetEncryptorStream();
        public abstract Stream GetDecryptorStream();
    }
}