using System.Collections.Generic;

namespace I3DShapesTool.Lib.Model.I3D
{
    public struct I3DMaterial
    {
        public string Name;
        public int MaterialID;
        public I3DVector4? DiffuseColor;
        public I3DVector4? EmissiveColor;
        public I3DVector? SpecularColor;
        public bool? AlphaBlending;
        public I3DFile? Texture;
        public I3DFile? Normalmap;
        public I3DFile? Glossmap;
        public I3DFile? Emissivemap;
        public I3DFile? Reflectionmap;
        public I3DFile? Refractionmap;
        public Dictionary<int, I3DVector4> ColorMats;

        public override string ToString()
        {
            return $"Material ({MaterialID}, {Name})";
        }
    }
}