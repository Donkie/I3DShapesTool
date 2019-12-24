using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using I3DShapesTool;

namespace I3DShapesToolTest
{
    [TestClass]
    public class UnitTest
    {
        [TestInitialize]
        public void TestInit()
        {
            SteamHelper.LoadSteamGameDirectories();
        }

        private static void AssertShapesFile(I3DShapesFile file, int seed, int version, int shapeCount)
        {
            Assert.AreEqual(seed, file.Seed, "Unexpected seed");
            Assert.AreEqual(version, file.Version, "Unexpected version");
            Assert.AreEqual(shapeCount, file.ShapeCount, "Unexpected shape count");
        }

        private static void AssertShape(I3DShape shape, string name, uint shapeId, int vertexCount, int faceCount)
        {
            Assert.AreEqual(name, shape.Name);
            Assert.AreEqual(shapeId, shape.ShapeId);
            Assert.AreEqual(vertexCount, shape.Positions.Length);
            Assert.AreEqual(vertexCount, shape.Normals.Length);
            Assert.AreEqual(vertexCount, shape.UVs.Length);
            Assert.AreEqual(faceCount, shape.Triangles.Length);
        }

        [TestMethod]
        public void TestFS19_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 19");

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
            AssertShapesFile(file, 201, 5, 9);
            AssertShape(file.Shapes[0], "colPartBackShape1", 4, 24, 12);
        }

        [TestMethod]
        public void TestFS17_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 17");

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
            AssertShapesFile(file, 49, 5, 12);
            AssertShape(file.Shapes[0], "wheelLoaderLogForkShape", 1, 24, 12);
        }

        [TestMethod]
        public void TestFS15_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 15");

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\grimme\grimmeFT300.i3d.shapes"));
            AssertShapesFile(file, 188, 3, 16);
            AssertShape(file.Shapes[0], "grimmeFTShape300", 1, 40, 20);
        }

        [TestMethod]
        public void TestFS13_1()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 2013");

            var file = new I3DShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
            AssertShapesFile(file, 68, 2, 32);
            AssertShape(file.Shapes[0], "blanketBarShape2", 26, 68, 44);
        }
    }
}
