using System;
using System.IO;
using System.ServiceModel.Channels;
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
        public int ShapeCount { get; private set; }
        public I3DShape[] Shapes { get; private set; }

        public I3DShapesFile(ILogger logger = null)
        {
            _logger = logger;
        }

        private I3DShape LoadShape(I3DDecryptorStream dfs)
        {
            var type = dfs.ReadInt32(FileEndian);
            var size = dfs.ReadInt32(FileEndian);

            var data = dfs.ReadBytes(size);

            var shape = new I3DShape(type, size, data);

            using (var ms = new MemoryStream(data))
            {
                var br = FileEndian == Endian.Big ? new BigEndianBinaryReader(ms) : new BinaryReader(ms);
                shape.Load(br, Version);
                br.Dispose();
            }

            _logger?.LogInformation($"Shape {shape.ShapeId} ({shape.Name}, Type {shape.Type})");

            return shape;
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
                    ShapeCount = dfs.ReadInt32(FileEndian);
                    Shapes = new I3DShape[ShapeCount];

                    _logger?.LogInformation($"Found {ShapeCount} shapes");

                    for (var i = 0; i < ShapeCount; i++)
                    {
                        Shapes[i] = LoadShape(dfs);
                    }
                }
            }
        }
    }
}
