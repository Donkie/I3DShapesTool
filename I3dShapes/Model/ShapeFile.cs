using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I3dShapes.Container;
using I3dShapes.Model.Contract;
using I3dShapes.Tools;
using Microsoft.Extensions.Logging;

namespace I3dShapes.Model
{
    public class ShapeFile
    {
        /// <summary>
        /// Loaders known shapeTypes;
        /// </summary>
        private static readonly Dictionary<ShapeType, Func<BinaryReader, int, IShapeObject>> KnownTypeToLoader =
            new Dictionary<ShapeType, Func<BinaryReader, int, IShapeObject>>
            {
                {
                    ShapeType.Type1, (reader, version) => new Shape(reader, version)
                },
                {
                    ShapeType.Spline, (reader, version) => new Spline(reader)
                },
                {
                    ShapeType.NavMesh, (reader, version) => new NavMesh(reader)
                },
            };

        /// <summary>
        /// All shapeTypes that we can read, except <see cref="ShapeType.Raw"/> and <see cref="ShapeType.RawNamed"/>.
        /// <see cref="ShapeType.Raw"/> and <see cref="ShapeType.RawNamed"/> readed from <see cref="ReadRawShape"/> and <see cref="ReadRawNamedShape"/>
        /// </summary>
        public static readonly ICollection<ShapeType> AllKnownReadTypes = KnownTypeToLoader.Keys.ToArray();

        private readonly ILogger _logger;
        private ICollection<Entity> _entities;

        public ShapeFile(string fileName, ILogger logger = null)
        {
            _logger = logger;
            Container = new FileContainer(fileName, logger);
            _entities = Container.GetEntities();
        }

        public int Seed => Container.Header.Seed;

        public int Version => Container.Header.Version;
        
        public FileContainer Container { get; }

        /// <inheritdoc cref="FileContainer.FilePath"/>
        public string FilePath => Container.FilePath;

        /// <summary>
        /// Read all shapeTypes as <see cref="IRawShapeObject"/>.
        /// </summary>
        /// <param name="rawTypes">A collection of readable raw shapeTypes. If not specified, then all.</param>
        /// <returns>A collection of <see cref="IRawShapeObject"/>.</returns>
        public IEnumerable<IRawShapeObject> ReadRawShape(params uint[] rawTypes)
        {
            var workEntities = rawTypes?.Any() == true
                ? _entities
                    .Where(v => rawTypes.Contains(v.Type))
                : _entities;

            foreach (var entityRaw in Container.ReadRawData(workEntities))
            {
                using var entityStream = new MemoryStream(entityRaw.RawData);
                using var reader = new EndianBinaryReader(entityStream, Container.Endian);
                yield return new RawShapeObject(entityRaw.Entity.Type, reader);
            }
        }

        /// <summary>
        /// Read all shapeTypes as <see cref="IRawNamedShapeObject"/>.
        /// </summary>
        /// <param name="rawTypes">A collection of readable raw shapeTypes. If not specified, then all.</param>
        /// <returns>A collection of <see cref="IRawNamedShapeObject"/>.</returns>
        public IEnumerable<IRawNamedShapeObject> ReadRawNamedShape(params uint[] rawTypes)
        {
            var workEntities = rawTypes?.Any() == true
                ? _entities
                    .Where(v => rawTypes.Contains(v.Type))
                : _entities;

            foreach (var entityRaw in Container.ReadRawData(workEntities))
            {
                using var entityStream = new MemoryStream(entityRaw.RawData);
                using var reader = new EndianBinaryReader(entityStream, Container.Endian);
                yield return new RawNamedShapeObject(entityRaw.Entity.Type, reader, Container.Endian);
            }
        }

        /// <summary>
        /// Read the shapeTypes that we can read.
        /// </summary>
        /// <param name="shapeTypes">All shapeTypes that we can read, except <see cref="ShapeType.Raw"/> and <see cref="ShapeType.RawNamed"/>.</param>
        /// <returns>A enumerable of <see cref="IShapeObject"/>.</returns>
        public IEnumerable<IShapeObject> ReadKnowTypes(params ShapeType[] shapeTypes)
        {
            shapeTypes = shapeTypes?.Any() == true
                ? shapeTypes
                : AllKnownReadTypes.ToArray();

            shapeTypes = shapeTypes
                    .Where(v => AllKnownReadTypes.Contains(v))
                    .ToArray();

            var rawTypes = shapeTypes
                           .Select(v => ShapeTypeToRawType(v))
                           .Where(v => v != 0)
                           .Distinct()
                           .ToArray();

            var workEntities = _entities
                .Where(entity => rawTypes.Contains(entity.Type));

            foreach (var entityRaw in Container.ReadRawData(workEntities))
            {
                using var entityStream = new MemoryStream(entityRaw.RawData);
                using var reader = new EndianBinaryReader(entityStream, Container.Endian);
                yield return KnownTypeToLoader[RawTypeToShapeType(entityRaw.Entity.Type)].Invoke(reader, Container.Header.Version);
            }
        }

        private static ShapeType RawTypeToShapeType(in uint rawType)
        {
            switch (rawType)
            {
                case 1:
                    return ShapeType.Type1;
                case 2:
                    return ShapeType.Spline;
                case 3:
                    return ShapeType.NavMesh;
                default:
                    return ShapeType.Unknown;
            }
        }

        private static uint ShapeTypeToRawType(ShapeType shapeType)
        {
            switch (shapeType)
            {
                case ShapeType.Type1:
                    return 1;
                case ShapeType.Spline:
                    return 2;
                case ShapeType.NavMesh:
                    return 3;
                default:
                    return 0;
            }
        }
    }
}
