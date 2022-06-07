using System;

namespace I3DShapesTool.Lib.Model
{
    [Flags]
    public enum I3DShapeOptions : uint
    {
        None = 0b0,
        HasNormals = 0b0001,
        HasUV1 = 0b0010,
        HasUV2 = 0b0100,
        HasUV3 = 0b1000,
        HasUV4 = 0b0001_0000,
        HasVertexColor = 0b0010_0000,
        HasSkinningInfo = 0b0100_0000,
        HasPrecomputed4DVectorData = 0b1000_0000, // Before i3d version 4 this flag meant something else, so be careful
        NoBlendWeights = 0b0001_0000_0000,
        HasUnknownData2 = 0b0010_0000_0000,
        All = 0b0011_1111_1111
    }
}
