using System.Globalization;
using System.Text;

namespace I3DShapesTool
{
    public class WavefrontObj
    {
        public string Name { get; set; }

        public string GeometryName { get; set; }

        public float Scale { get; set; }

        public I3DTri[] Triangles { get; set; }

        public I3DVector[] Positions { get; set; }

        public I3DVector[] Normals { get; set;  }

        public I3DUV[] UVs { get; set; }

        public WavefrontObj()
        {
            Scale = 100;
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
            sb.AppendFormat(CultureInfo.InvariantCulture, "v {0:G} {1:G} {2:G}\n", vec.X * Scale, vec.Y * Scale, vec.Z * Scale);
        }

        private static void AddUV(StringBuilder sb, I3DUV uv)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "vt {0:F} {1:F}\n", uv.U, uv.V);
        }

        private static void AddNormal(StringBuilder sb, I3DVector vec)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "vn {0:F4} {1:F4} {2:F4}\n", vec.X, vec.Y, vec.Z);
        }

        private static void AddTriangle(StringBuilder sb, I3DTri tri)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "f {0:F0}/{0:F0}/{0:F0} {1:F0}/{1:F0}/{1:F0} {2:F0}/{2:F0}/{2:F0}\n", tri.P1Idx, tri.P2Idx, tri.P3Idx);
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
            foreach (var t in UVs)
            {
                AddUV(sb, t);
            }
            foreach (var t in Normals)
            {
                AddNormal(sb, t);
            }
            SetSmoothing(sb, false);
            SetGroup(sb, GeometryName);
            foreach (var t in Triangles)
            {
                AddTriangle(sb, t);
            }
            
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
