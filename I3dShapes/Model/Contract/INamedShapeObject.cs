namespace I3dShapes.Model.Contract
{
    public interface INamedShapeObject : IShapeObject
    {
        /// <summary>
        /// Shape Id
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Shame name
        /// </summary>
        string Name { get; }
    }
}
