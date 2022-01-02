namespace I3DShapesTool.Lib.Container
{
    public interface ICipher
    {
        /// <summary>
        /// Process an array of data through the cipher
        /// </summary>
        /// <param name="buffer">Data to process</param>
        /// <param name="blockIndex">Block index of the data</param>
        /// <returns>New block index</returns>
        ulong Process(byte[] buffer, ulong blockIndex);
    }
}
