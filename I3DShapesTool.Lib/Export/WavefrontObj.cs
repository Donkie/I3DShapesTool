using System.Globalization;
using System.Text;
using I3DShapesTool.Lib.Model;

namespace I3DShapesTool.Lib.Export
{
    public class WavefrontObj
    {
        public string Name { get; }

        public string GeometryName { get; }

        public float Scale { get; }

        public I3DTri[] Triangles { get; }

        public I3DVector[] Positions { get; }

        public I3DVector[]? Normals { get; }

        public I3DUV[]? UVs { get; }

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

        private static void AddTriangleFace(StringBuilder sb, ushort idx, bool hasUV, bool hasNormal)
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
