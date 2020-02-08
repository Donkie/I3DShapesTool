using System;
using System.IO;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static byte[] ReadToEnd(this BinaryReader reader)
        {
            var residualLength = reader.BaseStream.Length - reader.BaseStream.Position;
            if (residualLength > int.MaxValue)
            {
                throw new IndexOutOfRangeException($"Residual length: {residualLength}");
            }

            return reader.ReadBytes((int) residualLength);
        }

        public static byte[] Align(this BinaryReader reader, int countBytes)
        {
            var mod = reader.BaseStream.Position % countBytes;
            if (mod == 0)
            {
                return new byte[0];
            }

            var bytesToRead = (int)(countBytes - mod);
            return reader.ReadBytes(bytesToRead);
        }
    }
}