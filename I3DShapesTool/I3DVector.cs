using System.IO;

namespace I3DShapesTool
{
    class I3DVector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public I3DVector(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
        }
    }
}
