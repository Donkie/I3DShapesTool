﻿using System.IO;

namespace I3DShapesTool.Lib.Model
{
    public class I3DUV
    {
        public float U { get; }
        public float V { get; }

        public I3DUV(BinaryReader br, int fileVersion)
        {
            if (fileVersion >= 4) // Can be 5 as well
            {
                V = br.ReadSingle();
                U = br.ReadSingle();
            }
            else
            {
                U = br.ReadSingle();
                V = br.ReadSingle();
            }
        }
    }
}
