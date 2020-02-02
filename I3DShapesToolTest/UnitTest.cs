using System;
using System.IO;
using Xunit;
using I3DShapesTool;

namespace I3DShapesToolTest
{
    public class UnitTest
    {
        public UnitTest()
        {
        }

        private static void AssertShapesFile(I3DShapesFile file, int seed, int version, int shapeCount)
        {
            Assert.Equal(seed, file.Seed);
            Assert.Equal(version, file.Version);
            Assert.Equal(shapeCount, file.ShapeCount);
        }

        private static void AssertShape(I3DShape shape, string name, uint shapeId, int vertexCount, int faceCount)
        {
            Assert.Equal(name, shape.Name);
            Assert.Equal(shapeId, shape.ShapeId);
            Assert.Equal(vertexCount, shape.Positions.Length);
            Assert.Equal(vertexCount, shape.Normals.Length);
            Assert.Equal(vertexCount, shape.UVs.Length);
            Assert.Equal(faceCount, shape.Triangles.Length);
        }

        [SkippableFact]
        public void TestFS19()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 19");
            Skip.If(gameFolder == null);

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
            AssertShapesFile(file, 201, 5, 9);
            AssertShape(file.Shapes[0], "colPartBackShape1", 4, 24, 12);
        }

        [SkippableFact]
        public void TestFS17_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 17");
            Skip.If(gameFolder == null);

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
            AssertShapesFile(file, 49, 5, 12);
            AssertShape(file.Shapes[0], "wheelLoaderLogForkShape", 1, 24, 12);
        }

        [SkippableFact]
        public void TestFS15_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 15");
            Skip.If(gameFolder == null);

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\grimme\grimmeFT300.i3d.shapes"));
            AssertShapesFile(file, 188, 3, 16);
            AssertShape(file.Shapes[0], "grimmeFTShape300", 1, 40, 20);
        }

        [SkippableFact]
        public void TestFS13_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 2013");
            Skip.If(gameFolder == null);

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
            AssertShapesFile(file, 68, 2, 32);
            AssertShape(file.Shapes[0], "blanketBarShape2", 26, 68, 44);
        }
    }
}
