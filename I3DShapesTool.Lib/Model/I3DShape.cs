using System;
using System.IO;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShape : I3DPart
    {
        private const int VERSION_PRECOMPUTED4DDATA = 4; // Not sure of exact version here

        public I3DVector4 BoundingVolume { get; private set; }

        /// <summary>
        /// Number of triangle corners
        /// </summary>
        public uint CornerCount => (uint)(Triangles.Length * 3);
        /// <summary>
        /// Number of unique vertices
        /// </summary>
        public uint VertexCount => (uint)Positions.Length;
        public I3DShapeExtra[] ExtraStuff { get; private set; }
        public bool ZeroBasedIndicesInRawData { get; private set; }
        public I3DTri[] Triangles { get; private set; }
        public I3DVector[] Positions { get; private set; }
        public I3DVector[]? Normals { get; private set; }
        public I3DVector4[]? Some4DData { get; private set; }
        public I3DUV[][] UVSets { get; private set; }
        public I3DVector4[]? VertexColor { get; private set; }
        public float[,]? BlendWeights { get; private set; }
        public byte[,]? BlendIndices { get; private set; }
        public float[]? UnknownData2 { get; private set; }
        public I3DShapeExtra2[] UnknownData3 { get; private set; }

        /// <summary>
        /// Contains options flags higher than we have enum data for
        /// This is so we can store and these unknown options on load and then save them again
        /// </summary>
        public uint OptionsHighBits { get; private set; }

        /// <summary>
        /// Dynamically generated options bitflag
        /// </summary>
        public I3DShapeOptions Options { 
            get {
                I3DShapeOptions opts = I3DShapeOptions.None;

                if(Normals != null)
                    opts |= I3DShapeOptions.HasNormals;

                for(int uvSet = 0; uvSet < 4; uvSet++)
                {
                    if(UVSets[uvSet] != null)
                        opts |= (I3DShapeOptions)((uint)I3DShapeOptions.HasUV1 << uvSet);
                }

                if(VertexColor != null)
                    opts |= I3DShapeOptions.HasVertexColor;

                if(BlendIndices != null)
                    opts |= I3DShapeOptions.HasSkinningInfo;

                if(BlendIndices != null && BlendWeights == null)
                    opts |= I3DShapeOptions.NoBlendWeights;

                if(UnknownData2 != null)
                    opts |= I3DShapeOptions.HasUnknownData2;

                if(Some4DData != null)
                    opts |= I3DShapeOptions.HasPrecomputed4DVectorData;

                opts |= (I3DShapeOptions)OptionsHighBits;

                return opts;
            } 
        }

#nullable disable
        public I3DShape(byte[] rawData, Endian endian, int version)
            : base(EntityType.Shape, rawData, endian, version)
        {
        }
#nullable restore

        protected override void ReadContents(BinaryReader reader)
        {
            BoundingVolume = new I3DVector4(reader);
            uint cornerCount = reader.ReadUInt32();
            uint numExtraStuff = reader.ReadUInt32();
            uint vertexCount = reader.ReadUInt32();

            uint options_num = reader.ReadUInt32();
            I3DShapeOptions options = (I3DShapeOptions)options_num;
            OptionsHighBits = options_num & ~(uint)I3DShapeOptions.All;

            ExtraStuff = new I3DShapeExtra[numExtraStuff];
            for(int i = 0; i < numExtraStuff; i++)
            {
                ExtraStuff[i] = new I3DShapeExtra(reader, Version, options);
            }

            ZeroBasedIndicesInRawData = false;
            Triangles = new I3DTri[cornerCount / 3];
            for(int i = 0; i < cornerCount / 3; i++)
            {
                Triangles[i] = new I3DTri(reader, vertexCount > (ushort.MaxValue + 1));

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

            Positions = new I3DVector[vertexCount];
            for(int i = 0; i < vertexCount; i++)
            {
                Positions[i] = new I3DVector(reader);
            }

            if(options.HasFlag(I3DShapeOptions.HasNormals))
            {
                Normals = new I3DVector[vertexCount];
                for(int i = 0; i < vertexCount; i++)
                {
                    Normals[i] = new I3DVector(reader);
                }
            }

            if(options.HasFlag(I3DShapeOptions.HasPrecomputed4DVectorData))
            {
                if(Version >= VERSION_PRECOMPUTED4DDATA)
                {
                    Some4DData = new I3DVector4[vertexCount];
                    for(int i = 0; i < vertexCount; i++)
                    {
                        Some4DData[i] = new I3DVector4(reader);
                    }
                }
                else
                {
                    // Stupid fix for this flag carrying some other meaning for older file versions
                    // We don't want to treat this file has having precomputed 4d vector data, but
                    // we still need to keep the flag for if we're saving later.
                    OptionsHighBits |= (uint)I3DShapeOptions.HasPrecomputed4DVectorData;
                }
            }

            UVSets = new I3DUV[4][];
            for(int uvSet = 0; uvSet < 4; uvSet++)
            {
                if(options.HasFlag((I3DShapeOptions)((uint)I3DShapeOptions.HasUV1 << uvSet)))
                {
                    I3DUV[] uvs = new I3DUV[vertexCount];
                    for(int i = 0; i < vertexCount; i++)
                    {
                        uvs[i] = new I3DUV(reader, Version);
                    }
                    UVSets[uvSet] = uvs;
                }
            }

            if(options.HasFlag(I3DShapeOptions.HasVertexColor))
            {
                VertexColor = new I3DVector4[vertexCount];
                for(int i = 0; i < vertexCount; i++)
                {
                    VertexColor[i] = new I3DVector4(reader);
                }
            }

            if(options.HasFlag(I3DShapeOptions.HasSkinningInfo))
            {
                bool noBlendWeights = options.HasFlag(I3DShapeOptions.NoBlendWeights);
                bool blendWeights = !noBlendWeights;

                // Load blend weights first
                if(blendWeights)
                {
                    BlendWeights = new float[vertexCount, 4];
                    for(int i = 0; i < BlendWeights.GetLength(0); i++)
                    {
                        for(int j = 0; j < BlendWeights.GetLength(1); j++)
                        {
                            BlendWeights[i, j] = reader.ReadSingle();
                        }
                    }
                }

                // Load blend indices
                // If weights exist, we will have 4 index slots per vertex, otherwise just 1.
                BlendIndices = new byte[vertexCount, blendWeights ? 4 : 1];
                for(int i = 0; i < BlendIndices.GetLength(0); i++)
                {
                    for(int j = 0; j < BlendIndices.GetLength(1); j++)
                    {
                        BlendIndices[i, j] = reader.ReadByte();
                    }
                }
            }

            if(options.HasFlag(I3DShapeOptions.HasUnknownData2))
            {
                UnknownData2 = new float[vertexCount];
                for(int i = 0; i < vertexCount; i++)
                {
                    UnknownData2[i] = reader.ReadSingle();
                }
            }

            /*
            TODO: Implement this if necessary
            if (Version < 5 && !Options.HasFlag(I3DShapeOptions.HasPrecomputed4DVectorData))
            {
                Some4DData = Compute4DData(Vertices, Normals, FirstNonNullUVMap);
            }
            */

            /*
            TODO: Implement this if necessary
            if (Version < 6)
            {
                foreach(var extra in ExtraStuff)
                {
                    // Loop over the 4 UV floats in extra and set them based on some math done on the vertex and UV set data
                }
            }
            */

            uint numUnknownData3 = reader.ReadUInt32();
            UnknownData3 = new I3DShapeExtra2[numUnknownData3];
            for(int i = 0; i < numUnknownData3; i++)
            {
                UnknownData3[i] = new I3DShapeExtra2(reader);
            }

            if(reader.BaseStream.Position != reader.BaseStream.Length)
                throw new DecodeException("Failed to read the entire shape data");
        }

        protected override void WriteContents(BinaryWriter writer)
        {
            BoundingVolume.Write(writer);
            writer.Write(CornerCount);
            writer.Write((uint)ExtraStuff.Length);
            writer.Write(VertexCount);
            writer.Write((uint)Options);
            foreach(I3DShapeExtra extra in ExtraStuff)
                extra.Write(writer, Version, Options);

            foreach(I3DTri tri in Triangles)
                tri.Write(writer, ZeroBasedIndicesInRawData, VertexCount > (ushort.MaxValue + 1));

            writer.Align(4);

            foreach(I3DVector pos in Positions)
                pos.Write(writer);

            if(Normals != null)
            {
                if(Normals.Length != VertexCount)
                    throw new InvalidOperationException("Normals array must be of size VertexCount");

                foreach(I3DVector norm in Normals)
                    norm.Write(writer);
            }

            if(Version < VERSION_PRECOMPUTED4DDATA && Some4DData != null)
                throw new InvalidOperationException($"I3D Entity version < {VERSION_PRECOMPUTED4DDATA} doesn't support \"Some4DData\"");

            if(Some4DData != null)
            {
                if(Some4DData.Length != VertexCount)
                    throw new InvalidOperationException("Some4DData array must be of size VertexCount");

                foreach(I3DVector4 vec in Some4DData)
                    vec.Write(writer);
            }

            for(int uvSet = 0; uvSet < 4; uvSet++)
            {
                if(UVSets[uvSet] != null)
                {
                    if(UVSets[uvSet].Length != VertexCount)
                        throw new InvalidOperationException($"UVSets[{uvSet}] array must be of size VertexCount");

                    foreach(I3DUV uv in UVSets[uvSet])
                        uv.Write(writer, Version);
                }
            }

            if(VertexColor != null)
            {
                if(VertexColor.Length != VertexCount)
                    throw new InvalidOperationException("VertexColor array must be of size VertexCount");

                foreach(I3DVector4 vec in VertexColor)
                    vec.Write(writer);
            }

            if(Options.HasFlag(I3DShapeOptions.HasSkinningInfo))
            {
                if(BlendIndices == null)
                    throw new InvalidOperationException("Options say we have skinning info but BlendIndices is null");

                bool noBlendWeights = BlendWeights == null;
                if(BlendWeights != null)
                {
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

            if(UnknownData2 != null)
            {
                if(UnknownData2.Length != VertexCount)
                    throw new InvalidOperationException("UnknownData2 array must be of size VertexCount");

                foreach(float v in UnknownData2)
                    writer.Write(v);
            }

            writer.Write((uint)UnknownData3.Length);
            foreach(I3DShapeExtra2 data in UnknownData3)
                data.Write(writer);
        }

        public override string ToString()
        {
            return $"I3DShape #{Id} V{Version} {Name}";
        }
    }
}