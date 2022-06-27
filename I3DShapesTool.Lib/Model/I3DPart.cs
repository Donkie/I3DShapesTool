using System.IO;
using System.Text;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public class I3DPart
    {
        /// <summary>
        /// The EntityType that 
        /// </summary>
        public virtual EntityType Type => EntityType.Unknown;

        /// <summary>
        /// Shape name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Shape ID
        /// </summary>
        public uint Id { get; private set; }

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
            byte[] nameBytes = Encoding.ASCII.GetBytes(Name);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            writer.Align(4);
            writer.Write(Id);
        }

        public void Read(BinaryReader reader, short fileVersion)
        {
            ReadHeader(reader);
            ReadContents(reader, fileVersion);
        }

        public void Write(BinaryWriter writer, short fileVersion)
        {
            WriteHeader(writer);
            WriteContents(writer, fileVersion);
        }

        protected virtual void ReadContents(BinaryReader reader, short fileVersion)
        {
            contents = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }

        protected virtual void WriteContents(BinaryWriter writer, short fileVersion)
        {
            writer.Write(contents);
        }

        public override string ToString()
        {
            return $"I3DPart #{Id} {Name}";
        }
    }
}
