using System;

namespace I3dShapes.Container
{
    internal partial class Decryptor : IDecryptor
    {
        public const int CryptBlockSize = 64;

        public void DecryptBlocks(uint[] buffer, ulong blockIndex)
        {
            var key = GetKeyByIndexBlock(_key, blockIndex);
            DecryptBlocks(key, buffer);
        }

        public void Decrypt(byte[] buffer, ulong blockIndex)
        {
            var nextBlockIndex = 0ul;
            Decrypt(buffer, blockIndex, ref nextBlockIndex);
        }

        /// <summary>
        /// Decrypt block
        /// </summary>
        /// <param name="buffer">Decrypt block</param>
        /// <param name="blockIndex">Decrypt block index</param>
        /// <param name="nextBlockIndex">Next decrypt block index</param>
        public void Decrypt(byte[] buffer, ulong blockIndex, ref ulong nextBlockIndex)
        {
            var copy = new byte[RoundUpTo(buffer.Length, CryptBlockSize)];
            buffer.CopyTo(copy, 0);

            var blocks = new uint[copy.Length / 4];
            CopyTo(copy, 0, blocks);

            DecryptBlocks(blocks, blockIndex);

            CopyTo(blocks, 0, copy);
            Array.Copy(copy, buffer, buffer.Length);
            nextBlockIndex = blockIndex + (ulong)(RoundUpTo(buffer.Length, CryptBlockSize) / CryptBlockSize);
        }

        /// <summary>
        /// Get key by index decrypt block
        /// </summary>
        /// <param name="key"></param>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        private static uint[] GetKeyByIndexBlock(uint[] key, ulong blockIndex)
        {
            var tempKey = new uint[key.Length];
            Array.Copy(key, tempKey, tempKey.Length);

            tempKey[8] = (uint)(blockIndex & 0xFFFFFFFF);
            tempKey[9] = (uint)(blockIndex >> 32);
            return tempKey;
        }

        private static void DecryptBlocks(uint[] key, uint[] buf)
        {
            if (buf.Length % 16 != 0)
            {
                throw new Exception("Expecting 16 byte blocks");
            }

            var tempKey = new uint[key.Length];
            var blockCounter = key[8] | ((ulong)key[9] << 32);
            for (var i = 0; i < buf.Length; i += 16)
            {
                key.CopyTo(tempKey, 0);

                for (var j = 0; j < 10; j++)
                {
                    Shuffle1(tempKey, 0x0, 0xC, 0x4, 0x8);
                    Shuffle1(tempKey, 0x5, 0x1, 0x9, 0xD);
                    Shuffle1(tempKey, 0xA, 0x6, 0xE, 0x2);
                    Shuffle1(tempKey, 0xF, 0xB, 0x3, 0x7);
                    Shuffle2(tempKey, 0x3, 0x0, 0x1, 0x2);
                    Shuffle2(tempKey, 0x4, 0x5, 0x6, 0x7);
                    Shuffle1(tempKey, 0xA, 0x9, 0xB, 0x8);
                    Shuffle2(tempKey, 0xE, 0xF, 0xC, 0xD);
                }

                for (var j = 0; j < key.Length; j++)
                    buf[i + j] ^= key[j] + tempKey[j];

                blockCounter++;
                key[8] = (uint)(blockCounter & 0xFFFFFFFF);
                key[9] = (uint)(blockCounter >> 32);
            }
        }
    }
}
