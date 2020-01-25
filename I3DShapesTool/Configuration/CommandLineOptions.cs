using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using NDesk.Options;

namespace I3DShapesTool.Configuration
{
    public class CommandLineOptions
    {
        public int Verbosity { get; set; }
        public bool CreateDir { get; set; }
        public string OutPath { get; set; }
        public bool DumpBinary { get; set; }
        public string InputFile { get; set; }


        public static CommandLineOptions Parse(string[] args, ILogger logger)
        {
            var commandLineOptions = new CommandLineOptions();

            var showHelp = false;

            var p = new OptionSet
            {
                {
                    "h|help",
                    "show this message and exit",
                    v => showHelp = v != null
                },
                {
                    "v|verbose",
                    "increase debug message verbosity",
                    v =>
                    {
                        if (v != null)
                        {
                            commandLineOptions.Verbosity++;
                        }
                    }
                },
                {
                    "d|createdir",
                    "extract the files to a folder in the output directory instead of directly to the output directory",
                    v => commandLineOptions.CreateDir = v != null
                },
                {
                    "out:",
                    "the {DIRECTORY} files should be extracted to\ndefaults to the directory of the input file",
                    v => commandLineOptions.OutPath = v
                },
                {
                    "b|bin", "dump the raw decrypted binary file", v => commandLineOptions.DumpBinary = v != null
                },
            };

            try
            {
                var extra = p.Parse(args);

                if (extra.Count == 0)
                {
                    showHelp = true;
                }
                else
                {
                    commandLineOptions.InputFile = extra[0];
                    if (!File.Exists(commandLineOptions.InputFile))
                    {
                        throw new OptionException("File doesn't exist.", "--file");
                    }
                }

                if (string.IsNullOrWhiteSpace(commandLineOptions.OutPath))
                {
                    commandLineOptions.OutPath = Path.GetDirectoryName(commandLineOptions.InputFile);
                }
                else
                {
                    if (!Directory.Exists(commandLineOptions.OutPath))
                    {
                        throw new OptionException("Directory doesn't exist.", "--out");
                    }
                }
            }
            catch (OptionException e)
            {
                logger.LogInformation(e.Message);
                logger.LogInformation("Try 'I3DShapesTool --help' for more information.");
                return null;
            }

            if (showHelp)
            {
                logger.LogInformation("This program needs to be run from a batch file or Windows command line.");
                ShowHelp(p, logger);
                logger.LogInformation("Press enter to exit...");
                Console.Read();
                return null;
            }

            return commandLineOptions;
        }



        private static void ShowHelp(OptionSet p, ILogger logger)
        {
            logger.LogInformation("Usage: I3DShapesTool [-dhv --out=outPath] [-b] inFile");
            logger.LogInformation("Extract model data from GIANTS engine's .i3d.shapes files.");

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
                logger.LogInformation(s);
            }
        }
    }
}