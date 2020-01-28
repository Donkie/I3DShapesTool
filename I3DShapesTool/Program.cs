using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using NLog;
using NLog.Layouts;

namespace I3DShapesTool
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('q', "quiet", Required = false, HelpText = "Suppress normal messages.")]
            public bool Quiet { get; set; }

            [Option('d', "createdir", Required = false, HelpText = "Extract the files to a folder in the output directory instead of directly to the output directory.")]
            public bool CreateDir { get; set; }

            [Option('b', "binary", Required = false, HelpText = "Dump the raw binary files as well as the model files.")]
            public bool DumpBinary { get; set; }

            [Option("out", Required = false, HelpText = "The directory files should be extracted to, defaults to the directory of the input file.")]
            public string Out { get; set; }

            [Value(0, MetaName = "input file", Required = true, HelpText = "The .i3d.shapes file to be processed")]
            public string File { get; set; }

            [Usage(ApplicationAlias = "I3DShapesTool")]
            // ReSharper disable once UnusedMember.Global
            public static IEnumerable<Example> Examples =>
                new List<Example>
                {
                    new Example("Basic usage (drag-drop a .i3d.shapes onto this application)", new Options { File = "k105.i3d.shapes" }),
                    new Example("Show more messages", new Options { File = "k105.i3d.shapes", Verbose = true }),
                    new Example("Specific output folder", new Options { File = "k105.i3d.shapes", Out = @"C:\Users\Me\Desktop\I3D Extract"})
                };
        }

        private static void ExtractFile()
        {
            var file = new I3DShapesFile();
            file.Load(Opts.File);

            string folder;
            if (Opts.CreateDir)
            {
                folder = Path.Combine(Opts.Out, "extract_" + file.FileName);
                Directory.CreateDirectory(folder);
            }
            else
            {
                folder = Opts.Out;
            }

            foreach (var shape in file.Shapes)
            {
                if (Opts.DumpBinary)
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

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Options Opts;

        private static void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new NLog.Targets.ConsoleTarget("logConsole")
            {
                Layout = new SimpleLayout("[${level:uppercase=true}] ${message}")
            };

            var minLevel = LogLevel.Info;
            if (Opts != null)
            {
                minLevel = Opts.Verbose ? LogLevel.Debug : LogLevel.Info;
                if (Opts.Quiet)
                    minLevel = LogLevel.Error;
            }

            config.AddRule(minLevel, LogLevel.Fatal, logconsole);

            LogManager.Configuration = config;
        }

        private static void Main(string[] args)
        {
            SetupLogging();

            try
            {
                var result = Parser.Default.ParseArguments<Options>(args);
                result
                    .WithParsed(Run)
                    .WithNotParsed(errs => DisplayHelp(result, errs));
            }
            catch (ArgumentValidationException e)
            {
                Logger.Error(e.Message);
                Console.Read();
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void Run(Options opt)
        {
            Opts = opt;
            SetupLogging(); // Set it up again now that we have verbosity information
            
            if (!File.Exists(opt.File))
                throw new ArgumentValidationException($"File {opt.File} does not exist.");

            if (opt.Out == null)
                opt.Out = Path.GetDirectoryName(opt.File);
            else if (!Directory.Exists(opt.Out))
                throw new ArgumentValidationException($"Directory {opt.Out} does not exist.");

            ExtractFile();

            Logger.Info("Done");
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e);

            foreach (var s in helpText.ToString().Split('\n'))
            {
                Logger.Info(s);
            }

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

    class ArgumentValidationException : Exception
    {
        public ArgumentValidationException(string message) : base(message) { }
    }
}
