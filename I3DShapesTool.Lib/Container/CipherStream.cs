using System;
using System.IO;

namespace I3DShapesTool.Lib.Container
{
    public class CipherStream : Stream
    {

        public CipherStream(Stream baseStream, ICipher cipher)
        {
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            Cipher = cipher;
            BlockOffset = 0;
        }

        public Stream BaseStream { get; }

        private readonly ICipher Cipher;
        private ulong BlockOffset;

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
            var i = 0;
            while (i < count)
            {
                var read = BaseStream.Read(buffer, offset + i, count - i);
                if (read < 1)
                    return read;
                i += read;
            }
            
            BlockOffset = Cipher.Process(buffer, BlockOffset);
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

            BlockOffset = Cipher.Process(copyBuffer, BlockOffset);

            BaseStream.Write(copyBuffer, 0, count);
        }
    }
}
