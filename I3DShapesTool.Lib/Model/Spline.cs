using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Tools;

namespace I3DShapesTool.Lib.Model
{
    public class Spline : I3DPart
    {
        public byte[] UnknownFlags { get; set; }

        public ICollection<I3DVector> Points { get; set; }

        public Spline(byte[] rawData, Endian endian, int version) 
            : base(ShapeType.Spline, rawData, endian, version)
        {
            ReadFromRawData();
        }

        protected override void ReadContents(BinaryReader binaryReader)
        {
            UnknownFlags = binaryReader.ReadBytes(4);

            var pointCount = binaryReader.ReadInt32();
            Points = Enumerable.Range(0, pointCount)
                .Select(index => new I3DVector(binaryReader))
                .ToArray();

            if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                throw new DecodeException("Failed to read to all of spline data.");
            }
        }

        protected override void WriteContents(BinaryWriter writer)
        {
            writer.Write(UnknownFlags);
            writer.Write(Points.Count);
            foreach (var point in Points)
                point.Write(writer);
        }
    }
}
