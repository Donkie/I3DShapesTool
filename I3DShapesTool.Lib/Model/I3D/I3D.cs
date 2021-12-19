using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        public static ILogger? logger { get; set; }

        public string? Name { get; set; }
        public string? Version { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

        private IDictionary<uint, Shape>? _shapesMap;

        public void LinkShapeData(I3DShape shape)
        {
            if (_shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            if (!_shapesMap.ContainsKey(shape.Id))
                throw new ArgumentException($"No shape with ID \"{shape.Id}\" in scene.");

            _shapesMap[shape.Id].ShapeData = shape;
        }

        public void LinkShapesFile(ShapesFile shapesFile)
        {
            foreach (var shape in shapesFile.Shapes)
            {
                try
                {
                    LinkShapeData(shape);
                }
                catch(ArgumentException ex)
                {
                    logger.LogWarning(ex.Message);
                }
            }
        }

        private void MapShapesRecurse(TransformGroup parent)
        {
            if (_shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            if (parent is Shape shape && shape.ShapeId != null)
            {
                _shapesMap.Add((uint)shape.ShapeId, shape);
            }

            foreach (var child in parent.Children)
            {
                MapShapesRecurse(child);
            }
        }

        private void MapShapes()
        {
            _shapesMap = new Dictionary<uint, Shape>();
            MapShapesRecurse(SceneRoot);
        }

        public void Setup()
        {
            MapShapes();
        }

        public IEnumerable<Shape> GetShapes()
        {
            if (_shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            return _shapesMap.Values;
        }

        public Shape? GetShape(uint id)
        {
            if (_shapesMap == null || !_shapesMap.ContainsKey(id))
                return null;
            return _shapesMap[id];
        }
    }
}
