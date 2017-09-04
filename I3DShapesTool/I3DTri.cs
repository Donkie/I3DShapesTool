namespace I3DShapesTool
{
    class I3DTri
    {
        public ushort P1Idx { get; }
        public ushort P2Idx { get; }
        public ushort P3Idx { get; }

        public I3DTri(BigEndianBinaryReader br)
        {
            P1Idx = br.ReadUInt16();
            P2Idx = br.ReadUInt16();
            P3Idx = br.ReadUInt16();
        }
    }
}
