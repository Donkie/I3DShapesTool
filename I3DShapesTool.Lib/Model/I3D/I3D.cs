using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);
    }
}
