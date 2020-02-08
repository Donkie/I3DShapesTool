using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Model
{
    public class ShapesFile
    {
        private readonly ILogger _logger;

        private FileContainer _container;

        public string FilePath { get; private set; }

        public int Seed => _container.Header.Seed;
        public int Version => _container.Header.Version;
        public int ShapeCount { get; private set; }
        public I3DShape[] Shapes { get; private set; }
        public ICollection<Spline> Splines { get; private set; }

        public ShapesFile(ILogger logger = null)
        {
            _logger = logger;
        }

        public void Load(string path)
        {
            FilePath = path;
            _container = new FileContainer(path, _logger);

            _logger?.LogInformation($"Loading file: {Path.GetFileName(path)}");

            var entities = _container.GetEntities();
            var parts = _container
                        .ReadRawData(entities)
                        .Select(
                            (entityRaw, index) =>
                            {
                                try
                                {
                                    var convert = Convert(entityRaw, _container.Endian, _container.Header.Version);
                                    entityRaw.RawData = null;
                                    entityRaw.Entity.Dispose();
                                    return convert;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    _logger?.LogCritical(
                                        "Cant load parts {index}.",
                                        index
                                    );
                                }

                                return null;
                            }
                        )
                        .Where(part => part != null)
                        .ToArray();
            ShapeCount = parts.Length;

            Shapes = parts
                     .OfType<I3DShape>()
                     .ToArray();
            Splines = parts
                      .OfType<Spline>()
                      .ToArray();
        }

        private static I3DPart Convert((Entity Entity, byte[] RawData) entityRaw, Endian endian, int version)
        {
            var partType = GetPartType(entityRaw.Entity.Type);
            switch (partType)
            {
                case ShapeType.Shape:
                    return new I3DShape(entityRaw.RawData, endian, version);
                case ShapeType.Spline:
                    return new Spline(entityRaw.RawData, endian, version);
                case ShapeType.Unknown:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ShapeType GetPartType(int rawType)
        {
            switch (rawType)
            {
                case 1:
                    return ShapeType.Shape;
                case 2:
                    return ShapeType.Spline;
                default:
                    return ShapeType.Unknown;
            }
        }

        /// <summary>
        /// https://stackoverflow.com/a/7393722/2911165
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars()
                       .Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
