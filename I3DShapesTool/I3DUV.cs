namespace I3DShapesTool
{
    class I3DUV
    {
        public float U { get; }
        public float V { get; }

        public I3DUV(BigEndianBinaryReader br)
        {
            U = br.ReadSingle();
            V = br.ReadSingle();
        }
    }
}
