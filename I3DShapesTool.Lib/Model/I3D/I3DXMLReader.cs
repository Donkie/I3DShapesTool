using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;

namespace I3DShapesTool.Lib.Model.I3D
{
    public class I3DXMLReader
    {
        public static ILogger? Logger;

        private static readonly XmlReaderSettings xmlSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        private static I3DVector ParseVectorString(string vecStr)
        {
            string[] components = vecStr.Split(' ');
            if(components.Length != 3)
            {
                Logger?.LogWarning("Encountered invalid vector string when parsing xml: \"{str}\"", vecStr);
                return I3DVector.Zero;
            }
            return new I3DVector(
                double.Parse(components[0], CultureInfo.InvariantCulture),
                double.Parse(components[1], CultureInfo.InvariantCulture),
                double.Parse(components[2], CultureInfo.InvariantCulture)
            );
        }

        private static I3DVector4 Parse4DVectorString(string vecStr)
        {
            string[] components = vecStr.Split(' ');
            if(components.Length != 4)
            {
                Logger?.LogWarning("Encountered invalid 4D vector string when parsing xml: \"{str}\"", vecStr);
                return I3DVector4.Zero;
            }
            return new I3DVector4(
                float.Parse(components[0], CultureInfo.InvariantCulture),
                float.Parse(components[1], CultureInfo.InvariantCulture),
                float.Parse(components[2], CultureInfo.InvariantCulture),
                float.Parse(components[3], CultureInfo.InvariantCulture)
            );
        }

        private static void TraverseScene(XmlReader reader, TransformGroup parent)
        {
            while(reader.NodeType != XmlNodeType.EndElement)
            {
                if(reader.NodeType == XmlNodeType.Element)
                {
                    I3DSceneType type;
                    try
                    {
                        type = (I3DSceneType)Enum.Parse(typeof(I3DSceneType), reader.Name);
                    }
                    catch(ArgumentException)
                    {
                        throw new ArgumentException($"Unknown element type \"{reader.Name}\" found in scene");
                    }

                    bool isEmpty = reader.IsEmptyElement;

                    string? name = null;
                    int? id = null;
                    int? shapeId = null;
                    I3DVector pos = I3DVector.Zero;
                    I3DVector rot = I3DVector.Zero;
                    I3DVector scl = I3DVector.One;
                    if(reader.MoveToFirstAttribute())
                    {
                        do
                        {
                            if(reader.Name == "name")
                                name = reader.Value;
                            else if(reader.Name == "nodeId")
                                id = int.Parse(reader.Value);
                            else if(reader.Name == "shapeId")
                                shapeId = int.Parse(reader.Value);
                            else if(reader.Name == "translation")
                                pos = ParseVectorString(reader.Value);
                            else if(reader.Name == "rotation")
                                rot = ParseVectorString(reader.Value);
                            else if(reader.Name == "scale")
                                scl = ParseVectorString(reader.Value);
                        }
                        while(reader.MoveToNextAttribute());
                        reader.Read();
                    }

                    TransformGroup child = type switch
                    {
                        I3DSceneType.Shape => new Shape(name, id, shapeId, pos, rot, scl),
                        I3DSceneType.Light => new Light(name, id, pos, rot, scl),
                        I3DSceneType.Camera => new Camera(name, id, pos, rot, scl),
                        _ => new TransformGroup(name, id, pos, rot, scl),
                    };
                    child.SetParent(parent);

                    if(!isEmpty)
                    {
                        TraverseScene(reader, child);
                    }
                }
            }
            reader.Read();
        }

        /// <summary>
        /// Parses the XML contents of an .i3d file into an I3D object.
        /// </summary>
        /// <param name="filePath">Path to the I3D file.</param>
        /// <returns></returns>
        public static I3D ParseXML(string filePath)
        {
            using StreamReader? fileStream = File.OpenText(filePath);
            using XmlReader reader = XmlReader.Create(fileStream, xmlSettings);

            I3D result = new I3D();

            while(reader.Read())
            {
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if(reader.Name == "i3D")
                        {
                            ParseHeader(reader, result);
                            break;
                        }
                        if(reader.Name == "Shapes")
                        {
                            result.ExternalShapesFile = ParseShapes(reader, Path.GetDirectoryName(filePath));
                            break;
                        }
                        if(reader.Name == "Scene")
                        {
                            ParseScene(reader, result.SceneRoot);
                            break;
                        }
                        if(reader.Name == "Files")
                        {
                            result.Files = ParseFiles(reader).ToArray();
                            break;
                        }
                        if(reader.Name == "Materials")
                        {
                            result.Materials = ParseMaterials(reader).ToArray();
                            break;
                        }
                        break;

                }
            }

            result.Setup();

            return result;
        }

        private static IList<I3DFile> ParseFiles(XmlReader reader)
        {
            List<I3DFile> files = new List<I3DFile>();

            reader.Read();
            while(reader.NodeType == XmlNodeType.Element && reader.Name == "File")
            {
                I3DFile file = new I3DFile();

                while(reader.MoveToNextAttribute())
                {
                    if(reader.Name == "fileId")
                        file.FileId = int.Parse(reader.Value);
                    else if(reader.Name == "filename")
                        file.Filename = reader.Value;
                    else
                        throw new InvalidOperationException($"Unknown file attribute {reader.Name}");
                }

                files.Add(file);

                reader.Read();
            }

            return files;
        }

        private static IList<I3DMaterial> ParseMaterials(XmlReader reader)
        {
            List<I3DMaterial> mats = new List<I3DMaterial>();

            reader.Read();
            while(reader.NodeType == XmlNodeType.Element && reader.Name == "Material")
            {
                I3DMaterial mat = new I3DMaterial();

                while(reader.MoveToNextAttribute())
                {
                    if(reader.Name == "name")
                        mat.Name = reader.Value;
                    else if(reader.Name == "materialId")
                        mat.MaterialID = int.Parse(reader.Value);
                    else if(reader.Name == "diffuseColor")
                        mat.DiffuseColor = Parse4DVectorString(reader.Value);
                    else if(reader.Name == "emissiveColor")
                        mat.EmissiveColor = Parse4DVectorString(reader.Value);
                    else if(reader.Name == "specularColor")
                        mat.SpecularColor = ParseVectorString(reader.Value);
                    else if(reader.Name == "alphaBlending")
                        mat.AlphaBlending = reader.Value == "true" || reader.Value == "1";
                }

                reader.Read();

                Dictionary<int, I3DVector4> colorMats = new Dictionary<int, I3DVector4>(4);

                while(reader.NodeType == XmlNodeType.Element)
                {
                    if(reader.Name == "CustomParameter")
                    {
                        string paramName = "";
                        string paramValue = "";
                        while(reader.MoveToNextAttribute())
                        {
                            if(reader.Name == "name")
                                paramName = reader.Value;
                            else if(reader.Name == "value")
                                paramValue = reader.Value;
                        }
                        if(paramName.StartsWith("colorMat"))
                        {
                            if(int.TryParse(paramName[8..], out int matId))
                            {
                                colorMats.Add(matId, Parse4DVectorString(paramValue));
                            }
                        }
                    }
                    else if(reader.Name == "Texture")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Texture = new I3DFile(fileId);
                        }
                    }
                    else if(reader.Name == "Normalmap")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Normalmap = new I3DFile(fileId);
                        }
                    }
                    else if(reader.Name == "Glossmap")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Glossmap = new I3DFile(fileId);
                        }
                    }
                    else if(reader.Name == "Emissivemap")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Emissivemap = new I3DFile(fileId);
                        }
                    }
                    else if(reader.Name == "Reflectionmap")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Reflectionmap = new I3DFile(fileId);
                        }
                    }
                    else if(reader.Name == "Refractionmap")
                    {
                        reader.MoveToFirstAttribute();
                        if(int.TryParse(reader.Value, out int fileId))
                        {
                            mat.Refractionmap = new I3DFile(fileId);
                        }
                    }
                    reader.Read();
                }

                mat.ColorMats = colorMats;

                mats.Add(mat);

                reader.Read();
            }

            return mats;
        }

        private static void ParseScene(XmlReader reader, TransformGroup sceneRoot)
        {
            reader.Read();
            TraverseScene(reader, sceneRoot);
        }

        private static string? ParseShapes(XmlReader reader, string rootDir)
        {
            string? externalShapesFile = null;

            reader.MoveToFirstAttribute();
            do
            {
                if(reader.Name == "externalShapesFile")
                    externalShapesFile = Path.Combine(rootDir, reader.Value);
            }
            while(reader.MoveToNextAttribute());

            return externalShapesFile;
        }

        private static void ParseHeader(XmlReader reader, I3D result)
        {
            reader.MoveToFirstAttribute();
            do
            {
                if(reader.Name == "name")
                    result.Name = reader.Value;
                if(reader.Name == "version")
                    result.Version = reader.Value;
            }
            while(reader.MoveToNextAttribute());
        }
    }
}
