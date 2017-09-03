using System;
using System.IO;

namespace I3DShapesTool
{
    /// <summary>
    /// Created by "high" https://facepunch.com/member.php?u=60704
    /// </summary>
    public static class StreamExt
    {
        [ThreadStatic]
        private static byte[] _buffer;

        private static byte[] Buffer => _buffer ?? (_buffer = new byte[16]);

        public static void FillBuffer(this Stream stream, int numBytes)
        {
            if (numBytes < 0x0 || numBytes > Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(numBytes));
            if (numBytes < 1)
                return;

            int read;
            if (numBytes == 0x1)
            {
                read = stream.ReadByte();
                if (read == -1)
                    throw new EndOfStreamException("End of stream");
                Buffer[0x0] = (byte) read;
            }
            else
            {
                int offset = 0x0;
                do
                {
                    read = stream.Read(Buffer, offset, numBytes - offset);
                    if (read == 0x0)
                        throw new EndOfStreamException("End of stream");
                    offset += read;
                } while (offset < numBytes);
            }
        }

        public static byte ReadInt8(this Stream s)
        {
            int read = s.ReadByte();
            if (read == -1)
                throw new EndOfStreamException("End of stream");
            return (byte) read;
        }

        public static bool ReadBoolean(this Stream s)
        {
            return s.ReadInt8() != 0;
        }

        public static short ReadInt16(this Stream s)
        {
            s.FillBuffer(0x2);
            return (short) (Buffer[0x0] | (Buffer[0x1] << 0x8));
        }

        public static short ReadInt16L(this Stream s)
        {
            s.FillBuffer(0x2);
            return (short) (Buffer[0x1] | (Buffer[0x0] << 0x8));
        }

        public static int ReadInt32(this Stream s)
        {
            s.FillBuffer(0x4);
            return Buffer[0x0] | (Buffer[0x1] << 0x8) | (Buffer[0x2] << 0x10) | (Buffer[0x3] << 0x18);
        }

        public static int ReadInt32L(this Stream s)
        {
            s.FillBuffer(0x4);
            return Buffer[0x3] | (Buffer[0x2] << 0x8) | (Buffer[0x1] << 0x10) | (Buffer[0x0] << 0x18);
        }

        public static long ReadInt64(this Stream s)
        {
            s.FillBuffer(0x8);
            ulong num = (uint) (Buffer[0x0] | (Buffer[0x1] << 0x8) | (Buffer[0x2] << 0x10) | (Buffer[0x3] << 0x18));
            ulong num2 = (uint) (Buffer[0x4] | (Buffer[0x5] << 0x8) | (Buffer[0x6] << 0x10) | (Buffer[0x7] << 0x18));
            return (long) ((num2 << 0x20) | num);
        }

        public static long ReadInt64L(this Stream s)
        {
            s.FillBuffer(0x8);
            ulong num = (uint) (Buffer[0x3] | (Buffer[0x2] << 0x8) | (Buffer[0x1] << 0x10) | (Buffer[0x0] << 0x18));
            ulong num2 = (uint) (Buffer[0x7] | (Buffer[0x6] << 0x8) | (Buffer[0x5] << 0x10) | (Buffer[0x4] << 0x18));
            return (long) ((num << 0x20) | num2);
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

            byte[] buffer = new byte[count];
            int offset = 0x0;
            do
            {
                int num2 = s.Read(buffer, offset, count);
                if (num2 == 0x0)
                    break;
                offset += num2;
                count -= num2;
            } while (count > 0x0);

            if (offset != buffer.Length)
            {
                byte[] dst = new byte[offset];
                System.Buffer.BlockCopy(buffer, 0x0, dst, 0x0, offset);
                buffer = dst;
            }

            return buffer;
        }
    }
}