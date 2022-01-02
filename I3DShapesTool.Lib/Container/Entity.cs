using System.IO;
using I3DShapesTool.Lib.Tools;
using I3DShapesTool.Lib.Tools.Extensions;

namespace I3DShapesTool.Lib.Container
{
    /// <summary>
    /// Entity.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Entity type.
        /// </summary>
        public int Type { get; }

        /// <summary>
        /// Entity type.
        /// </summary>
        public int Size { get; }

        public byte[] Data { get; }

        public Entity(int type, int size, byte[] data)
        {
            Type = type;
            Size = size;
            Data = data;
        }

        public static Entity Read(BinaryReader stream)
        {
            var type = stream.ReadInt32();
            var size = stream.ReadInt32();
            var data = stream.ReadBytes(size);

            return new Entity(type, size, data);
        }
    }
}