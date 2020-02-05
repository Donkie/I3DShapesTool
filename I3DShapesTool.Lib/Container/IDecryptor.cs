namespace I3DShapesTool.Lib.Container
{
    public interface IDecryptor
    {
        /// <summary>
        /// Decrypt buffer by decrypt block index
        /// </summary>
        /// <param name="buf">Decrypt block</param>
        /// <param name="blockIndex">Decrypt block index by key</param>
        void DecryptBlocks(uint[] buf, ulong blockIndex);

        void Decrypt(byte[] buffer, ulong blockIndex);

        void Decrypt(byte[] buffer, ulong blockIndex, ref ulong nextBlockIndex);
    }
}
