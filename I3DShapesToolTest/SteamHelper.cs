using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace I3DShapesToolTest
{
    /// <summary>
    /// Base on: https://stackoverflow.com/questions/54767662/finding-game-launcher-executables-in-directory-c-sharp
    /// </summary>
    public static class SteamHelper
    {
        private static ICollection<string> RegistryKeys = new[] { "SOFTWARE\\Wow6432Node\\Valve\\", "SOFTWARE\\VALVE\\" };
        private static ICollection<string> _steamGameDirs = new List<string>();

        static SteamHelper()
        {
            UpdateSteamGameDirectories();
        }

        private static void UpdateSteamGameDirectories()
        {
            _steamGameDirs = new List<string>();
            RegistryKeys.Select(v => Registry.LocalMachine.OpenSubKey(v))
                .Where(registryKey => registryKey != null)
                .SelectMany(registryKey =>
                {
                    using (registryKey)
                    {
                        return GetDirectories(registryKey).ToArray();
                    }
                })
                .ToList()
                .ForEach(directoryName => _steamGameDirs.Add(directoryName));
        }

        private static IEnumerable<string> GetDirectories(RegistryKey registryKey)
        {
            foreach (var subKeyName in registryKey.GetSubKeyNames())
            {
                using (var subKey = registryKey.OpenSubKey(subKeyName))
                {
                    var steamPath = subKey?.GetValue("InstallPath")?.ToString();
                    if (steamPath == null)
                        continue;
                    var configPath = $"{steamPath}/steamapps/libraryfolders.vdf";
                    const string driveRegex = @"[A-Z]:\\";
                    if (!File.Exists(configPath))
                        continue;
                    var configLines = File.ReadAllLines(configPath);
                    foreach (var item in configLines)
                    {
                        var match = Regex.Match(item, driveRegex);
                        if (item == string.Empty || !match.Success) continue;
                        var matched = match.ToString();
                        var item2 = item.Substring(item.IndexOf(matched, StringComparison.Ordinal));
                        item2 = item2.Replace("\\\\", "\\");
                        item2 = item2.Replace("\"", "\\steamapps\\common\\");
                        yield return item2;
                    }
                    yield return $"{steamPath}\\steamapps\\common\\";
                }
            }
        }

        public static string GetGameDirectory(string gameFolderName)
        {
            return _steamGameDirs.Select(v => Path.Combine(v, gameFolderName))
                .FirstOrDefault(v => Directory.Exists(v));
        }
    }
}