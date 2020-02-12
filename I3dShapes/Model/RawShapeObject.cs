using System.IO;
using I3dShapes.Model.Contract;
using I3dShapes.Tools.Extensions;

namespace I3dShapes.Model
{
    public class RawShapeObject : ShapeObject, IRawShapeObject
    {
        public RawShapeObject(uint rawType, BinaryReader reader)
            : base(ShapeType.Raw)
        {
            RawType = rawType;
            Load(reader);
        }

        public uint RawType { get; }

        public byte[] RawData { get; private set; }

        protected void Load(BinaryReader reader)
        {
            RawData = reader.ReadAll();
        }
    }
}
