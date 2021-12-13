using System.IO;
using System.Text;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public abstract class I3DPart
    {
#nullable disable
        protected I3DPart(ShapeType type, byte[] rawData, Endian endian, int version)
        {
            Type = type;
            RawData = rawData;
            Endian = endian;
            Version = version;
        }
#nullable restore

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

        private void ReadHeader(BinaryReader reader)
        {
            var nameLength = reader.ReadInt32();
            Name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
            reader.Align(4);
            Id = reader.ReadUInt32();
        }

        private void WriteHeader(BinaryWriter writer)
        {
            var nameBytes = Encoding.ASCII.GetBytes(Name);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            writer.Align(4);
            writer.Write(Id);
        }

        protected void Read(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadContents(reader);
        }

        protected void ReadFromRawData()
        {
            using var stream = new MemoryStream(RawData);
            using var reader = new EndianBinaryReader(stream, Endian);
            Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            WriteHeader(writer);
            WriteContents(writer);
        }

        protected abstract void ReadContents(BinaryReader reader);

        protected abstract void WriteContents(BinaryWriter writer);

        public override string ToString()
        {
            return $"I3DPart #{Id} V{Version} {Name}";
        }
    }
}
