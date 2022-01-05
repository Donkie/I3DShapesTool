using System;
using System.IO;

namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// File header
    /// </summary>
    public class FileHeader
    {
        public short Version { get; }
        public byte Seed { get; }

        public FileHeader(short version, byte seed)
        {
            Version = version;
            Seed = seed;
        }

        public static FileHeader Read(Stream stream)
        {
            byte b1 = (byte)stream.ReadByte();
            byte b2 = (byte)stream.ReadByte();
            byte b3 = (byte)stream.ReadByte();
            byte b4 = (byte)stream.ReadByte();

            byte seed;
            short version;

            if(b1 >= 4) // Might be 5 as well
            {
                version = b1;
                seed = b3;
            }
            else if(b4 == 2 || b4 == 3)
            {
                version = b4;
                seed = b2;
            }
            else
            {
                throw new NotSupportedException("Unknown version");
            }

            return new FileHeader(version, seed);
        }

        public void Write(Stream stream)
        {
            if(Version >= 4) // Might be 5 as well
            {
                stream.WriteByte((byte)Version);
                stream.WriteByte(0);
                stream.WriteByte(Seed);
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte(0);
                stream.WriteByte(Seed);
                stream.WriteByte(0);
                stream.WriteByte((byte)Version);
            }
        }
    }
}
