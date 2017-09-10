using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using Assimp.Unmanaged;
using NDesk.Options;

namespace I3DShapesTool
{
    class Program
    {
        private static void ParseFile()
        {
            using (FileStream fs = File.OpenRead(InPath))
            {
                string fileName = Path.GetFileName(fs.Name) ?? "N/A";
                Console.WriteLine("Loading file: " + fileName);

                byte seed = (byte)fs.ReadInt16L();
                Console.WriteLine("File Seed: " + seed);

                short version = fs.ReadInt16L();
                Console.WriteLine("File Version: " + version);

                if (version != 2)
                    throw new NotSupportedException("Unsupported version");

                Console.WriteLine();

                if (Verbosity > 0)
                {
                    LogStream logstream = new LogStream(delegate(string msg, string userData)
                    {
                        Console.WriteLine(msg);
                    });
                    logstream.Attach();
                }

                using (I3DDecryptorStream dfs = new I3DDecryptorStream(fs, seed))
                {
                    int itemCount = dfs.ReadInt32L();
                    Console.WriteLine("Found " + itemCount + " items");
                    Console.WriteLine();
                    for (int i = 0; i < itemCount; i++)
                    {
                        Console.WriteLine("Loading item " + (i + 1));

                        int type = dfs.ReadInt32L();
                        Console.WriteLine("Type: " + type);
                        int size = dfs.ReadInt32L();
                        Console.WriteLine("Size: " + size);
                        byte[] data = dfs.ReadBytes(size);

                        I3DShape shape;
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            using (BigEndianBinaryReader br = new BigEndianBinaryReader(ms))
                            {
                                shape = new I3DShape(br);
                            }
                        }


                        string folder;
                        if (CreateDir)
                        {
                            folder = Path.Combine(OutPath, "extract_" + fileName);
                            Directory.CreateDirectory(folder);
                        }
                        else
                        {
                            folder = OutPath;
                        }

                        string binFileName = shape.Name + ".bin";

                        File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), data);

                        Scene scene = shape.ToAssimp();
                        AssimpContext exporter = new AssimpContext();

                        string mdlFileName = Path.Combine(folder, CleanFileName(shape.Name + ".stl"));
                        
                        ExportDataBlob dataBlob = exporter.ExportToBlob(scene, "stl", PostProcessSteps.ValidateDataStructure);
                        
                        using (FileIOSystem ioSystem = new FileIOSystem())
                        {
                            using (IOStream iostream = ioSystem.OpenFile(mdlFileName, FileIOMode.WriteBinary))
                            {
                                iostream.Write(dataBlob.Data, dataBlob.Data.LongLength);
                            }
                        }
                        
                        Console.WriteLine("Finished with " + shape.Name);
                        Console.WriteLine();
                    }
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: I3DShapesTool");
            Console.WriteLine("Extract model data from GIANTS engine's .i3d.shapes files.");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
        }

        public static int Verbosity = 0;
        public static string InPath;
        public static string OutPath;
        public static bool CreateDir = false;

        private static void Main(string[] args)
        {
            bool showHelp = false;

            OptionSet p = new OptionSet
            {
                {
                    "h|help", "show this message and exit", v => showHelp = v != null
                },
                {
                    "v|verbose", "increase debug message verbosity", v =>
                    {
                        if (v != null) Verbosity++;
                    }
                },
                {
                    "file=", "the .i3d.shapes {FILE} to be extracted", v =>
                    {
                        InPath = v;
                    }
                },
                {
                    "d|createdir", "extract the files to a folder in the output directory instead of directly to the output directory", v => CreateDir = v != null
                },
                {
                    "out:", "the {DIRECTORY} files should be extracted to\ndefaults to the directory of the input file", v =>
                    {
                        OutPath = v;
                    }
                },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);

                if (InPath == null)
                {
                    showHelp = true;
                }
                else
                {
                    if (!File.Exists(InPath))
                        throw new OptionException("File doesn't exist.", "--file");
                }

                if (OutPath == null)
                {
                    OutPath = Path.GetDirectoryName(InPath);
                }
                else
                {
                    if (!Directory.Exists(OutPath))
                        throw new OptionException("Directory doesn't exist.", "--out");
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'I3DShapesTool --help' for more information.");
                Console.Read();
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                Console.Read();
                return;
            }

            ParseFile();

            Console.WriteLine("Done");
            Console.Read();
        }

        /// <summary>
        /// https://stackoverflow.com/a/7393722/2911165
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
