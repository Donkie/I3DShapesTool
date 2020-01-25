using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool
{
    public class I3DShapesFile
    {
        private readonly ILogger _logger;

        public string FilePath { get; private set; }
        public string FileName => Path.GetFileName(FilePath);
        public int Seed { get; private set; }
        public int Version { get; private set; }
        public Endian FileEndian => Version >= 4 ? Endian.Little : Endian.Big;
        public int PartsCount { get; private set; }
        public I3DShape[] Shapes { get; private set; }
        public ICollection<I3DSpline> Splines { get; private set; }

        public I3DShapesFile(ILogger logger = null)
        {
            _logger = logger;
        }

        private IEnumerable<I3DPart> LoadParts(I3DDecryptorStream dfs, int count)
        {
            return Enumerable.Range(0, count)
                .Select(index => I3DPart.Read(dfs, FileEndian, Version));
        }

        private static I3DPart Convert(I3DPart part)
        {
            switch (part.Type)
            {
                case I3DPartType.Unknown:
                    return part;
                case I3DPartType.Shape:
                    return new I3DShape(part);
                case I3DPartType.Spline:
                    return new I3DSpline(part);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Load(string path)
        {
            FilePath = path;

            _logger?.LogInformation($"Loading file: {FileName}");

            using (var fs = File.OpenRead(path))
            {
                var header = new I3DShapesHeader(fs);
                Seed = header.Seed;
                Version = header.Version;

                _logger?.LogDebug($"File seed: {Seed}");
                _logger?.LogDebug($"File version: {Version}");

                if (header.Version < 2 || header.Version > 5)
                    throw new NotSupportedException("Unsupported version");

                using (var dfs = new I3DDecryptorStream(fs, header.Seed))
                {
                    PartsCount = dfs.ReadInt32(FileEndian);

                    _logger?.LogInformation($"Found {PartsCount} parts");

                    var loadParts = LoadParts(dfs, PartsCount)
                        .ToArray();

                    // Convert to typed parts
                    loadParts = loadParts
                        .Select((v, i) =>
                        {
                            try
                            {
                                return Convert(v);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                _logger?.LogCritical("Cant load parts {index}", i);
                            }

                            return null;
                        })
                        .Where(v => v != null)
                        .ToArray();
                    Shapes = loadParts
                        .OfType<I3DShape>()
                        .ToArray();
                    Splines = loadParts
                        .OfType<I3DSpline>()
                        .ToArray();
                }
            }
        }
    }
}
