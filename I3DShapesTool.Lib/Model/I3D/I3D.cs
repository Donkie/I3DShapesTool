using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string ExternalShapesFile { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

        private readonly IList<Shape> shapes = new List<Shape>();

        public void LinkShapeData(I3DShape shape)
        {
            foreach(Shape shp in shapes)
            {
                if(shp.ShapeId == shape.Id)
                {
                    shp.ShapeData = shape;
                }
            }
        }

        public void LinkShapesFile(ShapesFile shapesFile)
        {
            foreach (I3DShape shape in shapesFile.Shapes)
            {
                LinkShapeData(shape);
            }
        }

        private void MapShapesRecurse(TransformGroup parent)
        {
            if (parent is Shape shape)
            {
                shapes.Add(shape);
            }

            foreach (TransformGroup child in parent.Children)
            {
                MapShapesRecurse(child);
            }
        }

        private void MapShapes()
        {
            MapShapesRecurse(SceneRoot);
        }

        public void Setup()
        {
            MapShapes();
        }

        public IEnumerable<Shape> GetShapes()
        {
            return shapes;
        }
    }
}
