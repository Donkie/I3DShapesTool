using System.Collections.Generic;
using System.Linq;

namespace I3DShapesTool.Lib.Model.I3D
{
    /// <summary>
    /// Represents an entire .i3d file, containing a scene of objects and related metadata
    /// </summary>
    public class I3D
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? ExternalShapesFile { get; set; }
        public TransformGroup SceneRoot { get; } = new TransformGroup("root", 0, I3DVector.Zero, I3DVector.Zero, I3DVector.One);

        public I3DFile[] Files { get; set; }

        public I3DMaterial[] Materials { get; set; }

        private readonly IList<Shape> shapes = new List<Shape>();

        public void LinkShapeData(I3DShape shape)
        {
            foreach(Shape shp in shapes)
            {
                if(shp.ShapeId == shape.Id)
                {
                    shp.ShapeData = shape;
                }
            }
        }

        public void LinkShapesFile(ShapesFile shapesFile)
        {
            foreach (I3DShape? shape in shapesFile.Shapes)
            {
                LinkShapeData(shape);
            }
        }

        private void MapShapesRecurse(TransformGroup parent)
        {
            if (parent is Shape shape)
            {
                shapes.Add(shape);
            }

            foreach (TransformGroup child in parent.Children)
            {
                MapShapesRecurse(child);
            }
        }

        private void MapShapes()
        {
            MapShapesRecurse(SceneRoot);
        }

        public I3DFile GetFileById(int id)
        {
            return Files.Where(f => f.FileId == id).Single();
        }

        private void ResolveFiles()
        {
            for(int i = 0; i < Materials.Length; i++)
            {
                I3DMaterial mat = Materials[i];
                if(mat.Texture != null)
                    mat.Texture = GetFileById(((I3DFile)mat.Texture).FileId);
                if(mat.Normalmap != null)
                    mat.Normalmap = GetFileById(((I3DFile)mat.Normalmap).FileId);
                if(mat.Glossmap != null)
                    mat.Glossmap = GetFileById(((I3DFile)mat.Glossmap).FileId);
                if(mat.Emissivemap != null)
                    mat.Emissivemap = GetFileById(((I3DFile)mat.Emissivemap).FileId);
                if(mat.Reflectionmap != null)
                    mat.Reflectionmap = GetFileById(((I3DFile)mat.Reflectionmap).FileId);
                if(mat.Refractionmap != null)
                    mat.Refractionmap = GetFileById(((I3DFile)mat.Refractionmap).FileId);
                Materials[i] = mat;
            }
        }

        public void Setup()
        {
            MapShapes();
            ResolveFiles();
        }

        public IEnumerable<Shape> GetShapes()
        {
            return shapes;
        }
    }
}
