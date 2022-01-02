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

        public string FilePath { get; }

        public int? Seed { get; private set; }
        public int? Version { get; private set; }
        public I3DPart[]? Parts { get; private set; }
        public IEnumerable<I3DShape> Shapes => Parts.OfType<I3DShape>();
        public IEnumerable<Spline> Splines => Parts.OfType<Spline>();

        public ShapesFile(string path, ILogger? logger = null)
        {
            FilePath = path;
            _logger = logger;
        }

        public void Load(byte? forceSeed = null, bool strict = false)
        {
            using var _reader = new ShapesFileReader(FilePath, _logger, forceSeed);
            Seed = _reader.Header.Seed;
            Version = _reader.Header.Version;

            var entities = _reader.GetEntities();
            Parts = entities
                        .Select(
                            (entityRaw, index) =>
                            {
                                try
                                {
                                    var partType = GetPartType(entityRaw.Type);
                                    var part = LoadPart(entityRaw, partType, _reader.Endian, _reader.Header.Version);
                                    if(part.Type == ShapeType.Unknown)
                                    {
                                        _logger?.LogInformation("Found part named {name} with unknown type {type}.", part.Name, part.RawType);
                                    }
                                    return part;
                                }
                                catch (Exception ex)
                                {
                                    if (strict)
                                        throw;

                                    Console.WriteLine(ex);
                                    _logger?.LogError("Failed to decode part {index}.", index);

                                    // Failed to decode as the real part type, load it as a generic I3DPart instead so we at least can get hold of the binary data
                                    try
                                    {
                                        return LoadPart(entityRaw, ShapeType.Unknown, _reader.Endian, _reader.Header.Version);
                                    }
                                    catch (Exception)
                                    {
                                        // We even failed to decode it as a generic part, just return null then instead.
                                        return null;
                                    }
                                }
                            }
                        )
                        .Where(part => part != null)
                        .Cast<I3DPart>()
                        .ToArray();
        }

        private static I3DPart LoadPart(Entity entityRaw, ShapeType partType, Endian endian, int version)
        {
            return partType switch
            {
                ShapeType.Shape => new I3DShape(entityRaw.Data, endian, version),
                ShapeType.Spline => new Spline(entityRaw.Data, endian, version),
                ShapeType.Unknown => new I3DPart(entityRaw.Type, entityRaw.Data, endian, version),
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
