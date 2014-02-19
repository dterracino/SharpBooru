// @Date : 15th July 2012
// @Author : Arpan Jati (arpan4017@yahoo.com; arpan4017@gmail.com)
// @Library : ArpanTECH.RSAx
// @CodeProject: http://www.codeproject.com/Articles/421656/RSA-Library-with-Private-Key-Encryption-in-Csharp  

using System;
using System.IO;
using System.Xml;
using System.Numerics;
using System.Security.Cryptography;

namespace ArpanTECH
{
    /// <summary>
    /// Class to keep the basic RSA parameters like Keys, and other information.
    /// </summary>
    public class RSAxParameters
    {
        private int _ModulusOctets;
        private BigInteger _N;
        private BigInteger _E;
        private BigInteger _D;
        private int _hLen = 20;
        private bool _Has_PRIVATE_Info = false;
        private bool _Has_PUBLIC_Info = false;

        public bool Has_PRIVATE_Info { get { return _Has_PRIVATE_Info; } }
        public bool Has_PUBLIC_Info { get { return _Has_PUBLIC_Info; } }

        public int OctetsInModulus { get { return _ModulusOctets; } }

        public BigInteger N { get { return _N; } }

        public int hLen { get { return _hLen; } }

        public BigInteger E { get { return _E; } }
        public BigInteger D { get { return _D; } }

        /// <summary>
        /// Initialize the RSA class. It's assumed that both the Public and Extended Private info are there. 
        /// </summary>
        /// <param name="rsaParams">Preallocated RSAParameters containing the required keys.</param>
        /// <param name="ModulusSize">Modulus size in bits</param>
        public RSAxParameters(RSAParameters rsaParams, int ModulusSize)
        {
            // rsaParams;
            _ModulusOctets = ModulusSize / 8;
            _E = RSAx.OS2IP(rsaParams.Exponent, false);
            _D = RSAx.OS2IP(rsaParams.D, false);
            _N = RSAx.OS2IP(rsaParams.Modulus, false);
            _Has_PUBLIC_Info = true;
            _Has_PRIVATE_Info = true;
        }

        public RSAxParameters(string XMLKeyInfo, int ModulusSize)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XMLKeyInfo);

            try
            {
                _ModulusOctets = ModulusSize / 8;
                _N = new BigInteger(Convert.FromBase64String(doc.DocumentElement.SelectSingleNode("N").InnerText));
                _E = new BigInteger(Convert.FromBase64String(doc.DocumentElement.SelectSingleNode("E").InnerText));
                _Has_PUBLIC_Info = true;
            }
            catch { }

            if (!_Has_PUBLIC_Info)
                try
                {
                    _D = new BigInteger(Convert.FromBase64String(doc.DocumentElement.SelectSingleNode("D").InnerText));
                    _Has_PRIVATE_Info = true;
                }
                catch { }

            if (!(_Has_PUBLIC_Info || _Has_PRIVATE_Info))
                throw new Exception("Could not process XMLKeyInfo. Incomplete key information.");
        }

        /// <summary>
        /// Initialize the RSA class. Only the public parameters.
        /// </summary>
        /// <param name="Modulus">Modulus of the RSA key.</param>
        /// <param name="Exponent">Exponent of the RSA key</param>
        /// <param name="ModulusSize">Modulus size in number of bits. Ex: 512, 1024, 2048, 4096 etc.</param>
        public RSAxParameters(byte[] Modulus, byte[] Exponent, int ModulusSize)
        {
            // rsaParams;
            _ModulusOctets = ModulusSize / 8;
            //_E = RSAx.OS2IP(Exponent, false);
            _N = new BigInteger(Modulus);
            _E = new BigInteger(Exponent);
            _Has_PUBLIC_Info = true;
        }

        /*
        /// <summary>
        /// Initialize the RSA class.
        /// </summary>
        /// <param name="Modulus">Modulus of the RSA key.</param>
        /// <param name="Exponent">Exponent of the RSA key</param>
        /// /// <param name="D">Exponent of the RSA key</param>
        /// <param name="ModulusSize">Modulus size in number of bits. Ex: 512, 1024, 2048, 4096 etc.</param>
        public RSAxParameters(byte[] Modulus, byte[] Exponent, byte[] D, int ModulusSize)
        {
            // rsaParams;
            _ModulusOctets = ModulusSize / 8;
            _E = RSAx.OS2IP(Exponent, false);
            _N = RSAx.OS2IP(Modulus, false);
            _D = RSAx.OS2IP(D, false);
            _Has_PUBLIC_Info = true;
            _Has_PRIVATE_Info = true;
        }

        /// <summary>
        /// Initialize the RSA class. For CRT.
        /// </summary>
        /// <param name="Modulus">Modulus of the RSA key.</param>
        /// <param name="Exponent">Exponent of the RSA key</param>
        /// /// <param name="D">Exponent of the RSA key</param>
        /// <param name="P">P paramater of RSA Algorithm.</param>
        /// <param name="Q">Q paramater of RSA Algorithm.</param>
        /// <param name="DP">DP paramater of RSA Algorithm.</param>
        /// <param name="DQ">DQ paramater of RSA Algorithm.</param>
        /// <param name="InverseQ">InverseQ paramater of RSA Algorithm.</param>
        /// <param name="ModulusSize">Modulus size in number of bits. Ex: 512, 1024, 2048, 4096 etc.</param>
        public RSAxParameters(byte[] Modulus, byte[] Exponent, byte[] D, byte[] P, byte[] Q, byte[] DP, byte[] DQ, byte[] InverseQ, int ModulusSize)
        {
            // rsaParams;
            _ModulusOctets = ModulusSize / 8;
            _E = RSAx.OS2IP(Exponent, false);
            _N = RSAx.OS2IP(Modulus, false);
            _D = RSAx.OS2IP(D, false);
            _P = RSAx.OS2IP(P, false);
            _Q = RSAx.OS2IP(Q, false);
            _DP = RSAx.OS2IP(DP, false);
            _DQ = RSAx.OS2IP(DQ, false);
            _InverseQ = RSAx.OS2IP(InverseQ, false);
            _Has_CRT_Info = true;
            _Has_PUBLIC_Info = true;
            _Has_PRIVATE_Info = true;
        }
        */

        public void ToFile(string File)
        {
            using (FileStream fs = new FileStream(File, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (XmlWriter writer = XmlWriter.Create(fs))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("RSAKeys");
                if (_Has_PUBLIC_Info)
                {
                    writer.WriteElementString("N", Convert.ToBase64String(_N.ToByteArray()));
                    writer.WriteElementString("E", Convert.ToBase64String(_E.ToByteArray()));
                }
                if (_Has_PRIVATE_Info)
                    writer.WriteElementString("D", Convert.ToBase64String(_D.ToByteArray()));
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}