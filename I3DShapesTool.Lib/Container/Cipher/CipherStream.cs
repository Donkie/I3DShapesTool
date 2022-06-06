using System;
using System.IO;

namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// A stream that passes the read/written data through a cipher
    /// </summary>
    public class CipherStream : Stream
    {

        public CipherStream(Stream baseStream, ICipher cipher)
        {
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            this.cipher = cipher;
            blockOffset = 0;
        }

        public Stream BaseStream { get; }

        private readonly ICipher cipher;
        private ulong blockOffset;

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            while(i < count)
            {
                int read = BaseStream.Read(buffer, offset + i, count - i);
                if(read < 1)
                    return read;
                i += read;
            }

            blockOffset = cipher.Process(buffer, blockOffset);
            return i;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] copyBuffer = new byte[count];
            Array.Copy(buffer, offset, copyBuffer, 0, count);

            blockOffset = cipher.Process(copyBuffer, blockOffset);

            BaseStream.Write(copyBuffer, 0, count);
        }
    }
}
