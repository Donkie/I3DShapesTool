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
                P1Idx = br.ReadUInt32() + 1u;
                P2Idx = br.ReadUInt32() + 1u;
                P3Idx = br.ReadUInt32() + 1u;
            }
            else
            {
                P1Idx = br.ReadUInt16() + 1u;
                P2Idx = br.ReadUInt16() + 1u;
                P3Idx = br.ReadUInt16() + 1u;
            }
        }

        public I3DTri(uint p1, uint p2, uint p3)
        {
            P1Idx = p1;
            P2Idx = p2;
            P3Idx = p3;
        }

        public void Write(BinaryWriter bw, bool isInt)
        {
            if(isInt)
            {
                bw.Write(P1Idx - 1);
                bw.Write(P2Idx - 1);
                bw.Write(P3Idx - 1);
            }
            else
            {
                bw.Write((ushort)(P1Idx - 1));
                bw.Write((ushort)(P2Idx - 1));
                bw.Write((ushort)(P3Idx - 1));
            }
        }

        public override string ToString()
        {
            return $"Tri ({P1Idx}, {P2Idx}, {P3Idx})";
        }
    }
}
