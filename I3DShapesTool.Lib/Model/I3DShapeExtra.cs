using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DShapeExtra
    {
        public uint Integer1 { get; }
        public uint Integer2 { get; }
        public uint Integer3 { get; }
        public uint Integer4 { get; }
        public float? FloatUV1 { get; }
        public float? FloatUV2 { get; }
        public float? FloatUV3 { get; }
        public float? FloatUV4 { get; }

        public I3DShapeExtra(BinaryReader br, int version, I3DShapeOptions options)
        {
            Integer1 = br.ReadUInt32();
            Integer2 = br.ReadUInt32();
            Integer3 = br.ReadUInt32();
            Integer4 = br.ReadUInt32();

            if (version >= 6)
            {
                if (options.HasFlag(I3DShapeOptions.HasUV1))
                    FloatUV1 = br.ReadSingle();
                if (options.HasFlag(I3DShapeOptions.HasUV2))
                    FloatUV2 = br.ReadSingle();
                if (options.HasFlag(I3DShapeOptions.HasUV3))
                    FloatUV3 = br.ReadSingle();
                if (options.HasFlag(I3DShapeOptions.HasUV4))
                    FloatUV4 = br.ReadSingle();
            }
        }
    }
}
