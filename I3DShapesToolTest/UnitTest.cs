using System.IO;
using Xunit;
using I3DShapesTool.Lib.Model;
using System.Linq;
using System;

namespace I3DShapesToolTest
{
    public class UnitTest
    {
        private const float MaxUV = 10f;

        public UnitTest()
        {
        }

        private static void AssertShapesFile(ShapesFile file, int seed, int version, int shapeCount)
        {
            Assert.Equal(seed, file.Seed);
            Assert.Equal(version, file.Version);
            Assert.Equal(shapeCount, file.ShapeCount);
        }

        private static void AssertShape(I3DShape shape, string name, uint shapeId, int vertexCount, int faceCount)
        {
            Assert.Equal(name, shape.Name);
            Assert.Equal(shapeId, shape.Id);
            Assert.Equal(vertexCount, shape.Positions.Length);
            Assert.Equal(faceCount, shape.Triangles.Length);

            if (shape.Normals != null)
            {
                Assert.Equal(vertexCount, shape.Normals.Length);
            }

            foreach (var uvSet in shape.UVSets)
            {
                if (uvSet != null)
                {
                    Assert.Equal(vertexCount, uvSet.Length);
                }
            }
        }

        private static void AssertShapeData(ShapesFile file)
        {
            foreach (var shape in file.Shapes)
            {
                Assert.True(shape.UVSets.All(uvSet => uvSet == null || uvSet.All(uv => uv.U >= -MaxUV && uv.U <= MaxUV && uv.V >= -MaxUV && uv.V <= MaxUV)));
                Assert.True(shape.Triangles.All(tri => tri.P1Idx < shape.CornerCount && tri.P2Idx < shape.CornerCount && tri.P3Idx < shape.CornerCount));
                if (shape.Normals != null)
                {
                    Assert.True(shape.Normals.All(v => v.IsUnitLength()));
                }
            }
        }

        [SkippableFact]
        public void TestFS22()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 22");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\boeckmann\bigMasterWesternWCF\bigMasterWesternWCF.i3d.shapes"));
            AssertShapesFile(file, 153, 7, 24);
            AssertShape(file.Shapes[0], "alphaShape", 20, 368, 260);
            AssertShapeData(file);
        }

        [SkippableFact]
        public void TestFS19()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 19");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
            AssertShapesFile(file, 201, 5, 9);
            AssertShape(file.Shapes[0], "colPartBackShape1", 4, 24, 12);
            AssertShapeData(file);
        }

        [SkippableFact]
        public void TestFS17_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 17");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
            AssertShapesFile(file, 49, 5, 12);
            AssertShape(file.Shapes[0], "wheelLoaderLogForkShape", 1, 24, 12);
            AssertShapeData(file);
        }

        [SkippableFact]
        public void TestFS15_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 15");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\grimme\grimmeFT300.i3d.shapes"));
            AssertShapesFile(file, 188, 3, 16);
            AssertShape(file.Shapes[0], "grimmeFTShape300", 1, 40, 20);
            AssertShapeData(file);
        }

        [SkippableFact]
        public void TestFS13_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 2013");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
            AssertShapesFile(file, 68, 2, 32);
            AssertShape(file.Shapes[0], "blanketBarShape2", 26, 68, 44);
            AssertShapeData(file);
        }
    }
}
