using System;
using System.IO;

namespace I3dShapes.Model
{
    /// <summary>
    /// Addition content in shape type 1.
    /// </summary>
    public class AdditionContainer
    {
        public AdditionContainer(BinaryReader reader)
        {
            Load(reader);
        }

        public uint Type { get; private set; }

        public byte[] RawData { get; private set; }

        private void Load(BinaryReader reader)
        {
            Type = reader.ReadUInt32();
            var size = reader.ReadUInt32();

            // ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
            if (size > Int32.MaxValue)
                // ReSharper restore BuiltInTypeReferenceStyleForMemberAccess
            {
                throw new Exception("size > Int32.MaxValue");
            }

            RawData = reader.ReadBytes((int)size);
        }
    }
}
