using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using ByteSizeLib;

namespace I3DShapesTool
{
    class Program
    {
        private static I3DShapesHeader ParseFileHeader(Stream fs)
        {
            byte b1 = (byte)fs.ReadByte();
            byte b2 = (byte)fs.ReadByte();
            byte b3 = (byte)fs.ReadByte();
            byte b4 = (byte)fs.ReadByte();

            byte seed;
            short version;

            if (b1 >= 4) // Might be 5 as well
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

                if (header.Version < 2 || header.Version > 5)
                    throw new NotSupportedException("Unsupported version");

                Endian fileEndian = header.Version >= 4 ? Endian.Little : Endian.Big; // Might be version 5

                Console.WriteLine();
                
                using (I3DDecryptorStream dfs = new I3DDecryptorStream(fs, header.Seed))
                {
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


                    //using (MemoryStream ms = new MemoryStream())
                    //{
                    //    var off = 0;
                    //    while (true)
                    //    {
                    //        var buf = dfs.ReadBytes(16);
                    //        if (buf.Length != 4)
                    //            break;
                    //        ms.Write(buf, 0, 16);
                    //        off += 4;
                    //    }
                    //    File.WriteAllBytes(Path.Combine(folder, "allShapes.bin"), ms.GetBuffer());
                    //}



                    int itemCount = dfs.ReadInt32(fileEndian);
                    Console.WriteLine("Found " + itemCount + " items");
                    Console.WriteLine();
                    for (int i = 0; i < itemCount; i++)
                    {
                        Console.Write("{0}: ", i + 1);

                        int type = dfs.ReadInt32(fileEndian);
                        int size = dfs.ReadInt32(fileEndian);
                        Console.Write("(Type {0}) ", type);
                        Console.Write(ByteSize.FromBytes(size));
                        byte[] data = dfs.ReadBytes(size);
                        
                        if (DumpBinary)
                        {
                            string binFileName = $"shape_{i}.bin";
                            File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), data);
                        }
                        
                        I3DShape shape;
                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            if(fileEndian == Endian.Big)
                            {
                                using (BigEndianBinaryReader br = new BigEndianBinaryReader(ms))
                                {
                                    shape = new I3DShape(br, header.Version);
                                }
                            }
                            else
                            {
                                using (BinaryReader br = new BinaryReader(ms))
                                {
                                    shape = new I3DShape(br, header.Version);
                                }
                            }
                        }

                        string mdlFileName = Path.Combine(folder, CleanFileName(shape.Name + ".obj"));

                        var objfile = shape.ToObj();
                        objfile.Name = fileName.Replace(".i3d.shapes", "");
                        var dataBlob = objfile.ExportToBlob();
                        
                        if(File.Exists(mdlFileName))
                            File.Delete(mdlFileName);

                        File.WriteAllBytes(mdlFileName, dataBlob);
                        
                        Console.WriteLine(" - {0}", shape.Name);
                    }
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: I3DShapesTool [-dhv --out=outPath] [-b] inFile");
            Console.WriteLine("Extract model data from GIANTS engine's .i3d.shapes files.");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
        }

        public static int Verbosity;
        public static string InPath;
        public static string OutPath;
        public static bool CreateDir;
        public static bool DumpBinary;

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
                    "d|createdir", "extract the files to a folder in the output directory instead of directly to the output directory", v => CreateDir = v != null
                },
                {
                    "out:", "the {DIRECTORY} files should be extracted to\ndefaults to the directory of the input file", v => OutPath = v
                },
                {
                    "b|bin", "dump the raw decrypted binary file",  v => DumpBinary = v != null
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
