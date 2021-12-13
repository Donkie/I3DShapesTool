using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DVector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public I3DVector4(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
            W = br.ReadSingle();
        }

        public override string ToString()
        {
            return $"4D ({X}, {Y}, {Z}, {W})";
        }
    }
}
