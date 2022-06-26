using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;

namespace I3DShapesTool.Lib.Model
{
    public class Spline : I3DPart
    {
        public uint? UnknownFlags { get; set; }

        public IList<I3DVector>? Points { get; set; }

        public Spline(int version)
            : base(EntityType.Spline, version)
        {
        }

        protected override void ReadContents(BinaryReader binaryReader)
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

        protected override void WriteContents(BinaryWriter writer)
        {
            if(UnknownFlags == null || Points == null)
                throw new InvalidOperationException("Data not set on class");

            writer.Write((uint)UnknownFlags);
            writer.Write(Points.Count);
            foreach(I3DVector point in Points)
                point.Write(writer);
        }
    }
}
