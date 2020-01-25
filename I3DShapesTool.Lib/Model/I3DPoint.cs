using System.IO;

namespace I3DShapesTool
{
    public class I3DPoint
    {
        public I3DPoint(BinaryReader binaryReader)
        {
            X = binaryReader.ReadSingle();
            Y = binaryReader.ReadSingle();
            Z = binaryReader.ReadSingle();
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }
}
