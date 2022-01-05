using I3DShapesTool.Lib.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace I3DShapesTool.Lib.Tools
{
    public class Transform
    {
        private readonly double[,] v;

        private const double DEG2RAD = Math.PI / 180;

        public static Transform Identity = new Transform(new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            });

        public Transform(double[,] data)
        {
            if (data.GetLength(0) != 4 || data.GetLength(1) != 4)
                throw new ArgumentException("Input matrix must be 4x4");
            v = data;
        }

        public Transform Translate(I3DVector vec)
        {
            Transform t = new Transform(new double[,]
            {
                { 1, 0, 0, vec.X },
                { 0, 1, 0, vec.Y },
                { 0, 0, 1, vec.Z },
                { 0, 0, 0, 1 }
            });

            return t * this;
        }

        public Transform Rotate(I3DVector rot)
        {
            double cosX = Math.Cos(rot.X * DEG2RAD);
            double sinX = Math.Sin(rot.X * DEG2RAD);
            Transform Rx = new Transform(new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, cosX, -sinX, 0 },
                { 0, sinX, cosX, 0 },
                { 0, 0, 0, 1 }
            });

            double cosY = Math.Cos(rot.Y * DEG2RAD);
            double sinY = Math.Sin(rot.Y * DEG2RAD);
            Transform Ry = new Transform(new double[,]
            {
                { cosY, 0, sinY, 0 },
                { 0, 1, 0, 0 },
                { -sinY, 0, cosY, 0 },
                { 0, 0, 0, 1 }
            });

            double cosZ = Math.Cos(rot.Z * DEG2RAD);
            double sinZ = Math.Sin(rot.Z * DEG2RAD);
            Transform Rz = new Transform(new double[,]
            {
                { cosZ, -sinZ, 0, 0 },
                { sinZ, cosZ, 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            });

            return Rz * (Ry * (Rx * this));
        }

        public Transform Scale(I3DVector scale)
        {
            Transform s = new Transform(new double[,]
            {
                { scale.X, 0, 0, 0 },
                { 0, scale.Y, 0, 0 },
                { 0, 0, scale.Z, 0 },
                { 0, 0, 0, 1 }
            });

            return s * this;
        }

        public double this[int i, int j]
        {
            get { return v[i, j]; }
        }

        public static Transform operator *(Transform a, Transform b)
        {
            double[,] res = new double[4, 4];

            for(int row = 0; row < 4; row++)
            {
                for(int col = 0; col < 4; col++)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        res[row, col] += a[row, i] * b[i, col];
                    }
                }
            }

            return new Transform(res);
        }

        public static I3DVector operator *(Transform a, I3DVector b)
        {
            double x = a[0, 0] * b.X + a[0, 1] * b.Y + a[0, 2] * b.Z + a[0, 3];
            double y = a[1, 0] * b.X + a[1, 1] * b.Y + a[1, 2] * b.Z + a[1, 3];
            double z = a[2, 0] * b.X + a[2, 1] * b.Y + a[2, 2] * b.Z + a[2, 3];
            return new I3DVector(x, y, z);
        }
    }
}
