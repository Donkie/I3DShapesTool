﻿namespace I3DShapesTool.Lib.Model.I3D
{
    public class Camera : TransformGroup
    {
        public Camera(string name, int? id, I3DVector translation, I3DVector rotation, I3DVector scale) : base(name, id, translation, rotation, scale)
        {
        }
    }
}
