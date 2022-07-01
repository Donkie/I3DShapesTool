using System;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib.Tools
{
    public class EndianBinaryWriter : BinaryWriter
    {
        public EndianBinaryWriter(Stream input, Endian endian)
            : base(input)
        {
            Endian = endian;
        }

        public EndianBinaryWriter(Stream input, Encoding encoding, Endian endian)
            : base(input, encoding)
        {
            Endian = endian;
        }

        public EndianBinaryWriter(Stream input, Encoding encoding, bool leaveOpen, Endian endian)
            : base(input, encoding, leaveOpen)
        {
            Endian = endian;
        }

        public Endian Endian { get; }

        public override void Write(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(ushort value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(uint value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(ulong value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(short value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(int value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        public override void Write(long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            Swipe(data);
            Write(data);
        }

        private void Swipe(byte[] data)
        {
            if((BitConverter.IsLittleEndian && Endian == Endian.Big)
                || (!BitConverter.IsLittleEndian && Endian == Endian.Little))
            {
                Array.Reverse(data);
            }
        }
    }
}
