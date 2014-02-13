using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class RSA : IDisposable
    {
        private bool _Private;
        private RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider(4096);

        public RSA() { _Private = true; }
        public RSA(string File) { LoadKeys(File); }
        public RSA(byte[] Modulus, byte[] Exponent) { SetPublicKey(Modulus, Exponent); }

        public void Dispose() { _RSA.Dispose(); }

        public void SaveKeys(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(fs, Encoding.ASCII))
                writer.WriteLine(_RSA.ToXmlString(_Private).Replace("><", ">" + Environment.NewLine + "<"));
        }

        public void LoadKeys(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs, Encoding.ASCII))
                _RSA.FromXmlString(reader.ReadToEnd());
            _Private = !_RSA.PublicOnly;
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
            _Private = false;
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

        public byte[] EncryptPublic(byte[] Data) { return _RSA.Encrypt(Data, false); }

        public byte[] DecryptPrivate(byte[] Data)
        {
            if (_Private)
                return _RSA.Decrypt(Data, false);
            else throw new Exception("No private key");
        }
    }
}