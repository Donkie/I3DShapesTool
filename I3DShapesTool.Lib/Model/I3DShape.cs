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

        public I3DVector4 BoundingVolume { get; private set; }

        /// <summary>
        /// Number of triangle corners
        /// </summary>
        public uint CornerCount => (uint)(Triangles.Length * 3);

        /// <summary>
        /// Number of unique vertices
        /// </summary>
        public uint VertexCount => (uint)Positions.Length;

        public I3DShapeSubset[] Subsets { get; set; } = new I3DShapeSubset[0];
        public I3DTri[] Triangles { get; set; } = new I3DTri[0];
        public I3DVector[] Positions { get; set; } = new I3DVector[0];
        public I3DVector[] Normals { get; set; }
        public I3DVector4[] Tangents { get; set; }
        public I3DUV[][] UVSets { get; set; } = new I3DUV[4][];
        public I3DVector4[] VertexColor { get; set; }
        public float[,] BlendWeights { get; set; }
        public byte[,] BlendIndices { get; set; }
        public float[] GenericData { get; set; }
        public I3DShapeAttachment[] Attachments { get; set; } = new I3DShapeAttachment[0];

        /// <summary>
        /// Contains options flags higher than we have enum data for
        /// This is so we can store and these unknown options on load and then save them again
        /// </summary>
        public uint OptionsHighBits { get; private set; }

        /// <summary>
        /// Dynamically generated options bitflag
        /// </summary>
        public I3DShapeOptions Options { 
            get
            {
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
                    opts |= I3DShapeOptions.SingleBlendWeights;

                if(GenericData != null)
                    opts |= I3DShapeOptions.HasGeneric;

                if(Tangents != null)
                    opts |= I3DShapeOptions.HasTangents;

                opts |= (I3DShapeOptions)OptionsHighBits;

                return opts;
            } 
        }

        public override EntityType Type => EntityType.Shape;

        protected override void ReadContents(BinaryReader reader, short fileVersion)
        {
            BoundingVolume = new I3DVector4(reader);
            uint cornerCount = reader.ReadUInt32();
            uint numSubsets = reader.ReadUInt32();
            uint vertexCount = reader.ReadUInt32();

            uint options_num = reader.ReadUInt32();
            I3DShapeOptions options = (I3DShapeOptions)options_num;
            OptionsHighBits = options_num & ~(uint)I3DShapeOptions.All;

            Subsets = new I3DShapeSubset[numSubsets];
            for(int i = 0; i < numSubsets; i++)
            {
                Subsets[i] = new I3DShapeSubset(reader, fileVersion, options);
            }

            Triangles = new I3DTri[cornerCount / 3];
            for(int i = 0; i < cornerCount / 3; i++)
            {
                Triangles[i] = new I3DTri(reader, vertexCount > (ushort.MaxValue + 1));
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

            if(options.HasFlag(I3DShapeOptions.HasTangents))
            {
                if(fileVersion >= VERSION_WITH_TANGENTS)
                {
                    Tangents = new I3DVector4[vertexCount];
                    for(int i = 0; i < vertexCount; i++)
                    {
                        Tangents[i] = new I3DVector4(reader);
                    }
                }
                else
                {
                    // Stupid fix for this flag carrying some other meaning for older file versions
                    // We don't want to treat this file has having tangents, but
                    // we still need to keep the flag for if we're saving later.
                    OptionsHighBits |= (uint)I3DShapeOptions.HasTangents;
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
                        uvs[i] = new I3DUV(reader, fileVersion);
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
                bool singleBlendWeights = options.HasFlag(I3DShapeOptions.SingleBlendWeights);

                int numIndices = singleBlendWeights ? 1 : 4;

                // Load blend weights first, if necessary.
                // All weights per vertex should sum up to 1. Thus if you only have 1 index per vertex, this weight is always 1 and we don't need to save the weight information.
                if(!singleBlendWeights)
                {
                    // based on how 3D engines usually do skeletal/skinned meshes, you always have 4 weights and 4 indices per vertex
                    BlendWeights = new float[vertexCount, 4];
                    for(int i = 0; i < vertexCount; i++)
                    {
                        for(int j = 0; j < 4; j++)
                        {
                            BlendWeights[i, j] = reader.ReadSingle();
                        }
                    }
                }

                // Load blend indices
                BlendIndices = new byte[vertexCount, numIndices];

                for(int i = 0; i < vertexCount; i++)
                {
                    for(int j = 0; j < numIndices; j++)
                    {
                        BlendIndices[i, j] = reader.ReadByte();
                    }
                }
            }

            if(options.HasFlag(I3DShapeOptions.HasGeneric))
            {
                GenericData = new float[vertexCount];
                for(int i = 0; i < vertexCount; i++)
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

        protected override void WriteContents(BinaryWriter writer, short fileVersion)
        {
            BoundingVolume.Write(writer);
            writer.Write(CornerCount);
            writer.Write((uint)Subsets.Length);
            writer.Write(VertexCount);
            writer.Write((uint)Options);
            foreach(I3DShapeSubset subset in Subsets)
                subset.Write(writer, fileVersion, Options);

            foreach(I3DTri tri in Triangles)
                tri.Write(writer, VertexCount > (ushort.MaxValue + 1));

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

            if(fileVersion < VERSION_WITH_TANGENTS && Tangents != null)
                throw new InvalidOperationException($"I3D Entity version < {VERSION_WITH_TANGENTS} doesn't support saving tangents.");

            if(Tangents != null)
            {
                if(Tangents.Length != VertexCount)
                    throw new InvalidOperationException("Tangents array must be of size VertexCount");

                foreach(I3DVector4 vec in Tangents)
                    vec.Write(writer);
            }

            for(int uvSet = 0; uvSet < 4; uvSet++)
            {
                if(UVSets[uvSet] != null)
                {
                    if(UVSets[uvSet].Length != VertexCount)
                        throw new InvalidOperationException($"UVSets[{uvSet}] array must be of size VertexCount");

                    foreach(I3DUV uv in UVSets[uvSet])
                        uv.Write(writer, fileVersion);
                }
            }

            if(VertexColor != null)
            {
                if(VertexColor.Length != VertexCount)
                    throw new InvalidOperationException("VertexColor array must be of size VertexCount");

                foreach(I3DVector4 vec in VertexColor)
                    vec.Write(writer);
            }

            if(BlendIndices != null)
            {
                if(BlendWeights == null && BlendIndices.GetLength(1) != 1)
                    throw new InvalidOperationException("Second dimension of BlendIndices must be 1 if blend weights haven't been specified (single index blend weight mode).");

                if(BlendWeights != null && BlendIndices.GetLength(1) != 4)
                    throw new InvalidOperationException("Second dimension of BlendIndices must be 4 if blend weights have been specified.");

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

                for(int i = 0; i < BlendIndices.GetLength(0); i++)
                {
                    for(int j = 0; j < BlendIndices.GetLength(1); j++)
                    {
                        writer.Write(BlendIndices[i, j]);
                    }
                }
            }

            if(GenericData != null)
            {
                if(GenericData.Length != VertexCount)
                    throw new InvalidOperationException("GenericData array must be of size VertexCount");

                foreach(float v in GenericData)
                    writer.Write(v);
            }

            writer.Write((uint)Attachments.Length);
            foreach(I3DShapeAttachment data in Attachments)
                data.Write(writer);
        }

        public override string ToString()
        {
            return $"I3DShape #{Id} {Name}";
        }
    }
}