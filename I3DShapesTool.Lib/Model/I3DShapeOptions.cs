using System;

namespace I3DShapesTool.Lib.Model
{
    [Flags]
    public enum I3DShapeOptions : uint
    {
        None = 0x0,
        HasNormals = 0x1,
        HasUV1 = 0x2,
        HasUV2 = 0x4,
        HasUV3 = 0x8,
        HasUV4 = 0x10,
        HasUnknown4DVect = 0x20,
        HasSkinningInfo = 0x40,
        HasPrecomputed4DVectorData = 0x80,
        NoBlendWeights = 0x100,
        HasUnknownData2 = 0x200
    }
}
