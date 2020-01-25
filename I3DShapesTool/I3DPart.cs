using System.IO;

namespace I3DShapesTool
{
    public class I3DPart
    {
        protected I3DPart(I3DPart part)
            : this(part.RawType, part.Type, part.Name, part.RawData, part.Endian, part.Version)
        {
        }

        public I3DPart(int rawType, I3DPartType type, string name, byte[] rawData, Endian endian, int version)
        {
            RawType = rawType;
            Type = type;
            Name = name;
            RawData = rawData;
            Endian = endian;
            Version = version;
        }

        public int RawType { get; }
        public I3DPartType Type { get; }
        public string Name { get; protected set; }
        public uint ShapeId { get; protected set; }

        public byte[] RawData { get; }
        public Endian Endian { get; }
        public int Version { get; }

        public static I3DPart Read(Stream stream, Endian fileEndian, int version)
        {
            var type = stream.ReadInt32(fileEndian);
            var size = stream.ReadInt32(fileEndian);
            var data = stream.ReadBytes(size);

            string name = null;
            uint id = 0;

            using(var streamData = new MemoryStream(data))
            using (var br = fileEndian == Endian.Big?new BigEndianBinaryReader(streamData) :new BinaryReader(streamData))
            {
                var nameLength = (int)br.ReadUInt32();
                name = System.Text.Encoding.ASCII.GetString(br.ReadBytes(nameLength));

                br.BaseStream.Align(4); // Align the stream to short

                id = br.ReadUInt32();
            }
            var partType = GetPartType(type);
            return new I3DPart(type, partType, name, data, fileEndian, version)
            {
                Name = name,
                ShapeId = id,
            };
        }

        private static I3DPartType GetPartType(int type)
        {
            switch (type)
            {
                case 1:
                    //case 4:
                    //case 5:
                    return I3DPartType.Shape;
                case 2:
                    return I3DPartType.Spline;
                default:
                    return I3DPartType.Unknown;
            }
        }
    }
}
