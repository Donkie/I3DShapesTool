using System.IO;
using I3dShapes.Model.Contract;
using I3dShapes.Tools;
using I3dShapes.Tools.Extensions;

namespace I3dShapes.Model
{
    public class RawNamedShapeObject : NamedShapeObject, IRawNamedShapeObject
    {
        private readonly Endian _endian;

        public RawNamedShapeObject(uint rawType, BinaryReader reader, Endian endian)
            : base(ShapeType.Raw)
        {
            _endian = endian;
            RawType = rawType;
            Load(reader);
        }

        /// <inheritdoc />
        public uint RawType { get; }

        /// <inheritdoc />
        public byte[] RawData { get; private set; }

        /// <inheritdoc />
        public long ContentPosition { get; private set; }

        private new void Load(BinaryReader reader)
        {
            RawData = reader.ReadAll();
            using var stream = new MemoryStream(RawData);
            using var rawReader = new EndianBinaryReader(stream, _endian);
            base.Load(rawReader, true);
            ContentPosition = rawReader.Position();
        }
    }
}
