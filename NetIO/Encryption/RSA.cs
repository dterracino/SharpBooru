using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class RSA : IDisposable
    {
        private bool _Private = false;
        private RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider(4096);

        public RSA() { }
        public RSA(string File) { LoadKeys(File); }
        public RSA(byte[] Modulus, byte[] Exponent) { SetPublicKey(Modulus, Exponent); }

        public void Dispose() { _RSA.Dispose(); }

        public void SaveKeys(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (ReaderWriter writer = new ReaderWriter(fs))
            {
                writer.Write(_Private);
                RSAParameters rsaParams = _RSA.ExportParameters(_Private);
                writer.Write(rsaParams.Modulus, true);
                writer.Write(rsaParams.Exponent, true);
                if (_Private)
                    writer.Write(rsaParams.D, true);
            }
        }

        public void LoadKeys(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ReaderWriter reader = new ReaderWriter(fs))
            {
                RSAParameters rsaParams = new RSAParameters();
                _Private = reader.ReadBool();
                rsaParams.Modulus = reader.ReadBytes();
                rsaParams.Exponent = reader.ReadBytes();
                if (_Private)
                    rsaParams.D = reader.ReadBytes();
                _RSA.ImportParameters(rsaParams);
            }
        }

        public void GetPublicKey(out byte[] Modulus, out byte[] Exponent)
        {
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            Modulus = rsaParams.Modulus;
            Exponent = rsaParams.Exponent;
        }

        public void SetPublicKey(byte[] Modulus, byte[] Exponent)
        {
            RSAParameters rsaParams = new RSAParameters()
            {
                Modulus = Modulus,
                Exponent = Exponent
            };
            _RSA.ImportParameters(rsaParams);
        }

        public string GetFingerprint()
        {
            RSAParameters rsaParams = _RSA.ExportParameters(false);
            byte[] pubkey = new byte[rsaParams.Modulus.Length + rsaParams.Exponent.Length];
            Array.Copy(rsaParams.Modulus, pubkey, rsaParams.Modulus.Length);
            Array.Copy(rsaParams.Exponent, 0, pubkey, rsaParams.Modulus.Length, rsaParams.Exponent.Length);
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                pubkey = sha1.ComputeHash(pubkey);
            StringBuilder sb = new StringBuilder(22);
            sb.Append("0x");
            for (byte i = 0; i < 5; i++)
                sb.Append(((byte)(pubkey[i] ^ pubkey[i + 5] ^ pubkey[i + 10] ^ pubkey[i + 15])).ToString("X2"));
            return sb.ToString();
        }

        public byte[] EncryptPublic(byte[] Data) { return _RSA.Encrypt(Data, true); }

        public byte[] DecryptPrivate(byte[] Data) { return _RSA.Decrypt(Data, true); }
    }
}