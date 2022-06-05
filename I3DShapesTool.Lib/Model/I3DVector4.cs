using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public struct I3DVector4
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }

        public I3DVector4(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
            W = br.ReadSingle();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(X);
            bw.Write(Y);
            bw.Write(Z);
            bw.Write(W);
        }

        public override string ToString()
        {
            return $"4D ({X}, {Y}, {Z}, {W})";
        }
    }
}
