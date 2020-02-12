namespace I3dShapes.Model.Contract
{
    public interface IRawShapeObject : IShapeObject
    {
        uint RawType { get; }

        byte[] RawData { get; }
    }
}
