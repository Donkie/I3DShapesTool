using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Container
{
    public class FileContainer
    {
        private readonly ILogger _logger;
        private readonly I3DDecryptor _decryptor;

        public FileContainer(string filePath, ILogger logger = null)
        {
            FilePath = filePath;
            _logger = logger;
            _decryptor = new I3DDecryptor();


            if (!File.Exists(FilePath))
            {
                _logger?.LogCritical("File not found: {filePath}.", filePath);
                throw new FileNotFoundException("File not found.", filePath);
            }

            Header = ReadHeader(FilePath);

            if (Header.Version < 2 || Header.Version > 5)
            {
                logger?.LogCritical("Unsupported version: {version}", Header.Version);
                throw new NotSupportedException("Unsupported version");
            }
            Endian = GetEndian(Header.Version);
        }

        public string FilePath { get; }

        public FileHeader Header { get; private set; }

        public Endian Endian { get; private set; }

        public ICollection<Entity> GetEntities()
        {
            return ReadEntities(_decryptor, FilePath);
        }

        public IEnumerable<(Entity Entity, byte[] RawData)> ReadRawData(IEnumerable<Entity> entities)
        {
            using (var stream = File.OpenRead(FilePath))
            {
                foreach (var entity in entities)
                {
                    yield return (Entity: entity, RawData: ReadRawData(stream, entity));
                }
            }
        }

        public byte[] ReadRawData(Entity entity)
        {
            using (var stream = File.OpenRead(FilePath))
            {
                return ReadRawData(stream, entity);
            }
        }

        private byte[] ReadRawData(Stream stream, Entity entity)
        {
            stream.Seek(entity.OffsetRawBlock, SeekOrigin.Begin);
            var buffer = stream.ReadBytes(entity.Size);
            _decryptor.Decrypt(buffer, entity.DecryptIndexBlock);
            return buffer;
        }

        private static FileHeader ReadHeader(string fileName, ILogger logger = null)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return ReadHeader(stream, logger);
            }
        }

        private static FileHeader ReadHeader(Stream stream, ILogger logger = null)
        {
            var readHeader = FileHeader.Read(stream);
            logger?.LogDebug("File seed: {fileSeed}", readHeader.Seed);
            logger?.LogDebug("File version: {version}", readHeader.Version);
            return readHeader;
        }

        private static ICollection<Entity> ReadEntities(I3DDecryptor decryptor, string fileName, ILogger logger = null)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var header = ReadHeader(stream);
                var endian = GetEndian(header.Version);

                decryptor.Init(header.Seed);

                var cryptBlockIndex = 0ul;

                var countEntities = decryptor.ReadInt32(stream, cryptBlockIndex, ref cryptBlockIndex, endian);

                return Enumerable.Range(0, countEntities)
                    .Select(v => Entity.Read(stream, decryptor, ref cryptBlockIndex, endian))
                    .ToArray();
            }
        }

        private static Endian GetEndian(short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }
    }
}
