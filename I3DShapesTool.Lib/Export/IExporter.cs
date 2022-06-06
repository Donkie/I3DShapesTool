using I3DShapesTool.Lib.Tools;
using System.IO;

namespace I3DShapesTool.Lib.Export
{
    public interface IExporter
    {
        byte[] ExportToBlob();
        void Export(Stream stream);
        void Transform(Transform t);
    }
}