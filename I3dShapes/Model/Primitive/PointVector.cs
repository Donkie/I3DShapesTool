using System.IO;

namespace I3dShapes.Model.Primitive
{
    public class PointVector
    {
        public PointVector(BinaryReader reader)
        {
            Load(reader);
        }

        /// <summary>
        /// Coordinate X
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Coordinate Y
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Coordinate Z
        /// </summary>
        public float Z { get; set; }

        private void Load(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
        }
    }
}
