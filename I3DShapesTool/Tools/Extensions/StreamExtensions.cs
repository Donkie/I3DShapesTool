using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace I3DShapesTool
{
    /// <summary>
    /// Created by "high" https://facepunch.com/member.php?u=60704
    /// </summary>
    public static class StreamExtensions
    {
        private const int MaxBufferLength = 16;

        public static byte[] ReadBuffer(this Stream stream, int numBytes)
        {
            if (numBytes < 0 || numBytes > MaxBufferLength)
                throw new ArgumentOutOfRangeException(nameof(numBytes));
            if (numBytes < 1)
                return new byte[0];

            var buffer = new byte[numBytes];

            int read;
            var offset = 0;
            do
            {
                read = stream.Read(buffer, offset, numBytes - offset);
                if (read == 0)
                    throw new EndOfStreamException("End of stream");
                offset += read;
            } while (offset < numBytes);

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
            List<byte> bytes = new List<byte>();
            byte b;
            do
            {
                b = (byte)stream.ReadByte();
                bytes.Add(b);
            } while (b != 0x0);

            bytes.RemoveAt(bytes.Count - 1);

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public static byte ReadInt8(this Stream s)
        {
            int read = s.ReadByte();
            if (read == -1)
                throw new EndOfStreamException("End of stream");
            return (byte)read;
        }

        public static bool ReadBoolean(this Stream s)
        {
            return s.ReadInt8() != 0;
        }

        public static short ReadInt16(this Stream s, Endian endian)
        {
            var buffer = s.ReadBuffer(2);

            if (endian == Endian.Big)
                return (short)(buffer[1] | (buffer[0] << 8));
            else
                return (short)(buffer[0] | (buffer[1] << 8));
        }

        public static int ReadInt32(this Stream s, Endian endian)
        {
            var buffer = s.ReadBuffer(4);

            if (endian == Endian.Big)
                return buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24);
            else
                return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
        }

        public static long ReadInt64(this Stream s, Endian endian)
        {
            var buffer = s.ReadBuffer(8);
            if (endian == Endian.Big)
            {
                ulong num = (uint)(buffer[3] | (buffer[2] << 8) | (buffer[1] << 16) | (buffer[0] << 24));
                ulong num2 = (uint)(buffer[7] | (buffer[6] << 8) | (buffer[5] << 16) | (buffer[4] << 24));
                return (long)((num << 32) | num2);
            }
            else
            {

                ulong num = (uint) (buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
                ulong num2 = (uint) (buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
                return (long) ((num2 << 32) | num);
            }
        }
        /*
        public static unsafe double ReadDouble(this Stream s)
        {
            ulong ret = (ulong) s.ReadInt64();
            return *((double*) &ret);
        }

        public static unsafe float ReadSingle(this Stream s)
        {
            int ret = s.ReadInt32();
            return *((float*) &ret);
        }
        */
        public static byte[] ReadBytes(this Stream s, int count)
        {
            if (count < 0x0)
                throw new ArgumentOutOfRangeException(nameof(count));

            //count = I3DDecryptor.RoundUpTo(count, 4);

            var buffer = new byte[count];
            var offset = 0;
            do
            {
                var num2 = s.Read(buffer, offset, count);
                if (num2 == 0)
                    break;
                offset += num2;
                count -= num2;
            } while (count > 0);

            if (offset != buffer.Length)
            {
                var dst = new byte[offset];
                Array.Copy(buffer, 0, dst, 0, offset);
                buffer = dst;
            }

            return buffer;
        }
    }
}