using System.IO;
using I3DShapesTool.Lib.Export;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShape : I3DPart
    {
        private readonly ILogger _logger;

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

        public I3DShape(byte[] rawData, Endian endian, int version)
            : base(ShapeType.Shape, rawData, endian, version)
        {
            Load();
        }

        protected override void Load(BinaryReader reader)
        {
            BoundingVolumeX = reader.ReadSingle();
            BoundingVolumeY = reader.ReadSingle();
            BoundingVolumeZ = reader.ReadSingle();
            BoundingVolumeR = reader.ReadSingle();
            VertexCount = reader.ReadUInt32();
            Unknown6 = reader.ReadUInt32();
            Vertices = reader.ReadUInt32();
            Unknown7 = reader.ReadUInt32();
            Unknown8 = reader.ReadUInt32();
            UvCount = reader.ReadUInt32();
            Unknown9 = reader.ReadUInt32();
            VertexCount2 = reader.ReadUInt32();

            var isZeroBased = false;
            Triangles = new I3DTri[VertexCount / 3];
            for (var i = 0; i < VertexCount / 3; i++)
            {
                Triangles[i] = new I3DTri(reader);

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

            //if (Version < 4) // Could be 5 as well
                reader.BaseStream.Align(4);

            Positions = new I3DVector[Vertices];
            for (var i = 0; i < Vertices; i++)
            {
                Positions[i] = new I3DVector(reader);
            }

            Normals = new I3DVector[Vertices];
            for (var i = 0; i < Vertices; i++)
            {
                Normals[i] = new I3DVector(reader);
            }

            if (Version >= 4) // Could be 5 as well
            {
                var bytesLeft = reader.BaseStream.Length - reader.BaseStream.Position;
                var unknownBytes = bytesLeft - UvCount * 2 * 4;
                if (unknownBytes > 4)
                {
                    reader.BaseStream.Seek(unknownBytes, SeekOrigin.Current);
                }
            }

            UVs = new I3DUV[UvCount];
            for (var i = 0; i < UvCount; i++)
            {
                UVs[i] = new I3DUV(reader, Version);
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