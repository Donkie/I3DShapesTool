using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3D
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public IList<TransformGroup> Scene { get; } = new List<TransformGroup>();
    }
}
