using System;
using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DVector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public I3DVector(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(X);
            bw.Write(Y);
            bw.Write(Z);
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public bool IsUnitLength()
        {
            return Math.Abs(X * X + Y * Y + Z * Z - 1.0) < 1e-3;
        }

        public bool IsZero()
        {
            return Math.Abs(X) < 1e-6 && Math.Abs(Y) < 1e-6 && Math.Abs(Z) < 1e-6;
        }

        public bool IsValidNormal()
        {
            return IsUnitLength() || IsZero();
        }

        public override string ToString()
        {
            return $"3D ({X}, {Y}, {Z}), L={Length()}";
        }
    }
}
