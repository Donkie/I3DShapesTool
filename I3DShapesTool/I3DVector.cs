namespace I3DShapesTool
{
    class I3DVector
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public I3DVector(BigEndianBinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Z = br.ReadSingle();
        }
    }
}
