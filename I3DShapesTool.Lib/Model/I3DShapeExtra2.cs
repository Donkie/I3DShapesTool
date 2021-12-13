using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShapeExtra2
    {
        public uint Flags { get; }
        public float[]? Floats { get; }
        public byte[] Data { get; }

        public I3DShapeExtra2(BinaryReader reader)
        {
            Flags = reader.ReadUInt32();

            if((Flags & 4) != 0)
            {
                // These could be ints as well, not entirely sure
                Floats = new float[3]
                {
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                };
            }

            var numBytes = reader.ReadInt32();
            Data = reader.ReadBytes(numBytes);
            if (Data.Length != numBytes)
                throw new DecodeException("Tried to read past end of file");
        }
    }
}
