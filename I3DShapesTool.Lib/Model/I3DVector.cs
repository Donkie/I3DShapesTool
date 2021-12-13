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

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public bool IsUnitLength()
        {
            return Math.Abs(Length() - 1.0) < 1e-6;
        }

        public override string ToString()
        {
            return $"3D ({X}, {Y}, {Z}), L={Length()}";
        }
    }
}
