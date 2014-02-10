using System;
using System.IO;

namespace TA.SharpBooru
{
    public class ReaderWriter : IDisposable
    {
        public bool DisposeStreams { get; set; }
        public readonly Stream ReadStream;
        public readonly Stream WriteStream;

        public ReaderWriter(Stream Stream)
        {
            DisposeStreams = false;
            ReadStream = Stream;
            WriteStream = Stream;
        }

        public ReaderWriter(Stream ReadStream, Stream WriteStream)
        {
            DisposeStreams = false;
            this.ReadStream = ReadStream;
            this.WriteStream = WriteStream;
        }

        public void Dispose()
        {
            if (DisposeStreams)
            {
                ReadStream.Dispose();
                WriteStream.Dispose();
            }
        }

        public void Write(byte Byte) { WriteStream.Write(new byte[1] { Byte }, 0, 1); }

        public byte ReadByte()
        {
            int ibyte = ReadStream.ReadByte();
            if (ibyte < 0)
                throw new EndOfStreamException();
            else return (byte)ibyte;
        }

        public void Write(byte[] Bytes, bool LengthPrefix)
        {
            if (LengthPrefix)
                Write((uint)Bytes.Length);
            WriteStream.Write(Bytes, 0, Bytes.Length);
        }

        public byte[] ReadBytes() { return ReadBytes(ReadUInt()); }
        public byte[] ReadBytes(uint Length)
        {
            byte[] buffer = new byte[Length];
            if (ReadStream.Read(buffer, 0, buffer.Length) != Length)
                throw new EndOfStreamException();
            else return buffer;
        }

        public void Write(ushort UShort)
        {
            Write((byte)(UShort & 0xFF));
            Write((byte)(UShort >> 8));
        }

        public ushort ReadUShort()
        {
            byte[] bytes = ReadBytes(2);
            return (ushort)(bytes[0] | bytes[1] << 8);
        }

        public void Write(uint UInt)
        {
            Write((ushort)(UInt & 0xFFFF));
            Write((ushort)(UInt >> 16));
        }

        public uint ReadUInt()
        {
            ushort ushort1 = ReadUShort();
            return (uint)(ushort1 | ReadUShort() << 16);
        }

        public void Write(ulong ULong)
        {
            Write((uint)(ULong & 0xFFFFFFFF));
            Write((uint)(ULong >> 32));
        }

        public ulong ReadULong()
        {
            uint uint1 = ReadUInt();
            return (ulong)(uint1 | ReadUInt() << 32);
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

        public void Write(bool Bool) { Write((byte)1); }

        public bool ReadBool() { return ReadByte() > 0; }

        public void Write(int Int) { unchecked { Write((uint)Int); } }

        public int ReadInt() { unchecked { return (int)ReadUInt(); } }

        public void Write(long Long) { unchecked { Write((ulong)Long); } }

        public long ReadLong() { unchecked { return (long)ReadULong(); } }
    }
}