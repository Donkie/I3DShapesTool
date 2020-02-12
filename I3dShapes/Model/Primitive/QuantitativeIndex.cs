using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Exceptions;

namespace I3dShapes.Model.Primitive
{
    public class QuantitativeIndex
    {
        public QuantitativeIndex(BinaryReader reader)
        {
            Load(reader);
        }

        public uint Count { get; private set; }

        public ICollection<PairIndex> Indexis { get; private set; }

        private void Load(BinaryReader reader)
        {
            Count = reader.ReadUInt32();

            // ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
            if (Count > Int32.MaxValue)
            {
                throw new UnknownFormatShapeException();
            }
            // ReSharper restore BuiltInTypeReferenceStyleForMemberAccess

            Indexis = Enumerable.Range(0, (int)Count)
                                .Select(v => new PairIndex(reader))
                                .ToArray();
        }
    }
}
