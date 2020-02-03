using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    /// <summary>
    /// Created by "high" https://facepunch.com/member.php?u=60704
    /// </summary>
    public static class StreamExtension
    {
        private const int MaxBufferLength = 16;

        public static byte[] FillBuffer(this Stream stream, int numBytes)
        {
            if (numBytes < 0x0 || numBytes > MaxBufferLength)
                throw new ArgumentOutOfRangeException(nameof(numBytes));
            if (numBytes < 1)
                return new byte[0];

            var buffer = new byte[numBytes];

            int read;
            if (numBytes == 0x1)
            {
                read = stream.ReadByte();
                if (read == -1)
                    throw new EndOfStreamException("End of stream");
                buffer[0x0] = (byte) read;
            }
            else
            {
                var offset = 0x0;
                do
                {
                    read = stream.Read(buffer, offset, numBytes - offset);
                    if (read == 0x0)
                        throw new EndOfStreamException("End of stream");
                    offset += read;
                } while (offset < numBytes);
            }

            return buffer;
        }

        public static void Align(this Stream s, byte wordSize)
        {
            var mod = s.Position % wordSize;
            if (mod == 0) return;

            var bytesToRead = (int)(4 - mod);
            s.ReadBytes(bytesToRead);
        }

        public static string ReadNullTerminatedString(this Stream stream)
        {
            var bytes = new List<byte>();
            byte b;
            do
            {
                b = (byte) stream.ReadByte();
                bytes.Add(b);
            } while (b != 0x0);

            bytes.RemoveAt(bytes.Count - 1);

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public static byte ReadInt8(this Stream s)
        {
            var read = s.ReadByte();
            if (read == -1)
                throw new EndOfStreamException("End of stream");
            return (byte) read;
        }

        public static bool ReadBoolean(this Stream s)
        {
            return s.ReadInt8() != 0;
        }

        public static short ReadInt16(this Stream s, Endian endian)
        {
            var buffer = s.FillBuffer(0x2);

            if (endian == Endian.Big)
                return (short)(buffer[0x1] | (buffer[0x0] << 0x8));

            return (short) (buffer[0x0] | (buffer[0x1] << 0x8));
        }

        public static int ReadInt32(this Stream s, Endian endian)
        {
            var buffer = s.FillBuffer(0x4);

            if (endian == Endian.Big)
                return buffer[0x3] | (buffer[0x2] << 0x8) | (buffer[0x1] << 0x10) | (buffer[0x0] << 0x18);
            
            return buffer[0x0] | (buffer[0x1] << 0x8) | (buffer[0x2] << 0x10) | (buffer[0x3] << 0x18);
        }

        public static long ReadInt64(this Stream s, Endian endian)
        {
            var buffer = s.FillBuffer(0x8);

            if (endian == Endian.Big)
            {
                ulong num = (uint)(buffer[0x3] | (buffer[0x2] << 0x8) | (buffer[0x1] << 0x10) | (buffer[0x0] << 0x18));
                ulong num2 = (uint)(buffer[0x7] | (buffer[0x6] << 0x8) | (buffer[0x5] << 0x10) | (buffer[0x4] << 0x18));
                return (long)((num << 0x20) | num2);
            }
            else
            {
                ulong num = (uint)(buffer[0x0] | (buffer[0x1] << 0x8) | (buffer[0x2] << 0x10) | (buffer[0x3] << 0x18));
                ulong num2 = (uint)(buffer[0x4] | (buffer[0x5] << 0x8) | (buffer[0x6] << 0x10) | (buffer[0x7] << 0x18));
                return (long)((num2 << 0x20) | num);
            }

        }

        public static byte[] ReadBytes(this Stream s, int count)
        {
            if (count < 0x0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var buffer = new byte[count];
            var offset = 0x0;
            do
            {
                var num2 = s.Read(buffer, offset, count);
                if (num2 == 0x0)
                    break;
                offset += num2;
                count -= num2;
            } while (count > 0x0);

            if (offset == buffer.Length)
                return buffer;

            var dst = new byte[offset];
            System.Buffer.BlockCopy(buffer, 0x0, dst, 0x0, offset);
            buffer = dst;

            return buffer;
        }
    }
}