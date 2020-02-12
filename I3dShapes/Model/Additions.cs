using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace I3dShapes.Model
{
    public class Additions
    {
        public Additions(BinaryReader reader)
        {
            Load(reader);
        }

        public ICollection<AdditionContainer> AdditionList { get; private set; }

        private void Load(BinaryReader reader)
        {
            var count = reader.ReadUInt32();
            
            // ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
            if (count > Int32.MaxValue)
                // ReSharper restore BuiltInTypeReferenceStyleForMemberAccess
            {
                throw new Exception("count > Int32.MaxValue");
            }

            AdditionList = Enumerable.Range(0, (int) count)
                                     .Select(v => new AdditionContainer(reader))
                                     .ToArray();
        }
    }
}
