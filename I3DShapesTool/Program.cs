using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using I3DShapesTool.Configuration;
using I3DShapesTool.Lib.Container;
using I3DShapesTool.Lib.Export;
using I3DShapesTool.Lib.Model;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Layouts;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = NLog.LogLevel;

namespace I3DShapesTool
{
    class Program
    {
        public static readonly ILoggerProvider LoggerProvider = new NLog.Extensions.Logging.NLogLoggerProvider();
        public static readonly ILogger Logger = LoggerProvider.CreateLogger("all");

        private static void Main(string[] args)
        {
            SetupLogging();

            try
            {
                var result = Parser.Default.ParseArguments<CommandLineOptions>(args);
                result
                    .WithParsed(Run)
                    .WithNotParsed(errs => DisplayHelp(result, errs));
            }
            catch (ArgumentValidationException e)
            {
                Logger.LogError(e.Message);
                Console.Read();
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void SetupLogging(CommandLineOptions options = null)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new NLog.Targets.ConsoleTarget("logConsole")
            {
                Layout = new SimpleLayout("[${level:uppercase=true}] ${message}")
            };

            var minLevel = LogLevel.Info;
            if (options != null)
            {
                minLevel = options.Verbose ? LogLevel.Debug : LogLevel.Info;
                if (options.Quiet)
                    minLevel = LogLevel.Error;
            }

            config.AddRule(minLevel, LogLevel.Fatal, logconsole);

            LogManager.Configuration = config;
        }

        /// <summary>
        /// Tests if the shapes loaded from this file are valid shapes
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsFileLoadSuccessful(ShapesFile file)
        {
            // All shape names should contain only valid ASCII characters, if they don't we know something has gone wrong
            return file.Shapes.SelectMany(shape => shape.Name).All(c => c >= 0x20 && c <= 0x7E);
        }

        private static ShapesFile LoadFileBruteForce(CommandLineOptions options)
        {
            var file = new ShapesFile(Logger);
            var success = false;

            byte seed;
            for (seed = 0; seed < 0xFF; seed++)
            {
                try
                {
                    file.Load(options.File, seed);
                }
                catch (DecryptFailureException)
                {
                    continue;
                }

                if (!IsFileLoadSuccessful(file))
                    continue;

                success = true;
                break;
            }

            if (!success)
            {
                Logger.LogWarning("Failed to find any matching seed for this file.");
                return null;
            }

            Logger.LogInformation($"Found successful seed {seed}");
            return file;
        }

        private static ShapesFile LoadFile(CommandLineOptions options)
        {
            var file = new ShapesFile(Logger);

            Logger.LogInformation($"Loading file: {Path.GetFileName(options.File)}");
            try
            {
                file.Load(options.File);
            }
            catch (DecryptFailureException)
            {
                Logger.LogInformation("Failed decrypting file. Attempting to brute-force the seed...");
                return LoadFileBruteForce(options);
            }

            return file;
        }

        private static void ExtractFile(CommandLineOptions options)
        {
            var file = LoadFile(options);
            if (file == null)
                return;

            string folder;
            if (options.CreateDir)
            {
                folder = Path.Combine(options.Out, "extract_" + Path.GetFileName(file.FilePath));
                Directory.CreateDirectory(folder);
            }
            else
            {
                folder = options.Out;
            }

            foreach (var shape in file.Shapes)
            {
                if (options.DumpBinary)
                {
                    var binFileName = $"shape_{shape.Name}.bin";
                    File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), shape.RawData);
                }

                var mdlFileName = Path.Combine(folder, CleanFileName(shape.Name + ".obj"));

                var objFileInternalName = Path.GetFileName(file.FilePath).Replace(".i3d.shapes", "");
                var objfile = new WavefrontObj(shape, objFileInternalName);
                var dataBlob = objfile.ExportToBlob();

                if (File.Exists(mdlFileName))
                    File.Delete(mdlFileName);

                File.WriteAllBytes(mdlFileName, dataBlob);
            }
        }

        private static void Run(CommandLineOptions options)
        {
            SetupLogging(options); // Set it up again now that we have verbosity information
            
            if (!File.Exists(options.File))
                throw new ArgumentValidationException($"File {options.File} does not exist.");

            if (options.Out == null)
                options.Out = Path.GetDirectoryName(options.File);
            else if (!Directory.Exists(options.Out))
                throw new ArgumentValidationException($"Directory {options.Out} does not exist.");

            ExtractFile(options);

            Logger.LogInformation("Done");
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e);

            foreach (var s in helpText.ToString().Split('\n'))
            {
                Logger.LogInformation(s);
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
