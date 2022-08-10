namespace I3DShapesTool.Lib.Model.I3D
{
    public class Shape : TransformGroup
    {
        public int? ShapeId { get; }
        public I3DShape ShapeData { get; set; }

        public Shape(string name, int? id, int? shapeId, I3DVector translation, I3DVector rotation, I3DVector scale) : base(name, id, translation, rotation, scale)
        {
            ShapeId = shapeId;
        }
    }
}
