using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Tools;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Model
{
    /// <summary>
    /// Contains methods for loading and saving the contents of a .i3d.shapes file
    /// </summary>
    public class ShapesFile
    {
        private readonly ILogger? _logger;

        /// <summary>
        /// Cipher seed
        /// </summary>
        public byte? Seed { get; set; }

        /// <summary>
        /// File version
        /// </summary>
        public short? Version { get; set; }

        /// <summary>
        /// Parts in file
        /// </summary>
        public I3DPart[]? Parts { get; set; }

        /// <summary>
        /// Shapes in file
        /// </summary>
        public IEnumerable<I3DShape> Shapes => Parts.OfType<I3DShape>();

        /// <summary>
        /// Splines in file
        /// </summary>
        public IEnumerable<Spline> Splines => Parts.OfType<Spline>();

        public ShapesFile(ILogger? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Read the .i3d.shapes contents from the input stream. The contents will be populated in the Parts array.
        /// </summary>
        /// <param name="inputStream">Stream of shapes file data</param>
        /// <param name="forceSeed">Force a specific seed instead of the one specified in the file header</param>
        /// <param name="strict">Abort reading and propagate any exceptions that pop up when parsing part data. If false, parts that failed to read will be ignored.</param>
        public void Load(Stream inputStream, byte? forceSeed = null, bool strict = false)
        {
            using var _reader = new ShapesFileReader(inputStream, _logger, forceSeed);
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

        /// <summary>
        /// Write Parts data to the output stream as a .i3d.shapes file.
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <exception cref="ArgumentNullException">Thrown if Seed, Version or Parts is not set</exception>
        public void Write(Stream outputStream)
        {
            if (Seed == null || Version == null || Parts == null)
                throw new ArgumentNullException("Seed, Version and Parts must be set before saving.");

            using var writer = new ShapesFileWriter(outputStream, (byte)Seed, (short)Version);
            var entities = Parts.Select(part =>
            {
                using var ms = new MemoryStream();
                using var bw = new EndianBinaryWriter(ms, writer.Endian);

                part.Write(bw);

                bw.Flush();
                var data = ms.ToArray();

                return new Entity(part.RawType, data.Length, data);
            }).ToArray();

            writer.SaveEntities(entities);
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
    }
}
