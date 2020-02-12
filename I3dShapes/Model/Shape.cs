using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Exceptions;
using I3dShapes.Model.Primitive;
using I3dShapes.Tools.Extensions;
using MoreLinq;

namespace I3dShapes.Model
{
    public class Shape : NamedShapeObject
    {
        private readonly int _version;

        //private bool Align => _version >= 4;
        [Flags]
        public enum ContainsFlag : uint
        {
            VertexNormal = 0x00000001,
            TextureCoordinate = 0x00000002,
            TextureCoordinate2 = 0x00000004,
            TextureCoordinate3 = 0x00000008,
            TextureCoordinate4 = 0x00000010,
            Flag6 = 0x00000020,
            Flag7 = 0x00000040,
            Flag8 = 0x00000080,
            Flag9 = 0x00000100,
            Flag10 = 0x00000200,
            Flag11 = 0x00000400,
            Flag12 = 0x00000800,
            Flag13 = 0x00001000,
            Flag14 = 0x00002000,
            Flag15 = 0x00004000,
            Flag16 = 0x00008000,
            Flag17 = 0x00010000,
            Flag18 = 0x00020000,
            Flag19 = 0x00040000,
            Flag20 = 0x00080000,
            Flag21 = 0x00100000,
            Flag22 = 0x00200000,
            Flag23 = 0x00400000,
            Flag24 = 0x00800000,
            Flag25 = 0x01000000,
            Flag26 = 0x02000000,
            Flag27 = 0x04000000,
            Flag28 = 0x08000000,
            Flag29 = 0x10000000,
            Flag30 = 0x20000000,
            Flag31 = 0x40000000,
            Flag32 = 0x80000000,
        }

        public Shape(BinaryReader reader, int version)
            : base(ShapeType.Type1)
        {
            _version = version;
            Load(reader);
        }

        public float BoundingVolumeX { get; private set; }
        public float BoundingVolumeY { get; private set; }
        public float BoundingVolumeZ { get; private set; }
        public float BoundingVolumeR { get; private set; }
        public int VertexCount { get; private set; }
        public int Unknown6 { get; private set; }
        public int Vertices { get; private set; }

        /// <summary>
        /// Contains values.
        /// </summary>
        public ContainsFlag ContainsFlags { get; private set; }

        public int Unknown8 { get; private set; }
        public int UvCount { get; private set; }
        public int Unknown9 { get; private set; }
        public int VertexCount2 { get; private set; }

        public ICollection<PointIndex> PointIndexes { get; private set; }

        public ICollection<PointVector> PointVectors { get; private set; }

        /// <summary>
        /// Set if <see cref="Unknown6"/> = 2.
        /// Size 4 UInt32.
        /// </summary>
        public ICollection<uint> UnknownStruct6 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.VertexNormal"> set.
        /// Size = <see cref="Vertices"/>
        /// </summary>
        public ICollection<VertexNormal> VertexNormals { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.Flag8"> set.
        /// Size = 4 * <see cref="Vertices"/>
        /// </summary>
        public ICollection<float> UnknownData8 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.TextureCoordinate"> set.
        /// Size = <see cref="Vertices"/>
        /// </summary>
        public ICollection<TextureCoordinateUV> TextureCoordinates { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.TextureCoordinate2"> set.
        /// Size = <see cref="Vertices"/>
        /// </summary>
        public ICollection<TextureCoordinateUV> TextureCoordinates2 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.TextureCoordinate3"> set.
        /// Size = <see cref="Vertices"/>
        /// </summary>
        public ICollection<TextureCoordinateUV> TextureCoordinates3 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.TextureCoordinate4"> set.
        /// Size = <see cref="Vertices"/>
        /// </summary>
        public ICollection<TextureCoordinateUV> TextureCoordinates4 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.Flag6"> set.
        /// Size = 4 * <see cref="Vertices"/>
        /// </summary>
        public ICollection<float> UnknownData6 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.Flag7"> set.
        /// Size = 4 * <see cref="Vertices"/>
        /// </summary>
        public ICollection<ICollection<float>> UnknownData7 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.Flag7"> set.
        /// Size = 2 * <see cref="Vertices"/>
        /// </summary>
        public ICollection<ICollection<ushort>> UnknownData7_2 { get; private set; }

        /// <summary>
        /// Set if <see cref="ContainsFlag.Flag9"> set.
        /// Size = 2 * <see cref="Vertices"/>
        /// </summary>
        public ICollection<byte> UnknownData9 { get; private set; }

        /// <summary>
        /// Addition content.
        /// </summary>
        public Additions Addition { get; set; }

        private new void Load(BinaryReader reader)
        {
            base.Load(reader, true);
            var pos = reader.BaseStream.Position;
            BoundingVolumeX = reader.ReadSingle();
            BoundingVolumeY = reader.ReadSingle();
            BoundingVolumeZ = reader.ReadSingle();
            BoundingVolumeR = reader.ReadSingle();
            VertexCount = reader.ReadInt32();
            Unknown6 = reader.ReadInt32();
            Vertices = reader.ReadInt32();
            ContainsFlags = (ContainsFlag)reader.ReadUInt32();
            Unknown8 = reader.ReadInt32();
            UvCount = reader.ReadInt32();
            Unknown9 = reader.ReadInt32();
            VertexCount2 = reader.ReadInt32();

            if (Unknown6 == 2)
            {
                UnknownStruct6 = Enumerable
                                 .Range(0, 4)
                                 .Select(v => reader.ReadUInt32())
                                 .ToArray();
            }
            else if (Unknown6 != 1)
            {
                throw new UnknownFormatShapeException();
            }

            PointIndexes = Enumerable
                           .Range(0, VertexCount / 3)
                           .Select(v => new PointIndex(reader, Vertices > 0xFFFF))
                           .ToArray();

            var align = reader.Align(4);
            if (align.Any(v => v != 0))
            {
                throw new UnknownFormatShapeException();
            }

            PointVectors = Enumerable
                           .Range(0, Vertices)
                           .Select(v => new PointVector(reader))
                           .ToArray();

            LoadBinary(reader, ContainsFlags, Vertices);
            Addition = new Additions(reader);

            if (!reader.EndOfStream())
            {
                throw new UnknownFormatShapeException();
            }
        }

        private void LoadBinary(BinaryReader reader, ContainsFlag flag, int verticesCount)
        {
            if (flag.HasFlag(ContainsFlag.VertexNormal))
            {
                VertexNormals = Enumerable
                                .Range(0, verticesCount)
                                .Select(v => new VertexNormal(reader))
                                .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.Flag8) && _version > 4)
            {
                UnknownData8 = Enumerable
                                     .Range(0, verticesCount * 4)
                                     .Select(v => reader.ReadSingle())
                                     .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.TextureCoordinate))
            {
                TextureCoordinates = Enumerable
                                     .Range(0, verticesCount)
                                     .Select(v => new TextureCoordinateUV(reader))
                                     .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.TextureCoordinate2))
            {
                TextureCoordinates2 = Enumerable
                                      .Range(0, verticesCount)
                                      .Select(v => new TextureCoordinateUV(reader))
                                      .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.TextureCoordinate3))
            {
                TextureCoordinates3 = Enumerable
                                      .Range(0, verticesCount)
                                      .Select(v => new TextureCoordinateUV(reader))
                                      .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.TextureCoordinate4))
            {
                TextureCoordinates4 = Enumerable
                                      .Range(0, verticesCount)
                                      .Select(v => new TextureCoordinateUV(reader))
                                      .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.Flag6))
            {
                UnknownData6 = Enumerable
                               .Range(0, verticesCount * 4)
                               .Select(v => reader.ReadSingle())
                               .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.Flag7) && !flag.HasFlag(ContainsFlag.Flag9))
            {
                UnknownData7 = Enumerable
                               .Range(0, verticesCount)
                               .Select(
                                   v => Enumerable
                                        .Range(0, 4)
                                        .Select(i => reader.ReadSingle())
                                        .ToArray()
                               )
                               .ToArray();
                UnknownData7_2 = Enumerable
                                 .Range(0, verticesCount)
                                 .Select(
                                     v => Enumerable
                                          .Range(0, 2)
                                          .Select(i => reader.ReadUInt16())
                                          .ToArray()
                                 )
                                 .ToArray();
            }

            if (flag.HasFlag(ContainsFlag.Flag9))
            {
                UnknownData9 = Enumerable
                               .Range(0, verticesCount)
                               .Select(i => reader.ReadByte())
                               .ToArray();
            }
        }
    }
}
