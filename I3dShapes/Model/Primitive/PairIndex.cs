using System.IO;

namespace I3dShapes.Model.Primitive
{
    public class PairIndex
    {
        public PairIndex(BinaryReader reader)
        {
            Load(reader);
        }

        private short Index1 { get; set; }

        private short Index2 { get; set; }

        private void Load(BinaryReader reader)
        {
            Index1 = reader.ReadInt16();
            Index2 = reader.ReadInt16();
        }
    }
}
