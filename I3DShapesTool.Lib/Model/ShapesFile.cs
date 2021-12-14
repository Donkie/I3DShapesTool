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
        private readonly ILogger? _logger;

        private FileContainer? _container;

        public string? FilePath { get; private set; }

        public int? Seed => _container?.Header.Seed;
        public int? Version => _container?.Header.Version;
        public I3DPart[]? Parts { get; private set; }
        public IEnumerable<I3DShape> Shapes => Parts.OfType<I3DShape>();
        public IEnumerable<Spline> Splines => Parts.OfType<Spline>();

        public ShapesFile(ILogger? logger = null)
        {
            _logger = logger;
        }

        public void Load(string path, byte? forceSeed = null)
        {
            FilePath = path;
            _container = new FileContainer(path, _logger, forceSeed);

            var entities = _container.GetEntities();
            Parts = _container
                        .ReadRawData(entities)
                        .Select(
                            (entityRaw, index) =>
                            {
                                try
                                {
                                    var part = Convert(entityRaw, _container.Endian, _container.Header.Version);
                                    if(part.Type == ShapeType.Unknown)
                                    {
                                        _logger?.LogInformation("Found part named {name} with unknown type {type}.", part.Name, part.RawType);
                                    }
                                    return part;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    _logger?.LogCritical("Failed to load part {index}.", index);
                                    return null;
                                }
                            }
                        )
                        .Where(part => part != null)
                        .Cast<I3DPart>()
                        .ToArray();
        }

        private static I3DPart Convert((Entity Entity, byte[] RawData) entityRaw, Endian endian, int version)
        {
            var partType = GetPartType(entityRaw.Entity.Type);
            return partType switch
            {
                ShapeType.Shape => new I3DShape(entityRaw.RawData, endian, version),
                ShapeType.Spline => new Spline(entityRaw.RawData, endian, version),
                ShapeType.Unknown => new I3DPart(entityRaw.Entity.Type, entityRaw.RawData, endian, version),
                _ => throw new ArgumentOutOfRangeException(),
            };
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
