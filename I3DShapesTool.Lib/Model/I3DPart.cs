using System.IO;
using System.Text;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public class I3DPart
    {
#nullable disable
        /// <summary>
        /// Construct I3DPart with a known part type.
        /// Should only be used by child classes, since if you know of a type you should have a child class for it.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="rawData">Raw binary data</param>
        /// <param name="endian">Endian</param>
        /// <param name="version">File version</param>
        protected I3DPart(EntityType type, byte[] rawData, Endian endian, int version)
        {
            Type = type;
            RawData = rawData;
            Endian = endian;
            Version = version;
            RawType = (int)type;

            ReadFromRawData();
        }

        /// <summary>
        /// Construct I3DPart with unknown part type.
        /// Type will get set to ShapeType.Unknown
        /// </summary>
        /// <param name="rawType">Raw type number</param>
        /// <param name="rawData">Raw binary data</param>
        /// <param name="endian">Endian</param>
        /// <param name="version">File version</param>
        public I3DPart(int rawType, byte[] rawData, Endian endian, int version) : this(EntityType.Unknown, rawData, endian, version)
        {
            RawType = rawType;
        }
#nullable restore

        public EntityType Type { get; }

        public int RawType { get; }

        /// <summary>
        /// Shape name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Shape ID
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Shape Raw Data 
        /// </summary>
        public byte[] RawData { get; }

        public Endian Endian { get; }

        public int Version { get; }

        private byte[] contents;

        private void ReadHeader(BinaryReader reader)
        {
            int nameLength = reader.ReadInt32();
            Name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
            reader.Align(4);
            Id = reader.ReadUInt32();
        }

        private void WriteHeader(BinaryWriter writer)
        {
            byte[]? nameBytes = Encoding.ASCII.GetBytes(Name);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            writer.Align(4);
            writer.Write(Id);
        }

        private void Read(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadContents(reader);
        }

        private void ReadFromRawData()
        {
            using MemoryStream? stream = new MemoryStream(RawData);
            using EndianBinaryReader? reader = new EndianBinaryReader(stream, Endian);
            Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            WriteHeader(writer);
            WriteContents(writer);
        }

        protected virtual void ReadContents(BinaryReader reader)
        {
            contents = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }

        protected virtual void WriteContents(BinaryWriter writer)
        {
            writer.Write(contents);
        }

        public override string ToString()
        {
            return $"I3DPart #{Id} V{Version} {Name}";
        }
    }
}
