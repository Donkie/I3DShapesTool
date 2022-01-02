using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Tools;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Container
{
    public class ShapesFileReader : IDisposable
    {
        private readonly ILogger? _logger;
        private readonly CipherStream stream;
        private readonly BinaryReader reader;

        public ShapesFileReader(string filePath, ILogger? logger = null, byte? forceSeed = null)
        {
            FilePath = filePath;
            _logger = logger;

            if (!File.Exists(FilePath))
            {
                _logger?.LogCritical("File not found: {filePath}.", filePath);
                throw new FileNotFoundException("File not found.", filePath);
            }

            var fileStream = File.OpenRead(FilePath);

            Header = FileHeader.Read(fileStream);
            _logger?.LogDebug("File seed: {fileSeed}", Header.Seed);
            _logger?.LogDebug("File version: {version}", Header.Version);

            if (Header.Version < 2 || Header.Version > 7)
            {
                _logger?.LogCritical("Unsupported version: {version}", Header.Version);
                throw new NotSupportedException("Unsupported version");
            }

            Endian = GetEndian(Header.Version);

            if (forceSeed != null)
            {
                Header = new FileHeader(Header.Version, (byte)forceSeed);
            }

            stream = new CipherStream(fileStream, new I3DCipherDecryptor(Header.Seed));
            reader = new EndianBinaryReader(stream, Endian);
        }

        public string FilePath { get; }

        public FileHeader Header { get; private set; }

        public Endian Endian { get; private set; }

        public ICollection<Entity> GetEntities()
        {
            try
            {
                var countEntities = reader.ReadInt32();
                if (countEntities < 0 || countEntities > 1e6) // I don't think any i3d file would contain more than a million shapes..
                    throw new DecryptFailureException();

                return Enumerable.Range(0, countEntities)
                    .Select(v => Entity.Read(reader))
                    .ToArray();
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException || e is IOException || e is OutOfMemoryException)
                    throw new DecryptFailureException();
                throw;
            }
        }

        private static Endian GetEndian(short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }

        public void Dispose()
        {
            stream.Dispose();
            reader.Dispose();
        }
    }
}
