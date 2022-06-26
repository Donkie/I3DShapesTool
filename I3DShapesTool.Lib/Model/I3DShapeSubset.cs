using System;
using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShapeSubset
    {
        public uint FirstVertex { get; }
        public uint NumVertices { get; }
        public uint FirstIndex { get; }
        public uint NumIndices { get; }
        public float? UVDensity1 { get; }
        public float? UVDensity2 { get; }
        public float? UVDensity3 { get; }
        public float? UVDensity4 { get; }

        public I3DShapeSubset(BinaryReader br, int version, I3DShapeOptions options)
        {
            FirstVertex = br.ReadUInt32();
            NumVertices = br.ReadUInt32();
            FirstIndex = br.ReadUInt32();
            NumIndices = br.ReadUInt32();

            if(version >= 6)
            {
                if(options.HasFlag(I3DShapeOptions.HasUV1))
                    UVDensity1 = br.ReadSingle();
                if(options.HasFlag(I3DShapeOptions.HasUV2))
                    UVDensity2 = br.ReadSingle();
                if(options.HasFlag(I3DShapeOptions.HasUV3))
                    UVDensity3 = br.ReadSingle();
                if(options.HasFlag(I3DShapeOptions.HasUV4))
                    UVDensity4 = br.ReadSingle();
            }
        }

        public void Write(BinaryWriter bw, int version, I3DShapeOptions options)
        {
            bw.Write(FirstVertex);
            bw.Write(NumVertices);
            bw.Write(FirstIndex);
            bw.Write(NumIndices);

            if(version >= 6)
            {
                if(options.HasFlag(I3DShapeOptions.HasUV1) && UVDensity1 == null)
                    throw new InvalidOperationException("Shape has a UV1 map but subset doesn't have any UV density 1 set.");
                if(options.HasFlag(I3DShapeOptions.HasUV2) && UVDensity2 == null)
                    throw new InvalidOperationException("Shape has a UV2 map but subset doesn't have any UV density 2 set.");
                if(options.HasFlag(I3DShapeOptions.HasUV3) && UVDensity3 == null)
                    throw new InvalidOperationException("Shape has a UV3 map but subset doesn't have any UV density 3 set.");
                if(options.HasFlag(I3DShapeOptions.HasUV4) && UVDensity4 == null)
                    throw new InvalidOperationException("Shape has a UV4 map but subset doesn't have any UV density 4 set.");

                if(UVDensity1 != null)
                    bw.Write((float)UVDensity1);
                if(UVDensity2 != null)
                    bw.Write((float)UVDensity2);
                if(UVDensity3 != null)
                    bw.Write((float)UVDensity3);
                if(UVDensity4 != null)
                    bw.Write((float)UVDensity4);
            }
        }
    }
}
