using I3DShapesTool.Lib.Model.I3D;
using System.IO;
using Xunit;
using System.Linq;
using I3DShapesTool.Lib.Model;

namespace I3DShapesToolTest
{
    public class TestI3DXMLReader
    {
        private readonly I3D i3d;

        public TestI3DXMLReader()
        {
            string gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 22");

            i3d = I3DXMLReader.ParseXML(Path.Combine("TestData", "TestObject.i3d"));
        }

        [SkippableFact]
        public void TestHeader()
        {
            Assert.Equal("TheTestObject", i3d.Name);
            Assert.Equal("1.6", i3d.Version);
        }

        [SkippableFact]
        public void TestScene()
        {
            Assert.Equal(2, i3d.SceneRoot.Children.Count);
            Shape s1 = i3d.SceneRoot.Children.Where(t => t.Id == 1).Single() as Shape;
            Assert.Equal("the_main_component", s1.Name);
            Assert.Equal(1, s1.ShapeId);
            Assert.Equal(1, s1.Children.Count);

            Shape s2 = i3d.SceneRoot.Children.Where(t => t.Id == 5).Single() as Shape;
            Assert.Equal("a_different_component", s2.Name);
            Assert.Equal(3, s2.ShapeId);
            Assert.Equal(1, s2.Children.Count);
        }

        [SkippableFact]
        public void TestShapesLink()
        {
            Assert.Equal(Path.Combine("TestData", "aShapesFile.i3d.shapes"), i3d.ExternalShapesFile);
        }

        [SkippableFact]
        public void TestFiles()
        {
            Assert.Equal(3, i3d.Files.Length);
            Assert.Equal("a/different/file.jpg", i3d.Files.Where(f => f.FileId == 1).Single().Filename);
            Assert.Equal("some/file.png", i3d.Files.Where(f => f.FileId == 2).Single().Filename);
            Assert.Equal("a/third/file.xml", i3d.Files.Where(f => f.FileId == 3).Single().Filename);
        }

        [SkippableFact]
        public void TestMaterials()
        {
            Assert.Equal(3, i3d.Materials.Length);

            I3DMaterial m1 = i3d.Materials.Where(f => f.MaterialID == 1).Single();
            Assert.Equal("lambert1", m1.Name);
            Assert.Equal(new I3DVector4(0.5f, 0.5f, 0.5f, 1), (I3DVector4)m1.DiffuseColor, new I3DVector4Comparer());

            I3DMaterial m2 = i3d.Materials.Where(f => f.MaterialID == 2).Single();
            Assert.Equal("myFirstMat_mat", m2.Name);
            Assert.Equal(new I3DVector4(1, 1, 1, 1), (I3DVector4)m2.DiffuseColor, new I3DVector4Comparer());
            Assert.Equal(3, m2.ColorMats.Count);
            Assert.Equal(new I3DVector4(0.9215f, 0, 0.0704f, 1.0f), m2.ColorMats[0], new I3DVector4Comparer());
            Assert.Equal("a/different/file.jpg", m2.Normalmap?.Filename);

            I3DMaterial m3 = i3d.Materials.Where(f => f.MaterialID == 7).Single();
            Assert.Equal("a/third/file.xml", m3.Texture?.Filename);
        }
    }
}
