using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I3DShapesTool
{
    class Program
    {
        private static void Main(string[] args)
        {

            string path = args.Length > 0 ? args[0] : @"D:\SteamLibrary\steamapps\common\Farming Simulator 2013\data\vehicles\balers\kroneBigPack1290.i3d.shapes";

            using (FileStream fs = File.OpenRead(path))
            {
                Console.WriteLine("Loading file: " + Path.GetFileName(fs.Name));

                byte seed = (byte)fs.ReadInt16L();
                Console.WriteLine("File Seed: " + seed);

                short version = fs.ReadInt16L();
                Console.WriteLine("File Version: " + version);

                if (version != 2)
                    throw new NotSupportedException("Unsupported version");

                Console.WriteLine();
                
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
                        File.WriteAllBytes(@"C:\Users\Daniel\Desktop\data\" + i, data);
                        Console.WriteLine();
                    }
                }
            }

            Console.Read();
        }
    }
}
