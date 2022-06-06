using I3DShapesTool.Lib.Tools;
using System.IO;

namespace I3DShapesTool.Lib.Export
{
    public interface IExporter
    {
        void Export(Stream stream);
        void Transform(Transform t);
    }
}