using System;
using System.IO;
using System.Runtime.InteropServices;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// Entity.
    /// </summary>
    public class Entity : IDisposable
    {
        /// <summary>
        /// Entity type.
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// Entity type.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Index Decrypt Block.
        /// </summary>
        public ulong DecryptIndexBlock { get; private set; }

        /// <summary>
        /// Offset by start entity.
        /// </summary>
        public long OffsetRawBlock { get; private set; }

        /// <summary>
        /// Read meta information <see cref="Entity"/>
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="decryptor">Decryptor.</param>
        /// <param name="decryptIndexBlock">Index Decrypt Block.</param>
        /// <param name="endian">File endian.</param>
        /// <param name="version">File version.</param>
        /// <returns></returns>
        public static Entity Read(Stream stream, IDecryptor decryptor, ref ulong decryptIndexBlock, Endian endian)
        {
            var cryptBlockCount = 0ul;
            var nextDecrIndex = 0ul;

            var type = decryptor.ReadInt32(stream, decryptIndexBlock + cryptBlockCount, ref nextDecrIndex, endian);

            var blockSize = (ulong)Marshal.SizeOf(type);
            cryptBlockCount += (blockSize + Decryptor.CryptBlockSize - 1) / Decryptor.CryptBlockSize;

            var size = decryptor.ReadInt32(stream, decryptIndexBlock + cryptBlockCount, ref nextDecrIndex, endian);
            blockSize = (ulong)Marshal.SizeOf(size);
            cryptBlockCount += (blockSize + Decryptor.CryptBlockSize - 1) / Decryptor.CryptBlockSize;
            var startDecryptIndexBlock = decryptIndexBlock + cryptBlockCount;

            var offset = stream.Position;

            cryptBlockCount += (ulong)((size + Decryptor.CryptBlockSize - 1) / Decryptor.CryptBlockSize);
            stream.Seek(size, SeekOrigin.Current);

            decryptIndexBlock += cryptBlockCount;
            return new Entity
            {
                Type = type,
                Size = size,
                OffsetRawBlock = offset,
                DecryptIndexBlock = startDecryptIndexBlock,
            };
        }

        public void Dispose()
        {
        }
    }
}