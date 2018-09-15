using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using Assimp.Unmanaged;
using NDesk.Options;
using ByteSizeLib;

namespace I3DShapesTool
{
    class Program
    {
        private static I3DShapesHeader ParseFileHeader(Stream fs)
        {
            byte b1 = fs.ReadInt8();
            byte b2 = fs.ReadInt8();
            byte b3 = fs.ReadInt8();
            byte b4 = fs.ReadInt8();

            byte seed;
            short version;

            if (b1 == 5)
            {
                //Some testing
                version = b1;
                seed = b3;
            }
            else if (b4 == 2 || b4 == 3)
            {
                version = b4;
                seed = b2;
            }
            else
            {
                throw new NotSupportedException("Unknown version");
            }

            return new I3DShapesHeader
            {
                Seed = seed,
                Version = version
            };
        }

        private static void ParseFile()
        {
            using (FileStream fs = File.OpenRead(InPath))
            {
                string fileName = Path.GetFileName(fs.Name) ?? "N/A";
                Console.WriteLine("Loading file: " + fileName);

                Console.WriteLine("File Size: " + ByteSize.FromBytes(new FileInfo(fs.Name).Length));

                I3DShapesHeader header = ParseFileHeader(fs);
                
                Console.WriteLine("File Seed: " + header.Seed);
                Console.WriteLine("File Version: " + header.Version);

                if (header.Version != 2 && header.Version != 3)
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
                
                using (I3DDecryptorStream dfs = new I3DDecryptorStream(fs, header.Seed))
                {
                    int itemCount = dfs.ReadInt32L();
                    Console.WriteLine("Found " + itemCount + " items");
                    Console.WriteLine();
                    for (int i = 0; i < itemCount; i++)
                    {
                        Console.Write("{0}: ", i + 1);

                        int type = dfs.ReadInt32L();
                        int size = dfs.ReadInt32L();
                        Console.Write("(Type {0}) ", type);
                        Console.Write(ByteSize.FromBytes(size));
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

                        if (DumpBinary)
                        {
                            string binFileName = shape.Name + ".bin";
                            File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), data);
                        }

                        if (ExportFormat != null)
                        {
                            Scene scene = shape.ToAssimp();
                            AssimpContext exporter = new AssimpContext();

                            string mdlFileName = Path.Combine(folder, CleanFileName(shape.Name + "." + ExportFormat.FileExtension));

                            ExportDataBlob dataBlob = exporter.ExportToBlob(scene, ExportFormat.FormatId, PostProcessSteps.ValidateDataStructure);

                            using (FileIOSystem ioSystem = new FileIOSystem())
                            {
                                using (IOStream iostream = ioSystem.OpenFile(mdlFileName, FileIOMode.WriteBinary))
                                {
                                    iostream.Write(dataBlob.Data, dataBlob.Data.LongLength);
                                }
                            }
                        }

                        Console.WriteLine(" - {0}", shape.Name);
                    }
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: I3DShapesTool [-dhv --supportedformats --out=outPath] [-f=format | -b] inFile");
            Console.WriteLine("Extract model data from GIANTS engine's .i3d.shapes files.");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
        }

        public static int Verbosity;
        public static string InPath;
        public static string OutPath;
        public static bool CreateDir;
        public static bool DumpBinary;
        public static ExportFormatDescription ExportFormat;

        private static void Main(string[] args)
        {
            bool showHelp = false;
            bool showSupportedFormats = false;
            string chosenFormat = null;

            AssimpContext exporter = new AssimpContext();
            ExportFormatDescription[] formats = exporter.GetSupportedExportFormats();

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
                    "d|createdir", "extract the files to a folder in the output directory instead of directly to the output directory", v => CreateDir = v != null
                },
                {
                    "out:", "the {DIRECTORY} files should be extracted to\ndefaults to the directory of the input file", v => OutPath = v
                },
                {
                    "b|bin", "dump the raw decrypted binary file",  v => DumpBinary = v != null
                },
                {
                    "f|format=", "the output {FORMAT}, check --supportedformats",  v => chosenFormat = v
                },
                {
                    "supportedformats", "prints out the supported extraction formats", v => showSupportedFormats = v != null
                },
            };

            try
            {
                List<string> extra = p.Parse(args);

                if (extra.Count == 0)
                {
                    showHelp = true;
                }
                else
                {
                    InPath = extra[0];
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

                if (chosenFormat == null && !DumpBinary)
                {
                    showHelp = true;
                }
                else
                {
                    foreach (ExportFormatDescription exportFormatDescription in formats)
                    {
                        if (!string.Equals(exportFormatDescription.FormatId, chosenFormat, StringComparison.CurrentCultureIgnoreCase)) continue;
                        ExportFormat = exportFormatDescription;
                        break;
                    }

                    if(ExportFormat == null && !DumpBinary)
                        throw new OptionException("Invalid output format. Check --supportedformats", "--f");
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'I3DShapesTool --help' for more information.");
                Console.Read();
                return;
            }
            
            if (showSupportedFormats)
            {
                Console.WriteLine("Supported Extraction Formats:");
                foreach (ExportFormatDescription exportFormatDescription in formats)
                {
                    Console.WriteLine("\t-f={0} - {1} (.{2})", exportFormatDescription.FormatId, exportFormatDescription.Description, exportFormatDescription.FileExtension);
                }
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
