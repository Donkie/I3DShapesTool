using I3DShapesTool.Lib.Model.I3D;
using System.IO;
using Xunit;

namespace I3DShapesToolTest
{
    public class TestI3DXMLReader
    {
        [SkippableFact]
        public void TestRead()
        {
            var gameFolder = SteamHelper.GetGameDirectoryOrSkip("Farming Simulator 22");

            I3DXMLReader.ParseXML(Path.Combine(gameFolder, @"data\vehicles\boeckmann\bigMasterWesternWCF\bigMasterWesternWCF.i3d"));
        }
    }
}
