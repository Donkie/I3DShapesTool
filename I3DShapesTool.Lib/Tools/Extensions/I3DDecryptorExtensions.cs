using System;
using System.IO;
using I3DShapesTool.Lib.Container;

namespace I3DShapesTool.Lib.Tools.Extensions
{
    public static class I3DDecryptorExtensions
    {
        public static Int32 ReadInt32(
            this I3DDecryptor decryptor,
            Stream stream,
            ulong cryptBlockIndex,
            ref ulong nextCryptBlockIndex,
            Endian endian
        )
        {
            var buffer = stream.FillBuffer(4);
            decryptor.Decrypt(buffer, cryptBlockIndex, ref nextCryptBlockIndex);
            if (endian == Endian.Big)
            {
                Array.Reverse(buffer);
            }

            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
