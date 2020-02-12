using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Container;
using I3dShapes.Model;
using I3dShapes.Model.Contract;
using I3dShapes.Tools;
using I3DShapesToolTest.Model;
using I3DShapesToolTest.Tools;
using MoreLinq;
using Xunit;

namespace I3DShapesToolTest
{
    public class UnitTest
    {
        /// <summary>
        /// Read ALL shapes in game directory.
        /// </summary>
        [Fact]
        public void ReadAllShapes()
        {
            var versions = new[]
            {
                FarmSimulatorVersion.FarmingSimulator2013,
                FarmSimulatorVersion.FarmingSimulator2015,
                FarmSimulatorVersion.FarmingSimulator2017,
                FarmSimulatorVersion.FarmingSimulator2019,
            };
            var hasError = false;
            versions
                .Select(version => (Version: version, Path: GamePaths.GetGamePath(version)))
                .Where(v => v.Path != null)
                .SelectMany(
                    v =>
                    {
                        var shapeFiles = Directory.GetFiles(v.Path, $"*{GameConstants.SchapesFileExtension}", SearchOption.AllDirectories);
                        return shapeFiles.Select(file => (Version: v.Version, FilePath: file));
                    }
                )
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForEach(
                    v =>
                    {
                        try
                        {
                            var container = new FileContainer(v.FilePath);
                            var entities = container.GetEntities();
                            foreach (var valueTuple in container.ReadRawData(entities))
                            {
                                using (var stream = new MemoryStream(valueTuple.RawData))
                                {
                                    try
                                    {
                                        using (var reader = new EndianBinaryReader(stream, container.Endian, true))
                                        {
                                            switch (valueTuple.Entity.Type)
                                            {
                                                case 1:
                                                    var shape = new Shape(reader, container.Header.Version);
                                                    break;
                                                case 2:
                                                    var spline = new Spline(reader);
                                                    break;
                                                case 3:
                                                    var mesh = new NavMesh(reader);
                                                    break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        hasError = true;
                                        stream.Seek(0, SeekOrigin.Begin);
                                        using (var reader = new EndianBinaryReader(stream, container.Endian))
                                        {
                                            SaveErrorShape(
                                                v.Version,
                                                v.FilePath,
                                                new RawNamedShapeObject(valueTuple.Entity.Type, reader, container.Endian)
                                            );
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                        }
                    }
                );

            Assert.False(hasError);
        }

        /// <summary>
        /// Save error parse shape in directory.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="shapeFileName"></param>
        /// <param name="rawShape"></param>
        private static void SaveErrorShape(FarmSimulatorVersion version, string shapeFileName, IRawNamedShapeObject rawShape)
        {
            var curentPath = Directory.GetCurrentDirectory();
            var outputPath = "Output";
            var outputDirectory = Path.Combine(
                curentPath,
                outputPath,
                version.ToString(),
                Path.GetFileName(shapeFileName)
                    .Replace(GameConstants.SchapesFileExtension, "")
            );
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var fileName = $"[{rawShape.Id}]_[{rawShape.RawType}]_{FileTool.CleanFileName(rawShape.Name)}.bin";
            File.WriteAllBytes(Path.Combine(outputDirectory, fileName), rawShape.RawData);
        }
    }
}
