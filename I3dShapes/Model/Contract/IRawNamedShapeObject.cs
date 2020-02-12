namespace I3dShapes.Model.Contract
{
    public interface IRawNamedShapeObject : IRawShapeObject, INamedShapeObject
    {
        /// <summary>
        /// Position skip Id and Name (Align 4).
        /// </summary>
        long ContentPosition { get; }
    }
}
