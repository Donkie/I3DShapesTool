using I3dShapes.Model.Contract;

namespace I3dShapes.Model
{
    /// <summary>
    /// Base shape object.
    /// </summary>
    public abstract class ShapeObject : IShapeObject
    {
        protected ShapeObject(ShapeType type)
        {
            Type = type;
        }

        /// <summary>
        /// Shape type
        /// </summary>
        public ShapeType Type { get; }
    }
}
