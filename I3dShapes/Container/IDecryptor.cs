namespace I3dShapes.Container
{
    public interface IDecryptor
    {
        /// <summary>
        /// Decrypt buffer by decrypt block index
        /// </summary>
        /// <param name="buffer">Decrypt block</param>
        /// <param name="blockIndex">Decrypt block index by key</param>
        void DecryptBlocks(uint[] buffer, ulong blockIndex);

        void Decrypt(byte[] buffer, ulong blockIndex);

        void Decrypt(byte[] buffer, ulong blockIndex, ref ulong nextBlockIndex);
    }
}
