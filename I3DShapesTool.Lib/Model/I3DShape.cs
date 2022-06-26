using System;
using System.IO;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShape : I3DPart
    {
        private const int VERSION_WITH_TANGENTS = 5;

        public float BoundingVolumeX { get; private set; }
        public float BoundingVolumeY { get; private set; }
        public float BoundingVolumeZ { get; private set; }
        public float BoundingVolumeR { get; private set; }

        /// <summary>
        /// Number of triangle corners
        /// </summary>
        public uint CornerCount { get; private set; }
        public uint NumSubsets { get; private set; }
        /// <summary>
        /// Number of unique vertices
        /// </summary>
        public uint VertexCount { get; private set; }
        public I3DShapeOptions Options { get; private set; }
        public I3DShapeSubset[] Subsets { get; private set; }
        public bool ZeroBasedIndicesInRawData { get; private set; }
        public I3DTri[] Triangles { get; private set; }
        public I3DVector[] Positions { get; private set; }
        public I3DVector[]? Normals { get; private set; }
        public I3DVector4[]? Tangents { get; private set; }
        public I3DUV[][] UVSets { get; private set; }
        public I3DVector4[]? VertexColor { get; private set; }
        public float[,]? BlendWeights { get; private set; }
        public byte[,]? BlendIndices { get; private set; }
        public float[]? GenericData { get; private set; }
        public I3DShapeAttachment[] Attachments { get; private set; }

#nullable disable
        public I3DShape(byte[] rawData, Endian endian, int version)
            : base(EntityType.Shape, rawData, endian, version)
        {
        }
#nullable restore

        protected override void ReadContents(BinaryReader reader)
        {
            BoundingVolumeX = reader.ReadSingle();
            BoundingVolumeY = reader.ReadSingle();
            BoundingVolumeZ = reader.ReadSingle();
            BoundingVolumeR = reader.ReadSingle();
            CornerCount = reader.ReadUInt32();
            NumSubsets = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            Options = (I3DShapeOptions)reader.ReadUInt32();
            Subsets = new I3DShapeSubset[NumSubsets];
            for(int i = 0; i < NumSubsets; i++)
            {
                Subsets[i] = new I3DShapeSubset(reader, Version, Options);
            }

            ZeroBasedIndicesInRawData = false;
            Triangles = new I3DTri[CornerCount / 3];
            for(int i = 0; i < CornerCount / 3; i++)
            {
                Triangles[i] = new I3DTri(reader, VertexCount > (ushort.MaxValue + 1));

                if(Triangles[i].P1Idx == 0 || Triangles[i].P2Idx == 0 || Triangles[i].P3Idx == 0)
                    ZeroBasedIndicesInRawData = true;
            }

            // Convert to 1-based indices if it's detected that it is a zero-based index
            if(ZeroBasedIndicesInRawData)
            {
                foreach(I3DTri t in Triangles)
                {
                    t.P1Idx += 1;
                    t.P2Idx += 1;
                    t.P3Idx += 1;
                }
            }

            // TODO: figure out the exact logic for this
            //if (Version < 4) // Could be 5 as well
            reader.BaseStream.Align(4);

            Positions = new I3DVector[VertexCount];
            for(int i = 0; i < VertexCount; i++)
            {
                Positions[i] = new I3DVector(reader);
            }

            if(Options.HasFlag(I3DShapeOptions.HasNormals))
            {
                Normals = new I3DVector[VertexCount];
                for(int i = 0; i < VertexCount; i++)
                {
                    Normals[i] = new I3DVector(reader);
                }
            }

            if(Version >= VERSION_WITH_TANGENTS && Options.HasFlag(I3DShapeOptions.HasTangents))
            {
                Tangents = new I3DVector4[VertexCount];
                for(int i = 0; i < VertexCount; i++)
                {
                    Tangents[i] = new I3DVector4(reader);
                }
            }

            UVSets = new I3DUV[4][];
            for(int uvSet = 0; uvSet < 4; uvSet++)
            {
                if(Options.HasFlag((I3DShapeOptions)((uint)I3DShapeOptions.HasUV1 << uvSet)))
                {
                    I3DUV[] uvs = new I3DUV[VertexCount];
                    for(int i = 0; i < VertexCount; i++)
                    {
                        uvs[i] = new I3DUV(reader, Version);
                    }
                    UVSets[uvSet] = uvs;
                }
            }

            if(Options.HasFlag(I3DShapeOptions.HasVertexColor))
            {
                VertexColor = new I3DVector4[VertexCount];
                for(int i = 0; i < VertexCount; i++)
                {
                    VertexColor[i] = new I3DVector4(reader);
                }
            }

            if(Options.HasFlag(I3DShapeOptions.HasSkinningInfo))
            {
                bool singleBlendWeights = Options.HasFlag(I3DShapeOptions.SingleBlendWeights);

                // based on how 3D engines usually do skeletal/skinned meshes, you always have 4 weights and 4 indices per vertex

                BlendWeights = new float[VertexCount, 4];

                int numIndices;

                // Load blend weights first, if necessary
                // usually all weights per vertex sum up to 1.0, which means, if you only have one index per vertex, the first weight would always have to be 1
                if (singleBlendWeights)
                {
                    numIndices = 1;

                    for(int i = 0; i < VertexCount; i++)
                    {
                        BlendWeights[i, 0] = 1.0f;
                    }
                }
                else
                {
                    numIndices = 4;

                    for(int i = 0; i < VertexCount; i++)
                    {
                        for(int j = 0; j < 4; j++)
                        {
                            BlendWeights[i, j] = reader.ReadSingle();
                        }
                    }
                }

                // Load blend indices
                BlendIndices = new byte[VertexCount, numIndices];

                for(int i = 0; i < VertexCount; i++)
                {
                    for(int j = 0; j < numIndices; j++)
                    {
                        BlendIndices[i, j] = reader.ReadByte();
                    }
                }
            }

            if(Options.HasFlag(I3DShapeOptions.HasGeneric))
            {
                GenericData = new float[VertexCount];
                for(int i = 0; i < VertexCount; i++)
                {
                    GenericData[i] = reader.ReadSingle();
                }
            }

            /*
            TODO: Implement this if necessary
            if (Version < 5 && !Options.HasFlag(I3DShapeOptions.HasTangents))
            {
                Tangents = ComputeTangents(Vertices, Normals, FirstNonNullUVMap);
                // algorithm is the same as https://github.com/mrdoob/three.js/blob/master/src/core/BufferGeometry.js#L472
            }
            */

            /*
            TODO: Implement this if necessary
            if (Version < 6)
            {
                foreach(var extra in SubSets)
                {
                    // Loop over the 4 UV floats in extra and set them based on some math done on the vertex and UV set data
                }
            }
            */

            uint numAttachments = reader.ReadUInt32();
            Attachments = new I3DShapeAttachment[numAttachments];
            for(int i = 0; i < numAttachments; i++)
            {
                Attachments[i] = new I3DShapeAttachment(reader);
            }

            if(reader.BaseStream.Position != reader.BaseStream.Length)
                throw new DecodeException("Failed to read the entire shape data");
        }

        protected override void WriteContents(BinaryWriter writer)
        {
            writer.Write(BoundingVolumeX);
            writer.Write(BoundingVolumeY);
            writer.Write(BoundingVolumeZ);
            writer.Write(BoundingVolumeR);
            writer.Write(CornerCount);
            writer.Write((uint)Subsets.Length);
            writer.Write(VertexCount);
            writer.Write((uint)Options);
            foreach(I3DShapeSubset subset in Subsets)
                subset.Write(writer, Version, Options);

            if(Triangles.Length != CornerCount / 3)
                throw new InvalidOperationException("Triangles array must be of size CornerCount / 3");

            foreach(I3DTri tri in Triangles)
                tri.Write(writer, ZeroBasedIndicesInRawData, VertexCount > (ushort.MaxValue + 1));

            writer.Align(4);

            if(Positions.Length != VertexCount)
                throw new InvalidOperationException("Positions array must be of size VertexCount");

            foreach(I3DVector pos in Positions)
                pos.Write(writer);

            if(Options.HasFlag(I3DShapeOptions.HasNormals))
            {
                if(Normals == null)
                    throw new InvalidOperationException("Options say we have normals but Normals field is null");

                if(Normals.Length != VertexCount)
                    throw new InvalidOperationException("Normals array must be of size VertexCount");

                foreach(I3DVector norm in Normals)
                    norm.Write(writer);
            }

            if(Version >= VERSION_WITH_TANGENTS && Options.HasFlag(I3DShapeOptions.HasTangents))
            {
                if(Tangents == null)
                    throw new InvalidOperationException("Options say we have 4d data but Tangents field is null");

                if(Tangents.Length != VertexCount)
                    throw new InvalidOperationException("Tangents array must be of size VertexCount");

                foreach(I3DVector4 vec in Tangents)
                    vec.Write(writer);
            }

            for(int uvSet = 0; uvSet < 4; uvSet++)
            {
                if(Options.HasFlag((I3DShapeOptions)((uint)I3DShapeOptions.HasUV1 << uvSet)))
                {
                    if(UVSets[uvSet] == null)
                        throw new InvalidOperationException($"Options say we have UV set {uvSet + 1} but UVSets[{uvSet}] is null");

                    if(UVSets[uvSet].Length != VertexCount)
                        throw new InvalidOperationException($"UVSets[{uvSet}] array must be of size VertexCount");

                    foreach(I3DUV uv in UVSets[uvSet])
                        uv.Write(writer, Version);
                }
            }

            if(Options.HasFlag(I3DShapeOptions.HasVertexColor))
            {
                if(VertexColor == null)
                    throw new InvalidOperationException("Options say we have vertex colors but VertexColor field is null");

                if(VertexColor.Length != VertexCount)
                    throw new InvalidOperationException("VertexColor array must be of size VertexCount");

                foreach(I3DVector4 vec in VertexColor)
                    vec.Write(writer);
            }

            if(Options.HasFlag(I3DShapeOptions.HasSkinningInfo))
            {
                if(BlendIndices == null)
                    throw new InvalidOperationException("Options say we have skinning info but BlendIndices is null");

                bool noBlendWeights = Options.HasFlag(I3DShapeOptions.SingleBlendWeights);
                if(!noBlendWeights)
                {
                    if(BlendWeights == null)
                        throw new InvalidOperationException("Options say we have blend weights but BlendWeights field is null");

                    if(BlendWeights.GetLength(0) != VertexCount)
                        throw new InvalidOperationException("First dimension of BlendWeights array must be of size VertexCount");

                    if(BlendWeights.GetLength(1) != 4)
                        throw new InvalidOperationException("Second dimension of BlendWeights array must be of size 4");

                    for(int i = 0; i < BlendWeights.GetLength(0); i++)
                    {
                        for(int j = 0; j < BlendWeights.GetLength(1); j++)
                        {
                            writer.Write(BlendWeights[i, j]);
                        }
                    }
                }

                if(BlendIndices.GetLength(0) != VertexCount)
                    throw new InvalidOperationException("First dimension of BlendIndices array must be of size VertexCount");

                if(noBlendWeights && BlendIndices.GetLength(1) != 1)
                    throw new InvalidOperationException("Second dimension of BlendIndices array must be of size 1 if there are no blend weights");
                else if(!noBlendWeights && BlendIndices.GetLength(1) != 4)
                    throw new InvalidOperationException("Second dimension of BlendIndices array must be of size 4 if there are blend weights");

                for(int i = 0; i < BlendIndices.GetLength(0); i++)
                {
                    for(int j = 0; j < BlendIndices.GetLength(1); j++)
                    {
                        writer.Write(BlendIndices[i, j]);
                    }
                }
            }

            if(Options.HasFlag(I3DShapeOptions.HasGeneric))
            {
                if(GenericData == null)
                    throw new InvalidOperationException("Options say we have UnknownData2 but UnknownData2 field is null");

                if(GenericData.Length != VertexCount)
                    throw new InvalidOperationException("UnknownData2 array must be of size VertexCount");

                foreach(float v in GenericData)
                    writer.Write(v);
            }

            writer.Write((uint)Attachments.Length);
            foreach(I3DShapeAttachment data in Attachments)
                data.Write(writer);
        }

        public override string ToString()
        {
            return $"I3DShape #{Id} V{Version} {Name}";
        }
    }
}