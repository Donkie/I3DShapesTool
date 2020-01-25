using System.IO;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool
{
    public class I3DShape
    {
        private readonly ILogger _logger;

        public int Type { get; }

        public int Size { get; }

        public byte[] RawBytes { get; }

        public string Name { get; private set; }

        public uint ShapeId { get; private set; }
        
        public float BoundingVolumeX { get; private set; }

        public float BoundingVolumeY { get; private set; }

        public float BoundingVolumeZ { get; private set; }

        public float BoundingVolumeR { get; private set; }

        public uint VertexCount { get; private set; }

        public uint Unknown6 { get; private set; }

        public uint Vertices { get; private set; }

        public uint Unknown7 { get; private set; }

        public uint Unknown8 { get; private set; }

        public uint UvCount { get; private set; }

        public uint Unknown9 { get; private set; }

        public uint VertexCount2 { get; private set; }

        public I3DTri[] Triangles { get; private set; }

        public I3DVector[] Positions { get; private set; }

        public I3DVector[] Normals { get; private set; }

        public I3DUV[] UVs { get; private set; }

        public I3DShape(int type, int size, byte[] rawBytes, ILogger logger = null)
        {
            Type = type;
            Size = size;
            RawBytes = rawBytes;

            _logger = logger;
        }

        public void Load(BinaryReader br, int fileVersion)
        {
            var nameLength = (int) br.ReadUInt32();
            Name = System.Text.Encoding.ASCII.GetString(br.ReadBytes(nameLength));
            
            br.BaseStream.Align(4); // Align the stream to short
            
            ShapeId = br.ReadUInt32();

            BoundingVolumeX = br.ReadSingle();
            BoundingVolumeY = br.ReadSingle();
            BoundingVolumeZ = br.ReadSingle();
            BoundingVolumeR = br.ReadSingle();
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
            for (var i = 0; i < VertexCount / 3; i++)
            {
                Triangles[i] = new I3DTri(br);

                if (Triangles[i].P1Idx == 0 || Triangles[i].P2Idx == 0 || Triangles[i].P3Idx == 0)
                    isZeroBased = true;
            }
            
            // Convert to 1-based indices if it's detected that it is a zero-based index
            if (isZeroBased)
            {
                _logger?.LogDebug("Shape has zero-based face indices");
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
            for (var i = 0; i < Vertices; i++)
            {
                Positions[i] = new I3DVector(br);
            }

            Normals = new I3DVector[Vertices];
            for (var i = 0; i < Vertices; i++)
            {
                Normals[i] = new I3DVector(br);
            }

            if (fileVersion >= 4) // Could be 5 as well
            {
                var bytesLeft = br.BaseStream.Length - br.BaseStream.Position;
                var unknownBytes = bytesLeft - UvCount * 2 * 4;
                if (unknownBytes > 4)
                {
                    br.BaseStream.Seek(unknownBytes, SeekOrigin.Current);
                }
            }

            UVs = new I3DUV[UvCount];
            for (var i = 0; i < UvCount; i++)
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