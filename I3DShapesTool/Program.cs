using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;
using NLog;
using NLog.Layouts;

namespace I3DShapesTool
{
    class Program
    {
        private static void ExtractFile()
        {
            var file = new I3DShapesFile();
            file.Load(InPath);

            string folder;
            if (CreateDir)
            {
                folder = Path.Combine(OutPath, "extract_" + file.FileName);
                Directory.CreateDirectory(folder);
            }
            else
            {
                folder = OutPath;
            }

            foreach (var shape in file.Shapes)
            {
                if (DumpBinary)
                {
                    var binFileName = $"shape_{shape.Name}.bin";
                    File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), shape.RawBytes);
                }

                var mdlFileName = Path.Combine(folder, CleanFileName(shape.Name + ".obj"));

                var objfile = shape.ToObj();
                objfile.Name = file.FileName.Replace(".i3d.shapes", "");
                var dataBlob = objfile.ExportToBlob();

                if (File.Exists(mdlFileName))
                    File.Delete(mdlFileName);

                File.WriteAllBytes(mdlFileName, dataBlob);
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Logger.Info("Usage: I3DShapesTool [-dhv --out=outPath] [-b] inFile");
            Logger.Info("Extract model data from GIANTS engine's .i3d.shapes files.");

            string optionDescriptions;
            using (var ms = new MemoryStream())
            {
                using (var tw = new StreamWriter(ms, Encoding.ASCII))
                {
                    p.WriteOptionDescriptions(tw);
                }
                optionDescriptions = Encoding.ASCII.GetString(ms.ToArray());
            }

            foreach (var s in optionDescriptions.Split('\n'))
            {
                Logger.Info(s);
            }
        }

        public static int Verbosity;
        public static string InPath;
        public static string OutPath;
        public static bool CreateDir;
        public static bool DumpBinary;
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new NLog.Targets.ConsoleTarget("logConsole");
            logconsole.Layout = new SimpleLayout("[${level:uppercase=true}] ${message}");

            LogLevel minLevel;
            if (Verbosity >= 2)
                minLevel = LogLevel.Trace;
            else if (Verbosity == 1)
                minLevel = LogLevel.Debug;
            else
                minLevel = LogLevel.Info;

            config.AddRule(minLevel, LogLevel.Fatal, logconsole);

            NLog.LogManager.Configuration = config;
        }

        private static void Main(string[] args)
        {
            SetupLogging();

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
                Logger.Info(e.Message);
                Logger.Info("Try 'I3DShapesTool --help' for more information.");
                return;
            }
            
            if (showHelp)
            {
                Logger.Info("This program needs to be run from a batch file or Windows command line.");
                ShowHelp(p);
                Logger.Info("Press enter to exit...");
                Console.Read();
                return;
            }

            ExtractFile();

            Logger.Info("Done");
            Logger.Info("Press enter to exit...");
            Console.Read();

            LogManager.Shutdown();
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
