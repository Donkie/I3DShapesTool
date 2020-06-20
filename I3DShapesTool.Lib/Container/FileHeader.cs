using System;
using System.IO;

namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// File header
    /// </summary>
    public class FileHeader
    {
        public short Version { get; private set; }
        public byte Seed { get; set; }

        public static FileHeader Read(Stream stream)
        {
            var b1 = (byte)stream.ReadByte();
            var b2 = (byte)stream.ReadByte();
            var b3 = (byte)stream.ReadByte();
            var b4 = (byte)stream.ReadByte();

            byte seed;
            short version;

            if (b1 >= 4) // Might be 5 as well
            {
                version = b1;
                seed = b3;
            }
            else if (b4 == 2 || b4 == 3)
            {
                version = b4;
                seed = b2;
            }
            else
            {
                throw new NotSupportedException("Unknown version");
            }

            return new FileHeader
            {
                Seed = seed,
                Version = version
            };
        }
    }
}
