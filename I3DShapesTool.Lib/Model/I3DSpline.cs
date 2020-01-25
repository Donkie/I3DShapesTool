using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace I3DShapesTool
{
    public class I3DSpline : I3DPart
    {
        public bool IsLoop { get; set; }

        public ICollection<I3DPoint> Points { get; set; }

        public I3DSpline(I3DPart part)
            : base(part)
        {
            using (var stream = new MemoryStream(RawData))
            using (var binaryReader = Endian == Endian.Big ? new BigEndianBinaryReader(stream) : new BinaryReader(stream))
            {
                Load(binaryReader);
            }
        }

        private void Load(BinaryReader binaryReader)
        {
            var nameLength = (int)binaryReader.ReadUInt32();
            Name = System.Text.Encoding.ASCII.GetString(binaryReader.ReadBytes(nameLength));

            binaryReader.BaseStream.Align(4); // Align the stream to short

            ShapeId = binaryReader.ReadUInt32();
            var buffer = binaryReader.ReadBytes(4);
            IsLoop = buffer[0] == 1;

            var pointCount = binaryReader.ReadInt32();
            Points = Enumerable.Range(0, pointCount)
                .Select(index => new I3DPoint(binaryReader))
                .ToArray();
        }
    }
}
