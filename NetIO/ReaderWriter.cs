using System;
using System.IO;

namespace TA.SharpBooru.NetIO
{
    public class ReaderWriter : IDisposable
    {
        public Stream Stream { get; set; }

        public ReaderWriter(Stream Stream) { this.Stream = Stream; }

        public void Write(byte Byte) { Stream.Write(new byte[1] { Byte }, 0, 1); }

        public byte ReadByte()
        {
            int ibyte = Stream.ReadByte();
            if (ibyte < 0)
                throw new EndOfStreamException();
            else return (byte)ibyte;
        }

        public void Write(byte[] Bytes, bool LengthPrefix)
        {
            if (LengthPrefix)
                Write((uint)Bytes.Length);
            Stream.Write(Bytes, 0, Bytes.Length);
        }

        public byte[] ReadBytes() { return ReadBytes(ReadUInt()); }
        public byte[] ReadBytes(uint Length)
        {
            byte[] buffer = new byte[Length];
            if (Stream.Read(buffer, 0, buffer.Length) != Length)
                throw new EndOfStreamException();
            else return buffer;
        }

        public void Write(ushort UShort)
        {
            Write((byte)(UShort & 0xFF));
            Write((byte)((UShort >> 8) & 0xFF));
        }

        public ushort ReadUShort()
        {
            byte[] bytes = ReadBytes(2);
            return (ushort)(bytes[0] | bytes[1] << 8);
        }

        public void Write(uint UInt)
        {
            Write((byte)(UInt & 0xFF));
            Write((byte)((UInt >> 8) & 0xFF));
            Write((byte)((UInt >> 16) & 0xFF));
            Write((byte)((UInt >> 24) & 0xFF));
        }

        public uint ReadUInt()
        {
            byte[] bytes = ReadBytes(4);
            return (uint)(bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24);
        }

        public void Write(char Char)
        {
            Write((byte)(Char & 0xFF));
            Write((byte)((Char >> 8) & 0xFF));
        }

        public char ReadChar()
        {
            byte[] bytes = ReadBytes(2);
            return (char)(bytes[0] | bytes[1] << 8);
        }

        public void Write(string String, bool LengthPrefix)
        {
            if (LengthPrefix)
                Write((ushort)String.Length);
            for (ushort s = 0; s < String.Length; s++)
                Write(String[s]);
        }

        public string ReadString() { return ReadString(ReadUShort()); }
        public string ReadString(ushort Length)
        {
            char[] chars = new char[Length];
            for (ushort s = 0; s < Length; s++)
                chars[s] = ReadChar();
            return new string(chars);
        }
    }
}