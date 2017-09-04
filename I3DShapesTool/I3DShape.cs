namespace I3DShapesTool
{
    class I3DShape
    {
        public uint Unknown1 { get; }

        public string Name { get; }

        public ushort ShapeId { get; }

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

        public I3DShape(BigEndianBinaryReader br)
        {
            Unknown1 = br.ReadUInt32();
            Name = br.BaseStream.ReadNullTerminatedString();

            br.BaseStream.Align(2); // Align the stream to short

            //This is pretty ugly, but they pretty much zero-pad after the alignment
            //So we read the padding until we found the shapeid
            do
            {
                ShapeId = br.ReadUInt16();
            } while (ShapeId == 0);

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

            Triangles = new I3DTri[VertexCount / 3];
            for (int i = 0; i < VertexCount / 3; i++)
            {
                Triangles[i] = new I3DTri(br);
            }

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

            UVs = new I3DUV[UvCount];
            for (int i = 0; i < UvCount; i++)
            {
                UVs[i] = new I3DUV(br);
            }
        }
    }
}