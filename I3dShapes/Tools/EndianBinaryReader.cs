using System;
using System.IO;
using System.Text;

namespace I3dShapes.Tools
{
    public class EndianBinaryReader : BinaryReader
    {
        public EndianBinaryReader(Stream input, Endian endian = Endian.Big, bool leaveOpen = false)
            : base(input, Encoding.ASCII, leaveOpen)
        {
            Endian = endian;
        }

        public EndianBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
        }

        public Endian Endian { get; }

        public override float ReadSingle()
        {
            var data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToSingle(data, 0);
        }

        public override double ReadDouble()
        {
            var data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToDouble(data, 0);
        }

        public override ushort ReadUInt16()
        {
            var data = ReadBytes(2);
            Swipe(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override uint ReadUInt32()
        {
            var data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override ulong ReadUInt64()
        {
            var data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public override short ReadInt16()
        {
            var data = ReadBytes(2);
            Swipe(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override int ReadInt32()
        {
            var data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override long ReadInt64()
        {
            var data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToInt64(data, 0);
        }

        public void Swipe(byte[] data)
        {
            if (BitConverter.IsLittleEndian && Endian == Endian.Big)
            {
                Array.Reverse(data);
            }
        }
    }
}
