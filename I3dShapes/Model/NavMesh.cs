using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Exceptions;
using I3dShapes.Model.Primitive;
using I3dShapes.Tools.Extensions;

namespace I3dShapes.Model
{
    public class NavMesh : NamedShapeObject
    {
        public NavMesh(BinaryReader reader)
            : base(ShapeType.NavMesh)
        {
            Load(reader);
        }

        public float Unknow1 { get; private set; }
        public float Unknow2 { get; private set; }
        public float Unknow3 { get; private set; }
        public float Unknow4 { get; private set; }
        public float Unknow5 { get; private set; }
        public float Unknow6 { get; private set; }
        public float Unknow7 { get; private set; }
        public float Unknow8 { get; private set; }

        public uint CountPoints { get; private set; }
        public uint CountIndexis { get; private set; }
        public uint Unknown11 { get; private set; }

        public ICollection<PointVector> Vectors { get; private set; }
        public ICollection<QuantitativeIndex> Indexis { get; private set; }

        private new void Load(BinaryReader reader)
        {
            base.Load(reader);
            Unknow1 = reader.ReadSingle();
            Unknow2 = reader.ReadSingle();
            Unknow3 = reader.ReadSingle();
            Unknow4 = reader.ReadSingle();
            Unknow5 = reader.ReadSingle();
            Unknow6 = reader.ReadSingle();
            Unknow7 = reader.ReadSingle();
            Unknow8 = reader.ReadSingle();

            CountPoints = reader.ReadUInt32();
            CountIndexis = reader.ReadUInt32();
            Unknown11 = reader.ReadUInt32();

            // ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
            if (CountPoints > Int32.MaxValue)
            {
                throw new UnknownFormatShapeException();
            }
            if (CountIndexis > Int32.MaxValue)
            {
                throw new UnknownFormatShapeException();
            }
            // ReSharper restore BuiltInTypeReferenceStyleForMemberAccess

            Vectors = Enumerable.Range(0, (int)CountPoints)
                                .Select(v => new PointVector(reader))
                                .ToArray();

            Indexis = Enumerable.Range(0, (int)CountIndexis)
                                .Select(v => new QuantitativeIndex(reader))
                                .ToArray();

            if (!reader.EndOfStream())
            {
                throw new UnknownFormatShapeException();
            }
        }
    }
}
