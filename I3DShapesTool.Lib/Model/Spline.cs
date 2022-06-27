using I3DShapesTool.Lib.Container;
using System.IO;
using System.Linq;

namespace I3DShapesTool.Lib.Model
{
    public class Spline : I3DPart
    {
        public uint UnknownFlags { get; set; } = 0;

        public I3DVector[] Points { get; set; } = new I3DVector[0];

        public override EntityType Type => EntityType.Spline;

        public Spline()
        {
        }

        protected override void ReadContents(BinaryReader binaryReader, short fileVersion)
        {
            UnknownFlags = binaryReader.ReadUInt32();

            int pointCount = binaryReader.ReadInt32();
            Points = Enumerable.Range(0, pointCount)
                .Select(index => new I3DVector(binaryReader))
                .ToArray();

            if(binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                throw new DecodeException("Failed to read to all of spline data.");
            }
        }

        protected override void WriteContents(BinaryWriter writer, short fileVersion)
        {
            writer.Write(UnknownFlags);
            writer.Write(Points.Length);
            foreach(I3DVector point in Points)
                point.Write(writer);
        }
    }
}
