using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace I3DShapesTool.Configuration
{
    public class CommandLineOptions
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
                new Example("Basic usage (drag-drop a .i3d.shapes onto this application)", new CommandLineOptions { File = "k105.i3d.shapes" }),
                new Example("Show more messages", new CommandLineOptions { File = "k105.i3d.shapes", Verbose = true }),
                new Example("Specific output folder", new CommandLineOptions { File = "k105.i3d.shapes", Out = @"C:\Users\Me\Desktop\I3D Extract"})
            };
    }
}
