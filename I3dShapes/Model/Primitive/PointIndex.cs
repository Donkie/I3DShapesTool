using System.IO;

namespace I3dShapes.Model.Primitive
{
    public class PointIndex
    {
        public PointIndex(BinaryReader reader, bool isMany = false)
        {
            Load(reader, isMany);
        }

        public uint Point1 { get; private set; }

        public uint Point2 { get; private set; }

        public uint Point3 { get; private set; }

        private void Load(BinaryReader reader, bool isMany)
        {
            if (isMany)
            {
                Point1 = reader.ReadUInt32();
                Point2 = reader.ReadUInt32();
                Point3 = reader.ReadUInt32();
            }
            else
            {
                Point1 = reader.ReadUInt16();
                Point2 = reader.ReadUInt16();
                Point3 = reader.ReadUInt16();
            }
        }
    }
}
