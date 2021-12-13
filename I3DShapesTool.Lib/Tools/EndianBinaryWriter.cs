using System;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib.Tools
{
    public class EndianBinaryWriter : BinaryWriter
    {
        public EndianBinaryWriter(Stream input, Endian endian = Endian.Big)
            : base(input)
        {
            Endian = endian;
        }

        public EndianBinaryWriter(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public EndianBinaryWriter(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
        }

        public Endian Endian { get; }

        public override void Write(float value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(double value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(ushort value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(uint value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(ulong value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(short value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(int value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(long value)
        {
            var data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        private void Swipe(byte[] data)
        {
            if ((BitConverter.IsLittleEndian && Endian == Endian.Big)
                || (!BitConverter.IsLittleEndian && Endian == Endian.Little))
            {
                Array.Reverse(data);
            }
        }
    }
}
