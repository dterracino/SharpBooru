// @Date : 15th July 2012
// @Author : Arpan Jati (arpan4017@yahoo.com; arpan4017@gmail.com)
// @Library : ArpanTECH.RSAx
// @CodeProject: http://www.codeproject.com/Articles/421656/RSA-Library-with-Private-Key-Encryption-in-Csharp  

using System;
using System.Xml;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ArpanTECH
{
    /// <summary>
    /// The main RSAx Class
    /// </summary>
    public class RSAx : IDisposable
    {
        private RSAxParameters rsaParams;
        private RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public RSAxParameters Parameters { get { return rsaParams; } }

        /// <summary>
        /// Initialize the RSA class.
        /// </summary>
        /// <param name="rsaParams">Preallocated RSAxParameters containing the required keys.</param>
        public RSAx(RSAxParameters rsaParams)
        {
            this.rsaParams = rsaParams;
        }

        /// <summary>
        /// Initialize the RSA class from a XML KeyInfo string.
        /// </summary>
        /// <param name="keyInfo">XML Containing Key Information</param>
        /// <param name="ModulusSize">Length of RSA Modulus in bits.</param>
        public RSAx(String keyInfo, int ModulusSize)
        {
            this.rsaParams = new RSAxParameters(keyInfo, ModulusSize);
        }

        /// <summary>
        /// Releases all the resources.
        /// </summary>
        public void Dispose() { rng.Dispose(); }

        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Low level RSA Process function for use with private key.
        /// Should never be used; Because without padding RSA is vulnerable to attacks.  Use with caution.
        /// </summary>
        /// <param name="PlainText">Data to encrypt. Length must be less than Modulus size in octets.</param>
        /// <param name="usePrivate">True to use Private key, else Public.</param>
        /// <returns>Encrypted Data</returns>
        private byte[] RSAProcess(byte[] PlainText, bool usePrivate)
        {
            if (usePrivate && (!rsaParams.Has_PRIVATE_Info))
                throw new CryptographicException("RSA Process: Incomplete Private Key Info");

            if ((usePrivate == false) && (!rsaParams.Has_PUBLIC_Info))
                throw new CryptographicException("RSA Process: Incomplete Public Key Info");

            BigInteger _E = usePrivate ? rsaParams.D : rsaParams.E;
            BigInteger PT = OS2IP(PlainText, false);
            BigInteger M = BigInteger.ModPow(PT, _E, rsaParams.N);

            if (M.Sign == -1)
                return I2OSP(M + rsaParams.N, rsaParams.OctetsInModulus, false);
            else return I2OSP(M, rsaParams.OctetsInModulus, false);
        }

        private byte[] RSAProcessEncodePKCS(byte[] Message, bool usePrivate)
        {
            if (Message.Length > rsaParams.OctetsInModulus - 11)
                throw new ArgumentException("Message too long.");

            // RFC3447 : Page 24. [RSAES-PKCS1-V1_5-ENCRYPT ((n, e), M)]
            // EM = 0x00 || 0x02 || PS || 0x00 || Msg 

            List<byte> PCKSv15_Msg = new List<byte>();

            PCKSv15_Msg.Add(0x00);
            PCKSv15_Msg.Add(0x02);

            byte[] PS = new byte[rsaParams.OctetsInModulus - Message.Length - 3];
            rng.GetNonZeroBytes(PS);

            PCKSv15_Msg.AddRange(PS);
            PCKSv15_Msg.Add(0x00);
            PCKSv15_Msg.AddRange(Message);

            return RSAProcess(PCKSv15_Msg.ToArray(), usePrivate);
        }

        private byte[] Decrypt(byte[] Message, byte[] Parameters, bool usePrivate)
        {
            byte[] EM = new byte[0];
            try { EM = RSAProcess(Message, usePrivate); }
            catch (CryptographicException ex) { throw new CryptographicException("Exception while Decryption: " + ex.Message); }
            catch { throw new Exception("Exception while Decryption: "); }

            try
            {
                if (EM.Length >= 11)
                {
                    if ((EM[0] == 0x00) && (EM[1] == 0x02))
                    {
                        int startIndex = 2;
                        List<byte> PS = new List<byte>();
                        for (int i = startIndex; i < EM.Length; i++)
                        {
                            if (EM[i] != 0)
                                PS.Add(EM[i]);
                            else break;
                        }

                        if (PS.Count >= 8)
                        {
                            int DecodedDataIndex = startIndex + PS.Count + 1;
                            if (DecodedDataIndex < (EM.Length - 1))
                            {
                                List<byte> DATA = new List<byte>();
                                for (int i = DecodedDataIndex; i < EM.Length; i++)
                                    DATA.Add(EM[i]);
                                return DATA.ToArray();
                            }
                            else return new byte[0];
                        }// #3: Invalid Key / Invalid Random Data Length
                        else throw new CryptographicException("PKCS v1.5 Decode Error");
                    }// #2: Invalid Key / Invalid Identifiers
                    else throw new CryptographicException("PKCS v1.5 Decode Error");
                }// #1: Invalid Key / PKCS Encoding
                else throw new CryptographicException("PKCS v1.5 Decode Error");
            }
            catch (CryptographicException ex) { throw new CryptographicException("Exception while decoding: " + ex.Message); }
            catch { throw new CryptographicException("Exception while decoding"); }
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Encrypts the given message with RSA.
        /// </summary>
        /// <param name="Message">Message to Encrypt. Maximum message length is For OAEP [ModulusLengthInOctets - (2 * HashLengthInOctets) - 2] and for PKCS [ModulusLengthInOctets - 11]</param>
        /// <param name="usePrivate">True to use Private key for encryption. False to use Public key.</param>
        /// <returns>Encrypted message.</returns>
        public byte[] Encrypt(byte[] Message, bool usePrivate) { return RSAProcessEncodePKCS(Message, usePrivate); }

        /// <summary>
        /// Decrypts the given RSA encrypted message.
        /// </summary>
        /// <param name="Message">The encrypted message.</param>
        /// <param name="usePrivate">True to use Private key for decryption. False to use Public key.</param>
        /// <returns>Encrypted byte array.</returns>
        public byte[] Decrypt(byte[] Message, bool usePrivate) { return Decrypt(Message, new byte[0], usePrivate); }

        /// <summary>
        /// Decrypts the given RSA encrypted message using Private key.
        /// </summary>
        /// <param name="Message">The encrypted message.</param>
        /// <returns>Encrypted byte array.</returns>
        public byte[] Decrypt(byte[] Message) { return Decrypt(Message, new byte[0], true); }

        #endregion

        #region UTILS

        /// <summary>
        /// Converts a non-negative integer to an octet string of a specified length.
        /// </summary>
        /// <param name="x">The integer to convert.</param>
        /// <param name="xLen">Length of output octets.</param>
        /// <param name="makeLittleEndian">If True little-endian convertion is followed, big-endian otherwise.</param>
        /// <returns></returns>
        internal static byte[] I2OSP(BigInteger x, int xLen, bool makeLittleEndian)
        {
            byte[] result = new byte[xLen];
            int index = 0;
            while ((x > 0) && (index < result.Length))
            {
                result[index++] = (byte)(x % 256);
                x /= 256;
            }
            if (!makeLittleEndian)
                Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Converts a byte array to a non-negative integer.
        /// </summary>
        /// <param name="data">The number in the form of a byte array.</param>
        /// <param name="isLittleEndian">Endianness of the byte array.</param>
        /// <returns>An non-negative integer from the byte array of the specified endianness.</returns>
        internal static BigInteger OS2IP(byte[] data, bool isLittleEndian)
        {
            BigInteger bi = 0;
            if (isLittleEndian)
                for (int i = 0; i < data.Length; i++)
                    bi += BigInteger.Pow(256, i) * data[i];
            else for (int i = 1; i <= data.Length; i++)
                    bi += BigInteger.Pow(256, i - 1) * data[data.Length - i];
            return bi;
        }

        /// <summary>
        /// Performs Bitwise Ex-OR operation to two given byte arrays.
        /// </summary>
        /// <param name="A">The first byte array.</param>
        /// <param name="B">The second byte array.</param>
        /// <returns>The bitwise Ex-OR result.</returns>
        public static byte[] XOR(byte[] A, byte[] B)
        {
            if (A.Length == B.Length)
            {
                byte[] R = new byte[A.Length];
                for (int i = 0; i < A.Length; i++)
                    R[i] = (byte)(A[i] ^ B[i]);
                return R;
            }
            else throw new ArgumentException("XOR: Parameter length mismatch");
        }

        private static void FixByteArraySign(ref byte[] bytes)
        {
            if ((bytes[bytes.Length - 1] & 0x80) > 0)
            {
                byte[] temp = new byte[bytes.Length];
                Array.Copy(bytes, temp, bytes.Length);
                bytes = new byte[temp.Length + 1];
                Array.Copy(temp, bytes, temp.Length);
            }
        }

        #endregion
    }
}