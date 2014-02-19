using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using ArpanTECH;

namespace TA.SharpBooru.NetIO.Encryption
{
    public class RSA : IDisposable
    {
        private RSAx _RSAx;

        public RSA()
        {
            using (RSACryptoServiceProvider internalRSA = new RSACryptoServiceProvider(4096))
            {
                RSAParameters rsaParameters = internalRSA.ExportParameters(true);
                RSAxParameters rsaxParameters = new RSAxParameters(rsaParameters, 4096);
                _RSAx = new RSAx(rsaxParameters);
            }
        }

        public RSA(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs, Encoding.ASCII))
                _RSAx = new RSAx(reader.ReadToEnd(), 4096);
        }

        public RSA(byte[] Modulus, byte[] Exponent)
        {
            RSAxParameters rsaxParameters = new RSAxParameters(Modulus, Exponent, 4096);
            _RSAx = new RSAx(rsaxParameters);
        }

        public void Dispose() { _RSAx.Dispose(); }

        public void SaveKeys(string File) { _RSAx.Parameters.ToFile(File); }

        public void GetPublicKey(out byte[] Modulus, out byte[] Exponent)
        {
            Modulus = _RSAx.Parameters.N.ToByteArray();
            Exponent = _RSAx.Parameters.N.ToByteArray();
        }

        public string GetFingerprint()
        {
            byte[] modulus, exponent;
            GetPublicKey(out modulus, out exponent);
            byte[] temp = new byte[modulus.Length + exponent.Length];
            Array.Copy(modulus, temp, modulus.Length);
            Array.Copy(exponent, 0, temp, modulus.Length, exponent.Length);
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                temp = sha1.ComputeHash(temp);
            StringBuilder sb = new StringBuilder(20);
            for (byte i = 0; i < 5; i++)
                sb.AppendFormat("{0:X2}", (byte)(temp[i] ^ temp[i + 5] ^ temp[i + 10] ^ temp[i + 15]));
            return sb.ToString();
        }

        public byte[] EncryptPublic(byte[] Data) { return _RSAx.Encrypt(Data, false); }

        public byte[] DecryptPublic(byte[] Data) { return _RSAx.Decrypt(Data, false); }

        public byte[] EncryptPrivate(byte[] Data) { return _RSAx.Encrypt(Data, true); }

        public byte[] DecryptPrivate(byte[] Data) { return _RSAx.Decrypt(Data, true); }
    }
}