using System;
using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

        private IDictionary<int, Shape>? _shapesMap;

        private void MapShapesRecurse(TransformGroup parent)
        {
            if (_shapesMap == null)
                throw new InvalidOperationException();

            if (parent is Shape shape && shape.ShapeId != null)
            {
                _shapesMap.Add((int)shape.ShapeId, shape);
            }

            foreach (var child in parent.Children)
            {
                MapShapesRecurse(child);
            }
        }

        private void MapShapes()
        {
            _shapesMap = new Dictionary<int, Shape>();
            MapShapesRecurse(SceneRoot);
        }

        public void Setup()
        {
            MapShapes();
        }

        public Shape? GetShape(int id)
        {
            if (_shapesMap == null || !_shapesMap.ContainsKey(id))
                return null;
            return _shapesMap[id];
        }
    }
}
