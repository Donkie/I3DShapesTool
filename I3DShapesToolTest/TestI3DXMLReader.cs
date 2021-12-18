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
            var gameFolder = SteamHelper.GetGameDirectory("Farming Simulator 22");
            Skip.If(gameFolder == null);

            I3DXMLReader.ParseXML(Path.Combine(gameFolder, @"data\vehicles\boeckmann\bigMasterWesternWCF\bigMasterWesternWCF.i3d"));
        }
    }
}
