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
        public UnitTest()
        {
        }

        private static void AssertShapesFile(ShapesFile file, byte seed, short version, int shapeCount)
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

            if(shape.Normals != null)
            {
                Assert.Equal(vertexCount, shape.Normals.Length);
            }

            foreach(I3DUV[] uvSet in shape.UVSets)
            {
                if(uvSet != null)
                {
                    Assert.Equal(vertexCount, uvSet.Length);
                }
            }
        }

        private static void AssertShapeData(ShapesFile file)
        {
            foreach(I3DShape shape in file.Shapes)
            {
                uint maxIndex = shape.VertexCount;
                Assert.True(shape.Triangles.All(tri => tri.P1Idx <= maxIndex && tri.P2Idx <= maxIndex && tri.P3Idx <= maxIndex));
                if(shape.Normals != null)
                {
                    double numUnitLengths = shape.Normals.Sum(v => v.IsValidNormal() ? 1 : 0);
                    // The data files can contain some bad normals, but most of them should be good
                    Assert.True(numUnitLengths / shape.Normals.Length > 0.95);
                    Assert.True(shape.Normals.First().IsValidNormal());
                    Assert.True(shape.Normals.Last().IsValidNormal());
                }
            }
        }

        /// <summary>
        /// Tests rewriting the part data into bytes and comparing with original (doesn't test decryption/encryption)
        /// </summary>
        /// <param name="file"></param>
        private static void TestDataRewrite(ShapesFile file)
        {
            foreach(I3DPart part in file.Parts)
            {
                byte[] originalRaw = part.RawData;

                using MemoryStream ms = new MemoryStream();
                using EndianBinaryWriter bw = new EndianBinaryWriter(ms, part.Endian);
                part.Write(bw);
                bw.Flush();

                byte[] newRaw = ms.ToArray();
                /*
                //Useful for debugging but a bit slow so leaving commented
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
            foreach(string file in Directory.GetFiles(baseDir))
            {
                if(file.EndsWith(".i3d.shapes"))
                {
                    outData.Add(file);
                }
            }

            foreach(string dir in Directory.GetDirectories(baseDir))
            {
                FindShapesFiles(dir, outData);
            }
        }

        /// <summary>
        /// Decrypts and parses a .i3d.shapes file and then encrypts it back and compares the binaries
        /// </summary>
        /// <param name="filePath"></param>
        private void TestFullRewriteShapes(string filePath)
        {
            // Read, decrypt and parse the .i3d.shapes data
            using FileStream fileStream = File.OpenRead(filePath);
            ShapesFile file = new ShapesFile();
            file.Load(fileStream, null, true);

            // Write and encrypt the shape data into a memory buffer
            using MemoryStream ms = new MemoryStream();
            file.Write(ms);
            byte[] rewrittenData = ms.ToArray();

            // Compare the raw data
            byte[] originalData = File.ReadAllBytes(filePath);
            Assert.Equal(originalData.Length, rewrittenData.Length);
            Assert.Equal(originalData, rewrittenData);

            // Re-decrypt the data and compare the contents.
            // This is a redundant step since we've already confirmed the binaries match perfectly
            // But nice anyway
            ms.Seek(0, SeekOrigin.Begin);

            ShapesFile file2 = new ShapesFile();
            file2.Load(ms, null, true);

            Assert.Equal(file.Seed, file2.Seed);
            Assert.Equal(file.Version, file2.Version);
            Assert.Equal(file.Parts.Length, file2.Parts.Length);
            Assert.Equal(file.Parts[0].RawData, file2.Parts[0].RawData);
        }

        [SkippableFact]
        public void TestFS22WriteShapes()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 22");

            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\boeckmann\bigMasterWesternWCF\bigMasterWesternWCF.i3d.shapes"));
            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\newHolland\chSeries\chSeries.i3d.shapes"));
            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\hardi\mega1200L\mega1200L.i3d.shapes"));
        }

        [SkippableFact]
        public void TestFS22()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 22");

            {
                using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\boeckmann\bigMasterWesternWCF\bigMasterWesternWCF.i3d.shapes"));
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapesFile(file, 153, 7, 24);
                AssertShape(file.Shapes.First(), "alphaShape", 20, 368, 260);
                AssertShapeData(file);
                TestDataRewrite(file);
            }
            {
                using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\newHolland\chSeries\chSeries.i3d.shapes"));
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapesFile(file, 117, 7, 113);
                AssertShape(file.Shapes.First(), "alpha1Shape", 15, 52, 40);
                AssertShapeData(file);
                TestDataRewrite(file);
            }
        }

        [SkippableFact]
        public void TestFS22Full()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 22");

            HashSet<string> shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(string filePath in shapeFiles)
            {
                using FileStream fileStream = File.OpenRead(filePath);
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS19WriteShapes()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 19");

            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
        }

        [SkippableFact]
        public void TestFS19()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 19");

            using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\magsi\telehandlerBaleFork\telehandlerBaleFork.i3d.shapes"));
            ShapesFile file = new ShapesFile();
            file.Load(fileStream, null, true);
            AssertShapesFile(file, 201, 5, 9);
            AssertShape(file.Shapes.First(), "colPartBackShape1", 4, 24, 12);
            AssertShapeData(file);
            TestDataRewrite(file);
        }

        [SkippableFact]
        public void TestFS19Full()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 19");

            HashSet<string> shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(string filePath in shapeFiles)
            {
                using FileStream fileStream = File.OpenRead(filePath);
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS17WriteShapes()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 17");

            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
        }

        [SkippableFact]
        public void TestFS17()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 17");

            using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
            ShapesFile file = new ShapesFile();
            file.Load(fileStream, null, true);
            AssertShapesFile(file, 49, 5, 12);
            AssertShape(file.Shapes.First(), "wheelLoaderLogForkShape", 1, 24, 12);
            AssertShapeData(file);
            TestDataRewrite(file);
        }

        [SkippableFact]
        public void TestFS17Full()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 17");

            HashSet<string> shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(string filePath in shapeFiles)
            {
                using FileStream fileStream = File.OpenRead(filePath);
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS15WriteShapes()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 15");

            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\tools\magsi\wheelLoaderLogFork.i3d.shapes"));
        }

        [SkippableFact]
        public void TestFS15()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 15");

            using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\tools\grimme\grimmeFT300.i3d.shapes"));
            ShapesFile file = new ShapesFile();
            file.Load(fileStream, null, true);
            AssertShapesFile(file, 188, 3, 16);
            AssertShape(file.Shapes.First(), "grimmeFTShape300", 1, 40, 20);
            AssertShapeData(file);
            TestDataRewrite(file);
        }

        [SkippableFact]
        public void TestFS15Full()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 15");

            HashSet<string> shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(string filePath in shapeFiles)
            {
                using FileStream fileStream = File.OpenRead(filePath);
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapeData(file);
            }
        }

        [SkippableFact]
        public void TestFS13WriteShapes()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 2013");

            TestFullRewriteShapes(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
        }

        [SkippableFact]
        public void TestFS13()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 2013");

            using FileStream fileStream = File.OpenRead(Path.Combine(gameFolder, @"data\vehicles\tools\kuhn\kuhnGA4521GM.i3d.shapes"));
            ShapesFile file = new ShapesFile();
            file.Load(fileStream, null, true);
            AssertShapesFile(file, 68, 2, 32);
            AssertShape(file.Shapes.First(), "blanketBarShape2", 26, 68, 44);
            AssertShapeData(file);
            TestDataRewrite(file);
        }

        [SkippableFact]
        public void TestFS13Full()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 2013");

            HashSet<string> shapeFiles = new HashSet<string>();
            FindShapesFiles(Path.Combine(gameFolder, "data"), shapeFiles);

            foreach(string filePath in shapeFiles)
            {
                using FileStream fileStream = File.OpenRead(filePath);
                ShapesFile file = new ShapesFile();
                file.Load(fileStream, null, true);
                AssertShapeData(file);
            }
        }
    }
}
