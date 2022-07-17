namespace I3DShapesTool.Lib.Model.I3D
{
    public struct I3DFile
    {
        public int FileId;
        public string? Filename;

        public I3DFile(int fileId)
        {
            FileId = fileId;
            Filename = null;
        }

        public I3DFile(int fileId, string fileName)
        {
            FileId = fileId;
            Filename = fileName;
        }

        public override string ToString()
        {
            return $"File ({FileId}, {Filename})";
        }
    }
}