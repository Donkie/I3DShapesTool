using System.IO;
using Xunit;
using I3DShapesTool.Lib.Model;
using System.Linq;
using I3DShapesTool.Lib.Tools;
using System.Collections.Generic;

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
            Assert.Equal(shapeCount, file.Parts.Length);
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
                // This UV check works in 99.9% percent of cases but some models just have extremely wacky UVs which means we can't rely on this test :(
                //Assert.True(shape.UVSets.All(uvSet => uvSet == null || uvSet.All(uv => uv.U >= -MaxUV && uv.U <= MaxUV && uv.V >= -MaxUV && uv.V <= MaxUV)));

                Assert.True(shape.Triangles.All(tri => tri.P1Idx <= shape.CornerCount && tri.P2Idx <= shape.CornerCount && tri.P3Idx <= shape.CornerCount));
                if (shape.Normals != null)
                {
                    double numUnitLengths = shape.Normals.Sum(v => v.IsValidNormal() ? 1 : 0);
                    // The data files can contain some bad normals, but most of them should be good
                    Assert.True(numUnitLengths / shape.Normals.Length > 0.95);
                    Assert.True(shape.Normals.First().IsValidNormal());
                    Assert.True(shape.Normals.Last().IsValidNormal());
                }
            }
        }

        private static void TestRewrite(ShapesFile file)
        {
            foreach (var part in file.Parts)
            {
                var originalRaw = part.RawData;

                using var ms = new MemoryStream();
                using var bw = new EndianBinaryWriter(ms, part.Endian);
                part.Write(bw);
                bw.Flush();

                var newRaw = ms.ToArray();
                /*
                //Useful for debugging but a bit slow so leaving uncommented
                for(var i = 0; i < originalRaw.Length; i++)
                {
                    Assert.Equal(originalRaw[i], newRaw[i]);
                }
                */
                Assert.Equal(originalRaw.Length, newRaw.Length);
                Assert.Equal(originalRaw, newRaw);
            }
        }

        private static void FindShapesFiles(string baseDir, ISet<string> outData)
        {
            foreach(var file in Directory.GetFiles(baseDir))
            {
                if (file.EndsWith(".i3d.shapes"))
                {
                    outData.Add(file);
                }
            }

            foreach(var dir in Directory.GetDirectories(baseDir))
            {
                FindShapesFiles(dir, outData);
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
            AssertShape(file.Shapes.First(), "alphaShape", 20, 368, 260);
            AssertShapeData(file);
            TestRewrite(file);
        }

        [SkippableFact]
        public void TestFS22Full()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 22");
            Skip.If(gameFolder == null);

            var shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(var filePath in shapeFiles)
            {
                var file = new ShapesFile();
                file.Load(filePath);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS19()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 19");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
            AssertShapesFile(file, 201, 5, 9);
            AssertShape(file.Shapes.First(), "colPartBackShape1", 4, 24, 12);
            AssertShapeData(file);
            TestRewrite(file);
        }

        [SkippableFact]
        public void TestFS19Full()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 19");
            Skip.If(gameFolder == null);

            var shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach (var filePath in shapeFiles)
            {
                var file = new ShapesFile();
                file.Load(filePath);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS17()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 17");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
            AssertShapesFile(file, 49, 5, 12);
            AssertShape(file.Shapes.First(), "wheelLoaderLogForkShape", 1, 24, 12);
            AssertShapeData(file);
            TestRewrite(file);
        }

        [SkippableFact]
        public void TestFS17Full()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 17");
            Skip.If(gameFolder == null);

            var shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach (var filePath in shapeFiles)
            {
                var file = new ShapesFile();
                file.Load(filePath);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS15()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 15");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\grimme\grimmeFT300.i3d.shapes"));
            AssertShapesFile(file, 188, 3, 16);
            AssertShape(file.Shapes.First(), "grimmeFTShape300", 1, 40, 20);
            AssertShapeData(file);
            TestRewrite(file);
        }

        [SkippableFact]
        public void TestFS15Full()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 15");
            Skip.If(gameFolder == null);

            var shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach (var filePath in shapeFiles)
            {
                var file = new ShapesFile();
                file.Load(filePath);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS13()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 2013");
            Skip.If(gameFolder == null);

            var file = new ShapesFile();
            file.Load(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
            AssertShapesFile(file, 68, 2, 32);
            AssertShape(file.Shapes.First(), "blanketBarShape2", 26, 68, 44);
            AssertShapeData(file);
            TestRewrite(file);
        }

        [SkippableFact]
        public void TestFS13Full()
        {
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 2013");
            Skip.If(gameFolder == null);

            var shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach (var filePath in shapeFiles)
            {
                var file = new ShapesFile();
                file.Load(filePath);
                AssertShapeData(file);
            }
        }
    }
}
