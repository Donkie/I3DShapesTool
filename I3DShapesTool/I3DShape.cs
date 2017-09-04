using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I3DShapesTool
{
    class I3DShape
    {
        private uint _a;
        private string _name;
        private ushort _shapeId;
        private float _f1;
        private float _f2;
        private float _f3;
        private float _f4;
        private uint _vertexCount;
        private uint _b;
        private uint _vertices;
        private uint _c;
        private uint _d;
        private uint _uvCount;
        private uint _e;
        private uint _vertexCount2;

        private I3DTri[] _triangles;
        private I3DVector[] _positions;
        private I3DVector[] _normals;
        private I3DUV[] _uvs;
    }
}
