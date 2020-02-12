using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Tools;
using I3dShapes.Tools.Extensions;
using Microsoft.Extensions.Logging;

namespace I3dShapes.Container
{
    public class FileContainer
    {
        private readonly ILogger _logger;
        private readonly IDecryptor _decryptor;

        public FileContainer(string filePath, ILogger logger = null)
        {
            FilePath = filePath;
            _logger = logger;


            if (!File.Exists(FilePath))
            {
                _logger?.LogCritical("File not found: {filePath}.", filePath);
                throw new FileNotFoundException("File not found.", filePath);
            }

            Initialize();

            _decryptor = new Decryptor(Header.Seed);
        }

        private void Initialize()
        {
            Header = ReadHeader(FilePath);
            _logger?.LogDebug("File seed: {fileSeed}", Header.Seed);
            _logger?.LogDebug("File version: {version}", Header.Version);

            if (Header.Version < 2 || Header.Version > 5)
            {
                _logger?.LogCritical("Unsupported version: {version}", Header.Version);
                throw new NotSupportedException("Unsupported version");
            }

            Endian = GetEndian(Header.Version);
            IsEncrypted = GetIsEncrypted(Header);
        }

        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath { get; }

        public FileHeader Header { get; private set; }

        public bool IsEncrypted { get; set; }

        public Endian Endian { get; private set; }

        /// <summary>
        /// Read all <see cref="Entity"/> in file.
        /// </summary>
        /// <returns>Collection <see cref="Entity"/></returns>
        public ICollection<Entity> GetEntities()
        {
            return ReadEntities(_decryptor, FilePath, IsEncrypted);
        }

        public IEnumerable<(Entity Entity, byte[] RawData)> ReadRawData(IEnumerable<Entity> entities)
        {
            using var stream = File.OpenRead(FilePath);
            foreach (var entity in entities)
            {
                yield return (Entity: entity, RawData: ReadRawData(stream, entity));
            }
        }

        public byte[] ReadRawData(Entity entity)
        {
            using var stream = File.OpenRead(FilePath);
            return ReadRawData(stream, entity);
        }

        private byte[] ReadRawData(Stream stream, Entity entity)
        {
            stream.Seek(entity.OffsetRawBlock, SeekOrigin.Begin);
            var buffer = stream.Read((int)entity.Size);

            if (IsEncrypted)
            {
                _decryptor.Decrypt(buffer, entity.DecryptIndexBlock);
            }

            return buffer;
        }

        /// <summary>
        /// Read <inheritdoc cref="FileHeader"/> by file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static FileHeader ReadHeader(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return ReadHeader(stream);
            }
        }

        /// <summary>
        /// Read <inheritdoc cref="FileHeader"/> by <inheritdoc cref="Stream"/>
        /// </summary>
        /// <param name="stream"><inheritdoc cref="Stream"/></param>
        /// <returns><inheritdoc cref="FileHeader"/></returns>
        private static FileHeader ReadHeader(Stream stream)
        {
            var readHeader = FileHeader.Read(stream);
            return readHeader;
        }

        /// <summary>
        /// Read all <see cref="Entity"/> in file.
        /// </summary>
        /// <param name="decryptor"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static ICollection<Entity> ReadEntities(IDecryptor decryptor, in string fileName, bool isEncrypted = true)
        {
            using var stream = File.OpenRead(fileName);
            var header = ReadHeader(stream);
            var endian = GetEndian(header.Version);

            var cryptBlockIndex = 0ul;

            var countEntities = isEncrypted
                ? ReadDecryptUInt32(stream, decryptor, cryptBlockIndex, ref cryptBlockIndex, endian)
                : stream.ReadUInt32(endian);

            return Enumerable
                   .Range(0, (int)countEntities)
                   .Select(v => Entity.Read(stream, decryptor, ref cryptBlockIndex, endian, isEncrypted))
                   .ToArray();
        }

        internal static uint ReadDecryptUInt32(in Stream stream, in IDecryptor decryptor, in ulong cryptBlockIndex, in Endian endian)
        {
            var nextCryptBlockIndex = 0ul;
            return ReadDecryptUInt32(stream, decryptor, cryptBlockIndex, ref nextCryptBlockIndex, endian);
        }

        internal static uint ReadDecryptUInt32(
            in Stream stream,
            in IDecryptor decryptor,
            in ulong cryptBlockIndex,
            ref ulong nextCryptBlockIndex,
            in Endian endian
        )
        {
            var buffer = stream.Read(sizeof(int));
            decryptor.Decrypt(buffer, cryptBlockIndex, ref nextCryptBlockIndex);
            if (endian == Endian.Big)
            {
                Array.Reverse(buffer);
            }

            return BitConverter.ToUInt32(buffer, 0);
        }

        internal static ulong RoundUp(in ulong value, in ulong toNearest)
        {
            return (value + toNearest - 1) / toNearest;
        }

        private static Endian GetEndian(in short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }
        private bool GetIsEncrypted(FileHeader header)
        {
            return header.Version != 2 && header.Seed != 0;
        }
    }
}
