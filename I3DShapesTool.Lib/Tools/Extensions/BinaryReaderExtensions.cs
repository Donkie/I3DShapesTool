using System.IO;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static byte[] Align(this BinaryReader reader, int countBytes)
        {
            long mod = reader.BaseStream.Position % countBytes;
            if(mod == 0)
            {
                return new byte[0];
            }

            int bytesToRead = (int)(countBytes - mod);
            return reader.ReadBytes(bytesToRead);
        }
    }
}