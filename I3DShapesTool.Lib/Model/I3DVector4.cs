using System;
using System.Collections.Generic;
using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public struct I3DVector4
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }

        public static I3DVector4 Zero = new I3DVector4(0, 0, 0, 0);

        public I3DVector4(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
            W = br.ReadSingle();
        }

        public I3DVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
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

    public class I3DVector4Comparer : EqualityComparer<I3DVector4>
    {
        public override bool Equals(I3DVector4 x, I3DVector4 y)
        {
            return Math.Abs(x.X - x.X) < 1e-5 && Math.Abs(x.Y - x.Y) < 1e-5 && Math.Abs(x.Z - x.Z) < 1e-5 && Math.Abs(x.W - x.W) < 1e-5;
        }

        public override int GetHashCode(I3DVector4 obj)
        {
            int hCode = obj.X.GetHashCode() ^ obj.Y.GetHashCode() ^ obj.Z.GetHashCode() ^ obj.W.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}
