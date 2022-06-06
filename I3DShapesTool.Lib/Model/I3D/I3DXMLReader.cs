using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

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
            string[]? components = vecStr.Split(' ');
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
                        if(reader.Name == "i3D" && reader.HasAttributes)
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
                            break;
                        }
                        if(reader.Name == "Shapes")
                        {
                            reader.MoveToFirstAttribute();
                            do
                            {
                                if(reader.Name == "externalShapesFile")
                                    result.ExternalShapesFile = Path.Combine(Path.GetDirectoryName(filePath), reader.Value);
                            }
                            while(reader.MoveToNextAttribute());
                            break;
                        }
                        if(reader.Name == "Scene")
                        {
                            reader.Read();
                            TraverseScene(reader, result.SceneRoot);
                        }
                        break;

                }
            }

            result.Setup();

            return result;
        }
    }
}
