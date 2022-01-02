using System;
using System.Collections.Generic;
using System.IO;
using I3DShapesTool.Lib.Tools;

namespace I3DShapesTool.Lib.Container
{
    public class ShapesFileWriter : IDisposable
    {
        private readonly CipherStream stream;
        private readonly BinaryWriter writer;

        public ShapesFileWriter(string filePath, byte seed, short version)
        {
            FilePath = filePath;

            var fileStream = File.OpenRead(FilePath);

            var header = new FileHeader(version, seed);
            header.Write(fileStream);

            Endian = GetEndian(version);

            stream = new CipherStream(fileStream, new I3DCipherEncryptor(seed));
            writer = new EndianBinaryWriter(stream, Endian);
        }

        public string FilePath { get; }

        public Endian Endian { get; private set; }
        
        public void SaveEntities(ICollection<Entity> entities)
        {
            writer.Write(entities.Count);
            foreach (var entity in entities)
            {
                entity.Write(writer);
            }
        }

        private static Endian GetEndian(short version)
        {
            return version >= 4 ? Endian.Little : Endian.Big;
        }

        public void Dispose()
        {
            stream.Dispose();
            writer.Dispose();
        }
    }
}
