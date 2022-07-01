
using I3DShapesTool.Lib.Model;
using I3DShapesTool.Lib.Model.I3D;
using I3DShapesTool.Lib.Tools;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;
using SharpGLTF.Scenes;
using System.Numerics;
using System.IO;

namespace I3DShapesTool.Lib.Export
{
    public class GLTFExporter
    {
        public static void Test(I3D i3dFile, string outFolder)
        {
            SceneBuilder scene = new SceneBuilder();

            MaterialBuilder material1 = new MaterialBuilder()
                .WithDoubleSide(true)
                .WithMetallicRoughnessShader()
                .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

            foreach(Shape shape in i3dFile.GetShapes())
            {
                if(shape.ShapeData == null || shape.ShapeData.Normals == null)
                    continue;

                MeshBuilder<VertexPositionNormal> mesh = new MeshBuilder<VertexPositionNormal>("mymesh");

                PrimitiveBuilder<MaterialBuilder, VertexPositionNormal, VertexEmpty, VertexEmpty> prim = mesh.UsePrimitive(material1);
                foreach(I3DTri tri in shape.ShapeData.Triangles)
                {
                    I3DVector v1 = shape.ShapeData.Positions[tri.P1Idx - 1];
                    I3DVector v2 = shape.ShapeData.Positions[tri.P2Idx - 1];
                    I3DVector v3 = shape.ShapeData.Positions[tri.P3Idx - 1];
                    I3DVector n1 = shape.ShapeData.Normals[tri.P1Idx - 1];
                    I3DVector n2 = shape.ShapeData.Normals[tri.P2Idx - 1];
                    I3DVector n3 = shape.ShapeData.Normals[tri.P3Idx - 1];

                    prim.AddTriangle(new VertexPositionNormal((float)v1.X, (float)v1.Y, (float)v1.Z, (float)n1.X, (float)n1.Y, (float)n1.Z),
                                     new VertexPositionNormal((float)v2.X, (float)v2.Y, (float)v2.Z, (float)n2.X, (float)n2.Y, (float)n2.Z),
                                     new VertexPositionNormal((float)v3.X, (float)v3.Y, (float)v3.Z, (float)n3.X, (float)n3.Y, (float)n3.Z));
                }

                Transform t = shape.AbsoluteTransform;
                /*
                scene.AddRigidMesh(mesh, new Matrix4x4(
                    (float)t[0, 0], (float)t[0, 1], (float)t[0, 2], (float)t[0, 3],
                    (float)t[1, 0], (float)t[1, 1], (float)t[1, 2], (float)t[1, 3],
                    (float)t[2, 0], (float)t[2, 1], (float)t[2, 2], (float)t[2, 3],
                    (float)t[3, 0], (float)t[3, 1], (float)t[3, 2], (float)t[3, 3]));
                */
                scene.AddRigidMesh(mesh, Matrix4x4.Identity);
            }

            ModelRoot model = scene.ToGltf2();
            model.SaveAsWavefront(Path.Combine(outFolder, "mesh.obj"));
            model.SaveGLB(Path.Combine(outFolder, "mesh.glb"));
            model.SaveGLTF(Path.Combine(outFolder, "mesh.gltf"));
        }
    }
}
