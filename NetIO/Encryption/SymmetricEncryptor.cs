using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class SymmetricEncryptor : StreamEncryptor
    {
        private SymmetricAlgorithm sAlgorithm;
        private Stream stream;
        private List<ICryptoTransform> transforms;

        public SymmetricEncryptor(SymmetricAlgorithm Algorithm, Stream Stream) 
        {
            sAlgorithm = Algorithm;
            stream = Stream;
            transforms = new List<ICryptoTransform>();
        }

        public override Stream GetEncryptorStream()
        {
            ICryptoTransform transform = sAlgorithm.CreateEncryptor();
            transforms.Add(transform);
            return new CryptoStream(stream, transform, CryptoStreamMode.Write);
        }

        public override Stream GetDecryptorStream()
        {
            ICryptoTransform transform = sAlgorithm.CreateDecryptor();
            transforms.Add(transform);
            return new CryptoStream(stream, transform, CryptoStreamMode.Read);
        }

        public void Dispose()
        {
            sAlgorithm.Dispose();
            foreach (ICryptoTransform transform in transforms)
                transform.Dispose();
        }
    }
}