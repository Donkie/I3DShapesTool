using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3DShapesTool.Lib.Tools;
using Microsoft.Extensions.Logging;

namespace I3DShapesTool.Lib.Container
{
    public class ShapesFileReader : IDisposable
    {
        private readonly CipherStream cipherStream;
        private readonly BinaryReader binaryReader;

        public ShapesFileReader(Stream inputStream, ILogger? logger = null, byte? forceSeed = null)
        {
            Header = FileHeader.Read(inputStream);
            logger?.LogDebug("File seed: {fileSeed}", Header.Seed);
            logger?.LogDebug("File version: {version}", Header.Version);

            if(Header.Version < 2 || Header.Version > 7)
            {
                logger?.LogCritical("Unsupported version: {version}", Header.Version);
                throw new NotSupportedException("Unsupported version");
            }

            Endian = GetEndian(Header.Version);

            if(forceSeed != null)
            {
                Header = new FileHeader(Header.Version, (byte)forceSeed);
            }

            cipherStream = new CipherStream(inputStream, new I3DCipher(Header.Seed));
            binaryReader = new EndianBinaryReader(cipherStream, Endian);
        }

        public FileHeader Header { get; private set; }

        public Endian Endian { get; private set; }

        public ICollection<Entity> GetEntities()
        {
            try
            {
                int countEntities = binaryReader.ReadInt32();
                if(countEntities < 0 || countEntities > 1e6) // I don't think any i3d file would contain more than a million shapes..
                    throw new DecryptFailureException();

                return Enumerable.Range(0, countEntities)
                    .Select(v => Entity.Read(binaryReader))
                    .ToArray();
            }
            catch(Exception e)
            {
                if(e is ArgumentOutOfRangeException || e is IOException || e is OutOfMemoryException)
                    throw new DecryptFailureException();
                throw;
            }
        }

        private static Endian GetEndian(short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }

        public void Dispose()
        {
            cipherStream.Dispose();
            binaryReader.Dispose();
        }
    }
}
