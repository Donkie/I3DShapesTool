using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace I3DShapesTool.Lib.Container
{
    public class DecryptorStream : Stream
    {

        public DecryptorStream(Stream baseStream, IDecryptor decryptor)
        {
            BaseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            Decryptor = decryptor;
            BlockOffset = 0;
        }

        public Stream BaseStream { get; }

        private readonly IDecryptor Decryptor;
        private ulong BlockOffset;

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

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
            
            BlockOffset = Decryptor.Decrypt(buffer, BlockOffset);
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
            throw new NotSupportedException();
        }
    }
}
