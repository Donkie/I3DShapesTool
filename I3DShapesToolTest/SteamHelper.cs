using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Xunit;

namespace I3DShapesToolTest
{
    /// <summary>
    /// Base on: https://stackoverflow.com/questions/54767662/finding-game-launcher-executables-in-directory-c-sharp
    /// </summary>
    public static class SteamHelper
    {
        private static readonly ICollection<string> registryKeys = new[] { "SOFTWARE\\Wow6432Node\\Valve\\", "SOFTWARE\\VALVE\\" };
        private static ICollection<string> steamGameDirs = new List<string>();

        static SteamHelper()
        {
            UpdateSteamGameDirectories();
        }

        private static void UpdateSteamGameDirectories()
        {
            steamGameDirs = new List<string>();
            registryKeys.Select(v => Registry.LocalMachine.OpenSubKey(v))
                .Where(registryKey => registryKey != null)
                .SelectMany(registryKey =>
                {
                    using(registryKey)
                    {
                        return GetDirectories(registryKey).ToArray();
                    }
                })
                .ToList()
                .ForEach(directoryName => steamGameDirs.Add(directoryName));
        }

        private static IEnumerable<string> GetDirectories(RegistryKey registryKey)
        {
            foreach(string subKeyName in registryKey.GetSubKeyNames())
            {
                using RegistryKey subKey = registryKey.OpenSubKey(subKeyName);
                string steamPath = subKey?.GetValue("InstallPath")?.ToString();
                if(steamPath == null)
                    continue;
                string configPath = $"{steamPath}/steamapps/libraryfolders.vdf";
                string driveRegex = @"[A-Z]:\\";
                if(!File.Exists(configPath))
                    continue;
                string[] configLines = File.ReadAllLines(configPath);
                foreach(string item in configLines)
                {
                    Match match = Regex.Match(item, driveRegex);
                    if(item == string.Empty || !match.Success)
                        continue;
                    string matched = match.ToString();
                    string item2 = item[item.IndexOf(matched, StringComparison.Ordinal)..];
                    item2 = item2.Replace("\\\\", "\\");
                    item2 = item2.Replace("\"", "\\steamapps\\common\\");
                    yield return item2;
                }
                yield return $"{steamPath}\\steamapps\\common\\";
            }
        }

        public static string GetGameDirectory(string gameFolderName)
        {
            return steamGameDirs
                .Select(v => Path.Combine(v, gameFolderName))
                .FirstOrDefault(v => Directory.Exists(v));
        }

        public static string GetGameDirectoryOrSkip(string gameFolderName)
        {
            string gameDir = GetGameDirectory(gameFolderName);
            Skip.If(gameDir == null);
            return gameDir;
        }
    }
}