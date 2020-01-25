using System;
using System.IO;
using System.Linq;
using I3DShapesTool.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;
using LogLevel = NLog.LogLevel;

namespace I3DShapesTool
{
    class Program
    {
        private static NLogLoggerProvider LogProvider = new NLogLoggerProvider();

        public static readonly Microsoft.Extensions.Logging.ILogger Logger = LogProvider.CreateLogger("all");

        private static void Main(string[] args)
        {
            SetupLogging();

            var commandLineOptions = CommandLineOptions.Parse(args, Logger);

            if (commandLineOptions == null)
            {
                return;
            }

            ExtractFile(commandLineOptions);
            Logger.LogInformation("Done");
            Logger.LogInformation("Press enter to exit...");
            Console.Read();

            LogManager.Shutdown();
        }

        private static void SetupLogging(int verbosity = 0)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new NLog.Targets.ConsoleTarget("logConsole");
            logconsole.Layout = new SimpleLayout("[${level:uppercase=true}] ${message}");

            LogLevel minLevel;
            if (verbosity >= 2)
                minLevel = LogLevel.Trace;
            else if (verbosity == 1)
                minLevel = LogLevel.Debug;
            else
                minLevel = LogLevel.Info;

            config.AddRule(minLevel, LogLevel.Fatal, logconsole);

            LogManager.Configuration = config;
        }

        private static void ExtractFile(CommandLineOptions commandLineOptions)
        {
            var file = new I3DShapesFile(Logger);
            file.Load(commandLineOptions.InputFile);

            string folder;
            if (commandLineOptions.CreateDir)
            {
                folder = Path.Combine(commandLineOptions.OutPath, "extract_" + file.FileName);
                Directory.CreateDirectory(folder);
            }
            else
            {
                folder = commandLineOptions.OutPath;
            }

            foreach (var shape in file.Shapes)
            {
                if (commandLineOptions.DumpBinary)
                {
                    var binFileName = $"shape_{shape.Name}.bin";
                    File.WriteAllBytes(Path.Combine(folder, CleanFileName(binFileName)), shape.RawData);
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

        /// <summary>
        /// https://stackoverflow.com/a/7393722/2911165
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
