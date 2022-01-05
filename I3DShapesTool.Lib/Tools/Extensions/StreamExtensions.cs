using System.IO;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    public static class StreamExtension
    {
        public static void Align(this Stream s, byte wordSize)
        {
            long mod = s.Position % wordSize;
            if(mod == 0)
                return;

            int bytesToRead = (int)(4 - mod);
            byte[] destBuffer = new byte[bytesToRead];
            while(bytesToRead > 0)
            {
                int bytesRead = s.Read(destBuffer, 0, bytesToRead);
                if(bytesRead <= 0)
                    return;
                bytesToRead -= bytesRead;
            }
        }
    }
}