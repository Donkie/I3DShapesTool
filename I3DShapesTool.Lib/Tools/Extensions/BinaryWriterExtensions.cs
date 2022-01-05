using System.IO;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void Align(this BinaryWriter writer, int countBytes)
        {
            long mod = writer.BaseStream.Position % countBytes;
            if(mod == 0)
                return;

            int bytesToWrite = (int)(countBytes - mod);
            writer.Write(new byte[bytesToWrite]);
        }
    }
}