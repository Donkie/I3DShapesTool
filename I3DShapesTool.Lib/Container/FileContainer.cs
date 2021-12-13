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
        private readonly IDecryptor _decryptor;

        public FileContainer(string filePath, ILogger logger = null, byte? forceSeed = null)
        {
            FilePath = filePath;
            _logger = logger;


            if (!File.Exists(FilePath))
            {
                _logger?.LogCritical("File not found: {filePath}.", filePath);
                throw new FileNotFoundException("File not found.", filePath);
            }

            Initialize();

            if (forceSeed != null)
            {
                Header.Seed = (byte) forceSeed;
            }

            _decryptor = new Decryptor(Header.Seed);
        }

        private void Initialize()
        {
            Header = ReadHeader();
            _logger?.LogDebug("File seed: {fileSeed}", Header.Seed);
            _logger?.LogDebug("File version: {version}", Header.Version);

            if (Header.Version < 2 || Header.Version > 7)
            {
                _logger?.LogCritical("Unsupported version: {version}", Header.Version);
                throw new NotSupportedException("Unsupported version");
            }

            Endian = GetEndian(Header.Version);
        }

        public string FilePath { get; }

        public FileHeader Header { get; private set; }

        public Endian Endian { get; private set; }

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

        private byte[] ReadRawData(Stream stream, Entity entity)
        {
            stream.Seek(entity.OffsetRawBlock, SeekOrigin.Begin);
            var buffer = stream.ReadBytes(entity.Size);
            _decryptor.Decrypt(buffer, entity.DecryptIndexBlock);
            return buffer;
        }

        /// <summary>
        /// Read <inheritdoc cref="FileHeader"/> by file name
        /// </summary>
        /// <returns></returns>
        private FileHeader ReadHeader()
        {
            using (var stream = File.OpenRead(FilePath))
                return FileHeader.Read(stream);
        }

        public ICollection<Entity> GetEntities()
        {
            try
            {
                using (var stream = File.OpenRead(FilePath)) // TODO: Don't open the file stream twice for the parsing process
                {
                    FileHeader.Read(stream);

                    var cryptBlockIndex = 0ul;

                    var countEntities = _decryptor.ReadInt32(stream, cryptBlockIndex, ref cryptBlockIndex, Endian);
                    if(countEntities < 0 || countEntities > 1e6) // I don't think any i3d file would contain more than a million shapes..
                        throw new DecryptFailureException();

                    return Enumerable.Range(0, countEntities)
                        .Select(v => Entity.Read(stream, _decryptor, ref cryptBlockIndex, Endian))
                        .ToArray();
                }
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
    }
}
