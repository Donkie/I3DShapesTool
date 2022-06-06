namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// A Cipher acting on bytes of data, read/written in blocks
    /// </summary>
    public interface ICipher
    {
        /// <summary>
        /// Process an array of data through the cipher. The cipher will modify the data in the buffer.
        /// </summary>
        /// <param name="buffer">Data to process</param>
        /// <param name="blockIndex">Block index of the data</param>
        /// <returns>New block index</returns>
        ulong Process(byte[] buffer, ulong blockIndex);
    }
}
