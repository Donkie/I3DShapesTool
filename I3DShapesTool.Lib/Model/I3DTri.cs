using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DTri
    {
        public uint P1Idx { get; set; }
        public uint P2Idx { get; set; }
        public uint P3Idx { get; set; }

        public I3DTri(BinaryReader br, bool isInt)
        {
            if(isInt)
            {
                P1Idx = br.ReadUInt32();
                P2Idx = br.ReadUInt32();
                P3Idx = br.ReadUInt32();
            }
            else
            {
                P1Idx = br.ReadUInt16();
                P2Idx = br.ReadUInt16();
                P3Idx = br.ReadUInt16();
            }
        }

        public void Write(BinaryWriter bw, bool zeroBasedIndicesInRawData, bool isInt)
        {
            int offset = zeroBasedIndicesInRawData ? -1 : 0;

            if(isInt)
            {
                bw.Write((uint)(P1Idx + offset));
                bw.Write((uint)(P2Idx + offset));
                bw.Write((uint)(P3Idx + offset));
            }
            else
            {
                bw.Write((ushort)(P1Idx + offset));
                bw.Write((ushort)(P2Idx + offset));
                bw.Write((ushort)(P3Idx + offset));
            }
        }

        public override string ToString()
        {
            return $"Tri ({P1Idx}, {P2Idx}, {P3Idx})";
        }
    }
}
