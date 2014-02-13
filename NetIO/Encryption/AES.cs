using System;
using System.IO;
using System.Security.Cryptography;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class AES:IDisposable
    {
        private AesCryptoServiceProvider _AES;

        public AES()
        {
            _AES = new AesCryptoServiceProvider();
            _AES.KeySize = 256;
            _AES.Mode = CipherMode.ECB;
            _AES.GenerateKey();
        }

        public AES(byte[] Key)
            : this() 
        { this.Key = Key; }

        public void Dispose() { _AES.Dispose(); }

        public byte[] Key
        {
            get { return _AES.Key; }
            set { _AES.Key = value; }
        }

        public CryptoStream CreateEncryptorStream(Stream InnerStream)
        {
            ICryptoTransform transform = _AES.CreateEncryptor();
            return new CryptoStream(InnerStream, transform, CryptoStreamMode.Write);
        }

        public CryptoStream CreateDecryptorStream(Stream InnerStream)
        {
            ICryptoTransform transform = _AES.CreateDecryptor();
            return new CryptoStream(InnerStream, transform, CryptoStreamMode.Read);
        }
    }
}