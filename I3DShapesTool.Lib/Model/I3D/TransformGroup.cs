using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class TransformGroup
    {
        public string? Name { get; protected set; }
        public int? Id { get; protected set; }
        public I3DVector Translation { get; protected set; } = I3DVector.Zero;
        public I3DVector Rotation { get; protected set; } = I3DVector.Zero;
        public I3DVector Scale { get; protected set; } = I3DVector.One;
        public TransformGroup? Parent { get; protected set; }
        public IList<TransformGroup> Children { get; protected set; } = new List<TransformGroup>();
    }
}
