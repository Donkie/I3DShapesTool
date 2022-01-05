using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        private static ILogger? Logger { get; set; }

        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? ExternalShapesFile { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

        private IDictionary<uint, Shape>? shapesMap;

        public void LinkShapeData(I3DShape shape)
        {
            if(shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            if(!shapesMap.ContainsKey(shape.Id))
                throw new ArgumentException($"No shape with ID \"{shape.Id}\" in scene.");

            shapesMap[shape.Id].ShapeData = shape;
        }

        public void LinkShapesFile(ShapesFile shapesFile)
        {
            foreach(I3DShape? shape in shapesFile.Shapes)
            {
                try
                {
                    LinkShapeData(shape);
                }
                catch(ArgumentException ex)
                {
                    Logger.LogWarning(ex.Message);
                }
            }
        }

        private void MapShapesRecurse(TransformGroup parent)
        {
            if(shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            if(parent is Shape shape && shape.ShapeId != null)
            {
                shapesMap.Add((uint)shape.ShapeId, shape);
            }

            foreach(TransformGroup? child in parent.Children)
            {
                MapShapesRecurse(child);
            }
        }

        private void MapShapes()
        {
            shapesMap = new Dictionary<uint, Shape>();
            MapShapesRecurse(SceneRoot);
        }

        public void Setup()
        {
            MapShapes();
        }

        public IEnumerable<Shape> GetShapes()
        {
            if(shapesMap == null)
                throw new InvalidOperationException("Shape map not generated yet.");

            return shapesMap.Values;
        }

        public Shape? GetShape(uint id)
        {
            if(shapesMap == null || !shapesMap.ContainsKey(id))
                return null;
            return shapesMap[id];
        }
    }
}
