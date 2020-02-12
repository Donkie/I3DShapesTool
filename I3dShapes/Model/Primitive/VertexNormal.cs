using System.IO;

namespace I3dShapes.Model.Primitive
{
    public class VertexNormal
    {
        public VertexNormal(BinaryReader reader)
        {
            Load(reader);
        }

        /// <summary>
        /// Coordinate X
        /// </summary>
        public float X { get; private set; }

        /// <summary>
        /// Coordinate Y
        /// </summary>
        public float Y { get; private set; }

        /// <summary>
        /// Coordinate Z
        /// </summary>
        public float Z { get; private set; }

        private void Load(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
        }
    }
}
