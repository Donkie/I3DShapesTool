using System;
using System.IO;

namespace I3DShapesTool
{
    /// <summary>
    /// Created by "high" https://facepunch.com/member.php?u=60704
    /// </summary>
    class I3DDecryptorStream : Stream
    {
        private readonly I3DDecryptor _decryptor;
        private readonly Stream _input;

        public I3DDecryptorStream(Stream input, int seed)
        {
            _decryptor = new I3DDecryptor();
            _decryptor.Init((byte) seed);
            _input = input;
        }

        public override bool CanRead => _input.CanRead;

        public override bool CanSeek => _input.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _input.Length;

        public override long Position
        {
            get => _input.Position;
            set => _input.Position = value;
        }

        public override void Flush()
        {
            _input.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _input.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _input.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var i = 0;
            while (i < count)
            {
                var read = _input.Read(buffer, offset + i, count - i);
                if (read < 1)
                    return read;
                i += read;
            }

            _decryptor.Decrypt(buffer, offset, count);
            return i;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}