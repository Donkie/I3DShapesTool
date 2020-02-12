using System.IO;

namespace I3dShapes.Model.Primitive
{
    // ReSharper disable InconsistentNaming
    public class TextureCoordinateUV
        // ReSharper restore InconsistentNaming
    {
        public TextureCoordinateUV(BinaryReader reader)
        {
            Load(reader);
        }

        public float U { get; private set; }
        public float V { get; private set; }

        private void Load(BinaryReader reader)
        {
            U = reader.ReadSingle();
            V = reader.ReadSingle();
        }
    }
}
