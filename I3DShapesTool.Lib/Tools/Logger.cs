using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace I3DShapesTool.Lib.Tools
{
    /// <summary>
    /// Singleton logger for the library
    /// Set the Instance property to a logger of your choice.
    /// </summary>
    public class Logger
    {
        public static ILogger Instance { get; set; } = NullLogger.Instance;
    }
}
