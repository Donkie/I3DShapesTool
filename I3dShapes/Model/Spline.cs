using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Exceptions;
using I3dShapes.Model.Primitive;
using I3dShapes.Tools.Extensions;

namespace I3dShapes.Model
{
    public class Spline : NamedShapeObject
    {
        public Spline(BinaryReader reader)
            : base(ShapeType.Spline)
        {
            Load(reader);
        }

        public uint UnknownFlags { get; set; }

        public ICollection<PointVector> Points { get; set; }

        private new void Load(BinaryReader reader)
        {
            base.Load(reader);

            UnknownFlags = reader.ReadUInt32();

            var pointCount = reader.ReadUInt32();
            // ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
            if (pointCount > Int32.MaxValue)
                // ReSharper restore BuiltInTypeReferenceStyleForMemberAccess
            {
                throw new UnknownFormatShapeException();
            }

            Points = Enumerable
                     .Range(0, (int)pointCount)
                     .Select(index => new PointVector(reader))
                     .ToArray();

            if (!reader.EndOfStream())
            {
                throw new UnknownFormatShapeException();
            }
        }
    }
}
