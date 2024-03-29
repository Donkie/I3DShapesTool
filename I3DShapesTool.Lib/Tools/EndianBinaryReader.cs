﻿using System;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib.Tools
{
    public class EndianBinaryReader : BinaryReader
    {
        public EndianBinaryReader(Stream input, Endian endian)
            : base(input)
        {
            Endian = endian;
        }

        public EndianBinaryReader(Stream input, Encoding encoding, Endian endian)
            : base(input, encoding)
        {
            Endian = endian;
        }

        public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endian endian)
            : base(input, encoding, leaveOpen)
        {
            Endian = endian;
        }

        public Endian Endian { get; }

        public override float ReadSingle()
        {
            byte[] data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToSingle(data, 0);
        }

        public override double ReadDouble()
        {
            byte[] data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToDouble(data, 0);
        }

        public override ushort ReadUInt16()
        {
            byte[] data = ReadBytes(2);
            Swipe(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public override short ReadInt16()
        {
            byte[] data = ReadBytes(2);
            Swipe(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override int ReadInt32()
        {
            byte[] data = ReadBytes(4);
            Swipe(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override long ReadInt64()
        {
            byte[] data = ReadBytes(8);
            Swipe(data);
            return BitConverter.ToInt64(data, 0);
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
