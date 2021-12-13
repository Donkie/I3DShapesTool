using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DTri
    {
        public ushort P1Idx { get; set; }
        public ushort P2Idx { get; set; }
        public ushort P3Idx { get; set; }

        public I3DTri(BinaryReader br)
        {
            P1Idx = br.ReadUInt16();
            P2Idx = br.ReadUInt16();
            P3Idx = br.ReadUInt16();
        }

        public void Write(BinaryWriter bw, bool zeroBasedIndicesInRawData)
        {
            int offset = zeroBasedIndicesInRawData ? -1 : 0;

            bw.Write((ushort)(P1Idx + offset));
            bw.Write((ushort)(P2Idx + offset));
            bw.Write((ushort)(P3Idx + offset));
        }

        public override string ToString()
        {
            return $"Tri ({P1Idx}, {P2Idx}, {P3Idx})";
        }
    }
}
