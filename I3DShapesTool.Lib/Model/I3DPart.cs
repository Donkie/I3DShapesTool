using System.IO;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public abstract class I3DPart
    {
        protected I3DPart(ShapeType type, byte[] rawData, Endian endian, int version)
        {
            Type = type;
            RawData = rawData;
            Endian = endian;
            Version = version;
        }

        public ShapeType Type { get; protected set; }

        /// <summary>
        /// Shape name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Shape ID
        /// </summary>
        public uint Id { get; protected set; }

        /// <summary>
        /// Shape Raw Data 
        /// </summary>
        public byte[] RawData { get; protected set; }

        public Endian Endian { get; protected set; }

        public int Version { get; protected set; }

        protected void Load()
        {
            using (var stream = new MemoryStream(RawData))
            using (var reader = new EndianBinaryReader(stream, Endian))
            {
                var nameLength = reader.ReadInt32();
                Name = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
                reader.Align(4);
                Id = reader.ReadUInt32();

                Load(reader);
            }
        }

        protected abstract void Load(BinaryReader reader);
    }
}
