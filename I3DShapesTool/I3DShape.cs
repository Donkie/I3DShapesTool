using System.IO;

namespace I3DShapesTool
{
    class I3DShape
    {
        public uint Unknown1 { get; }

        public string Name { get; }

        public uint ShapeId { get; }

        public float Unknown2 { get; }

        public float Unknown3 { get; }

        public float Unknown4 { get; }

        public float Unknown5 { get; }

        public uint VertexCount { get; }

        public uint Unknown6 { get; }

        public uint Vertices { get; }

        public uint Unknown7 { get; }

        public uint Unknown8 { get; }

        public uint UvCount { get; }

        public uint Unknown9 { get; }

        public uint VertexCount2 { get; }

        public I3DTri[] Triangles { get; }

        public I3DVector[] Positions { get; }

        public I3DVector[] Normals { get; }

        public I3DUV[] UVs { get; }

        public I3DShape(BinaryReader br, int fileVersion)
        {
            int nameLength = (int) br.ReadUInt32();
            Name = System.Text.Encoding.ASCII.GetString(br.ReadBytes(nameLength));
            //Name = br.ReadString();
            //Name = br.BaseStream.ReadNullTerminatedString();

            br.BaseStream.Align(4); // Align the stream to short

            //This is pretty ugly, but they pretty much zero-pad after the alignment
            //So we read the padding until we found the shapeid
            //do
            //{
            //} while (ShapeId == 0);
            ShapeId = br.ReadUInt32();

            Unknown2 = br.ReadSingle();
            Unknown3 = br.ReadSingle();
            Unknown4 = br.ReadSingle();
            Unknown5 = br.ReadSingle();
            VertexCount = br.ReadUInt32();
            Unknown6 = br.ReadUInt32();
            Vertices = br.ReadUInt32();
            Unknown7 = br.ReadUInt32();
            Unknown8 = br.ReadUInt32();
            UvCount = br.ReadUInt32();
            Unknown9 = br.ReadUInt32();
            VertexCount2 = br.ReadUInt32();

            var isZeroBased = false;
            Triangles = new I3DTri[VertexCount / 3];
            for (int i = 0; i < VertexCount / 3; i++)
            {
                Triangles[i] = new I3DTri(br);

                if (Triangles[i].P1Idx == 0 || Triangles[i].P2Idx == 0 || Triangles[i].P3Idx == 0)
                    isZeroBased = true;
            }
            
            // Convert to 1-based indices if it's detected that it is a zero-based index
            if (isZeroBased)
            {
                foreach (var t in Triangles)
                {
                    t.P1Idx += 1;
                    t.P2Idx += 1;
                    t.P3Idx += 1;
                }
            }

            if(fileVersion < 4) // Could be 5 as well
                br.BaseStream.Align(4);

            Positions = new I3DVector[Vertices];
            for (int i = 0; i < Vertices; i++)
            {
                Positions[i] = new I3DVector(br);
            }

            Normals = new I3DVector[Vertices];
            for (int i = 0; i < Vertices; i++)
            {
                Normals[i] = new I3DVector(br);
            }

            if (fileVersion >= 4) // Could be 5 as well
            {
                long bytesLeft = br.BaseStream.Length - br.BaseStream.Position;
                long unknownBytes = bytesLeft - UvCount * 2 * 4;
                if (unknownBytes > 4)
                {
                    br.BaseStream.Seek(unknownBytes, SeekOrigin.Current);
                }
            }

            UVs = new I3DUV[UvCount];
            for (int i = 0; i < UvCount; i++)
            {
                UVs[i] = new I3DUV(br, fileVersion);
            }
        }

        public WavefrontObj ToObj()
        {
            var geomname = Name;
            if (geomname.EndsWith("Shape"))
                geomname = geomname.Substring(0, geomname.Length - 5);

            return new WavefrontObj
            {
                GeometryName = geomname,
                Positions = Positions,
                Normals = Normals,
                UVs = UVs,
                Triangles = Triangles
            };
        }
    }
}