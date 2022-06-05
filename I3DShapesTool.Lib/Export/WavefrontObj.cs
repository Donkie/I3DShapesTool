using System;
using System.Globalization;
using System.Text;
using System.Linq;
using I3DShapesTool.Lib.Model;
using I3DShapesTool.Lib.Model.I3D;

namespace I3DShapesTool.Lib.Export
{
    public class WavefrontObj
    {
        public string Name { get; }

        public string GeometryName { get; }

        public float Scale { get; }

        public I3DTri[] Triangles { get; private set; }

        public I3DVector[] Positions { get; private set; }

        public I3DVector[]? Normals { get; private set; }

        public I3DUV[]? UVs { get; private set; }

        public WavefrontObj(I3DShape shape, string name)
        {
            Scale = 100;

            var geomname = shape.Name;
            if (geomname.EndsWith("Shape"))
                geomname = geomname.Substring(0, geomname.Length - 5);

            Name = name;
            GeometryName = geomname;
            Positions = shape.Positions;
            Triangles = shape.Triangles;

            if(shape.Normals != null)
                Normals = shape.Normals;

            if(shape.UVSets.Length > 0)
                UVs = shape.UVSets[0];
        }

        public WavefrontObj(Shape shape, string name, bool shouldTransform)
        {
            var shapeData = shape.ShapeData;
            if (shapeData == null)
                throw new ArgumentException("Input shape doesn't have any assigned shape data");

            Scale = 1;

            var geomname = name;
            if (geomname.EndsWith("Shape"))
                geomname = geomname[0..^5];

            Name = name;
            GeometryName = geomname;
            if (shouldTransform)
            {
                Positions = shapeData.Positions.Select(v => shape.AbsoluteTransform * v).ToArray();
            }
            else
            {
                Positions = shapeData.Positions;
            }
            Triangles = shapeData.Triangles;

            if (shapeData.Normals != null)
                Normals = shapeData.Normals;

            if (shapeData.UVSets.Length > 0)
                UVs = shapeData.UVSets[0];
        }

        /// <summary>
        /// Merges the vertex data of another WavefrontObj into this
        /// </summary>
        /// <param name="newObj"></param>
        public void Merge(WavefrontObj newObj)
        {
            uint oldVertCnt = (uint)Positions.Length;

            I3DVector[] newPos = new I3DVector[Positions.Length + newObj.Positions.Length];
            Positions.CopyTo(newPos, 0);
            newObj.Positions.CopyTo(newPos, oldVertCnt);

            if(Normals != null || newObj.Normals != null)
            {
                I3DVector[] newNorm = new I3DVector[(Normals?.Length ?? 0) + (newObj.Normals?.Length ?? 0)];
                Normals?.CopyTo(newNorm, 0);
                newObj.Normals?.CopyTo(newNorm, Normals?.Length ?? 0);
                Normals = newNorm;
            }

            if(UVs != null || newObj.UVs != null)
            {
                I3DUV[] newUV = new I3DUV[(UVs?.Length ?? 0) + (newObj.UVs?.Length ?? 0)];
                UVs?.CopyTo(newUV, 0);
                newObj.UVs?.CopyTo(newUV, UVs?.Length ?? 0);
                UVs = newUV;
            }

            I3DTri[] newTris = new I3DTri[Triangles.Length + newObj.Triangles.Length];
            Triangles.CopyTo(newTris, 0);
            for(int i = 0; i < newObj.Triangles.Length; i++)
            {
                I3DTri newObjTr = newObj.Triangles[i];
                newTris[Triangles.Length + i] = new I3DTri(newObjTr.P1Idx + oldVertCnt, newObjTr.P2Idx + oldVertCnt, newObjTr.P3Idx + oldVertCnt);
            }

            Positions = newPos;
            Triangles = newTris;
        }

        private void WriteHeader(StringBuilder sb)
        {
            sb.AppendLine("# Wavefront OBJ file");
            sb.AppendLine("# Creator: I3DShapesTool by Donkie");
            sb.AppendFormat(CultureInfo.InvariantCulture, "# Name: {0:G}\n", Name);
            sb.AppendFormat(CultureInfo.InvariantCulture, "# Scale: {0:F}\n", Scale);
        }

        private static void SetGroup(StringBuilder sb, string groupName)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "g {0:G}\n", groupName);
        }

        private static void SetSmoothing(StringBuilder sb, bool smoothOn)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "s {0:G}\n", smoothOn ? "on" : "off");
        }

        private void AddVertex(StringBuilder sb, I3DVector vec)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "v {0:F4} {1:F4} {2:F4}\n", vec.X * Scale, vec.Y * Scale, vec.Z * Scale);
        }

        private static void AddUV(StringBuilder sb, I3DUV uv)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "vt {0:F6} {1:F6}\n", uv.U, uv.V);
        }

        private static void AddNormal(StringBuilder sb, I3DVector vec)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "vn {0:F6} {1:F6} {2:F6}\n", vec.X, vec.Y, vec.Z);
        }

        private static void AddTriangleFace(StringBuilder sb, uint idx, bool hasUV, bool hasNormal)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F0}", idx);

            if (hasUV)
                sb.AppendFormat(CultureInfo.InvariantCulture, "/{0:F0}", idx);
            else if (hasNormal)
                sb.Append('/');

            if (hasNormal)
                sb.AppendFormat(CultureInfo.InvariantCulture, "/{0:F0}", idx);
        }

        private static void AddTriangle(StringBuilder sb, I3DTri tri, bool hasUV, bool hasNormal)
        {
            sb.Append("f ");
            AddTriangleFace(sb, tri.P1Idx, hasUV, hasNormal);
            sb.Append(" ");
            AddTriangleFace(sb, tri.P2Idx, hasUV, hasNormal);
            sb.Append(" ");
            AddTriangleFace(sb, tri.P3Idx, hasUV, hasNormal);
            sb.Append("\n");
        }

        public byte[] ExportToBlob()
        {
            var sb = new StringBuilder();

            WriteHeader(sb);
            sb.AppendLine();
            SetGroup(sb, "default");
            sb.AppendLine();
            foreach (var t in Positions)
            {
                AddVertex(sb, t);
            }
            if(UVs != null)
            {
                foreach (var t in UVs)
                {
                    AddUV(sb, t);
                }
            }
            if (Normals != null)
            {
                foreach (var t in Normals)
                {
                    AddNormal(sb, t);
                }
            }
            SetSmoothing(sb, false);
            SetGroup(sb, GeometryName);
            foreach (var t in Triangles)
            {
                AddTriangle(sb, t, UVs != null, Normals != null);
            }
            
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
