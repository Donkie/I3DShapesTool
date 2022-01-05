using System;
using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DVector
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static I3DVector Zero = new I3DVector(0, 0, 0);
        public static I3DVector One = new I3DVector(1, 1, 1);

        public I3DVector(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
        }

        public I3DVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((float)X);
            bw.Write((float)Y);
            bw.Write((float)Z);
        }

        public double Length()
        {
            return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }

        public bool IsUnitLength()
        {
            return Math.Abs((X * X) + (Y * Y) + (Z * Z) - 1.0) < 1e-3;
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
