using System;
using System.Collections.Generic;
using System.IO;
using I3DShapesTool.Lib.Tools;

namespace I3DShapesTool.Lib.Container
{
    public class ShapesFileWriter : IDisposable
    {
        private readonly CipherStream cipherStream;
        private readonly BinaryWriter binaryWriter;

        public ShapesFileWriter(Stream outputStream, byte seed, short version)
        {
            var header = new FileHeader(version, seed);
            header.Write(outputStream);

            Endian = GetEndian(version);

            cipherStream = new CipherStream(outputStream, new I3DCipherEncryptor(seed));
            binaryWriter = new EndianBinaryWriter(cipherStream, Endian);
        }

        public Endian Endian { get; private set; }

        public void SaveEntities(ICollection<Entity> entities)
        {
            binaryWriter.Write(entities.Count);
            foreach (var entity in entities)
            {
                entity.Write(binaryWriter);
            }
        }

        private static Endian GetEndian(short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }

        public void Dispose()
        {
            cipherStream.Dispose();
            binaryWriter.Dispose();
        }
    }
}
