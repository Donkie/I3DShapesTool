using System;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib
{
    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }
        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }
        public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }
        public override float ReadSingle()
        {
            var data = ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        public override double ReadDouble()
        {
            var data = ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
        public override ushort ReadUInt16()
        {
            var data = ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }
        public override uint ReadUInt32()
        {
            var data = ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }
        public override ulong ReadUInt64()
        {
            var data = ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }
        public override short ReadInt16()
        {
            var data = ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        public override int ReadInt32()
        {
            var data = ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        public override long ReadInt64()
        {
            var data = ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
    }
}
