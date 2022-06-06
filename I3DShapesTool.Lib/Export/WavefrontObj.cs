using System.Linq;
using I3DShapesTool.Lib.Model;
using I3DShapesTool.Lib.Tools;
using System.IO;

namespace I3DShapesTool.Lib.Export
{
    public class WavefrontObj : IExporter
    {
        private string Name { get; }

        private string GeometryName { get; }

        private float Scale { get; }

        private I3DTri[] Triangles { get; set; }

        private I3DVector[] Positions { get; set; }

        private I3DVector[]? Normals { get; set; }

        private I3DUV[]? UVs { get; set; }

        public WavefrontObj(I3DShape shape, string name, float scale = 100)
        {
            Scale = scale;

            string? geomname = shape.Name;
            if(geomname.EndsWith("Shape"))
                geomname = geomname[0..^5];

            Name = name;
            GeometryName = geomname;
            Positions = shape.Positions;
            Triangles = shape.Triangles;

            if(shape.Normals != null)
                Normals = shape.Normals;

            if(shape.UVSets.Length > 0)
                UVs = shape.UVSets[0];
        }

        /// <summary>
        /// Transforms the vertices of this obj using the transformation matrix
        /// </summary>
        /// <param name="t">Transformation matrix</param>
        public void Transform(Transform t)
        {
            Positions = Positions.Select(v => t * v).ToArray();
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

        private void WriteHeader(StreamWriter s)
        {
            s.WriteLine("# Wavefront OBJ file");
            s.WriteLine("# Creator: I3DShapesTool by Donkie");
            s.WriteLine("# Name: {0:G}", Name);
            s.WriteLine("# Scale: {0:F}", Scale);
        }

        private static void WriteGroup(StreamWriter s, string groupName)
        {
            s.WriteLine("g {0:G}", groupName);
        }

        private static void WriteSmoothing(StreamWriter s, bool smoothOn)
        {
            s.WriteLine("s {0:G}", smoothOn ? "on" : "off");
        }

        private void WriteVertex(StreamWriter s, I3DVector vec)
        {
            s.WriteLine("v {0:F4} {1:F4} {2:F4}", vec.X * Scale, vec.Y * Scale, vec.Z * Scale);
        }
        private static void WriteUV(StreamWriter s, I3DUV uv)
        {
            s.WriteLine("vt {0:F6} {1:F6}", uv.U, uv.V);
        }

        private static void WriteNormal(StreamWriter s, I3DVector vec)
        {
            s.WriteLine("vn {0:F6} {1:F6} {2:F6}", vec.X, vec.Y, vec.Z);
        }

        private void WriteTriangleFace(StreamWriter s, uint idx)
        {
            s.Write("{0:F0}", idx);

            if(UVs != null)
                s.Write("/{0:F0}", idx);
            else if(Normals != null)
                s.Write('/');

            if(Normals != null)
                s.Write("/{0:F0}", idx);
        }

        private void WriteTriangle(StreamWriter s, I3DTri tri)
        {
            s.Write("f ");
            WriteTriangleFace(s, tri.P1Idx);
            s.Write(" ");
            WriteTriangleFace(s, tri.P2Idx);
            s.Write(" ");
            WriteTriangleFace(s, tri.P3Idx);
            s.WriteLine();
        }

        /// <summary>
        /// Writes the .obj data to a stream
        /// </summary>
        /// <param name="stream">The stream</param>
        public void Export(Stream stream)
        {
            using StreamWriter s = new InvariantStreamWriter(stream);

            WriteHeader(s);
            s.WriteLine();
            WriteGroup(s, "default");
            s.WriteLine();
            foreach(I3DVector t in Positions)
            {
                WriteVertex(s, t);
            }
            if(UVs != null)
            {
                foreach(I3DUV t in UVs)
                {
                    WriteUV(s, t);
                }
            }
            if(Normals != null)
            {
                foreach(I3DVector t in Normals)
                {
                    WriteNormal(s, t);
                }
            }
            WriteSmoothing(s, false);
            WriteGroup(s, GeometryName);
            foreach(I3DTri t in Triangles)
            {
                WriteTriangle(s, t);
            }
        }
    }
}
