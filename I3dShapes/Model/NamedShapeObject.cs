using System.IO;
using System.Linq;
using System.Text;
using I3dShapes.Exceptions;
using I3dShapes.Model.Contract;
using I3dShapes.Tools.Extensions;

namespace I3dShapes.Model
{
    public abstract class NamedShapeObject : ShapeObject, INamedShapeObject
    {
        protected NamedShapeObject(ShapeType type)
            : base(type)
        {
        }

        /// <inheritdoc />
        public uint Id { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="UnknownFormatShapeException"></exception>
        protected void Load(BinaryReader reader, bool isAlign = true)
        {
            var nameLength = reader.ReadInt32();
            Name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));

            if (isAlign)
            {
                var align = reader.Align(4);
                if (align.Any(v => v != 0))
                {
                    throw new UnknownFormatShapeException();
                }
            }

            Id = reader.ReadUInt32();
        }
    }
}
