using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;

namespace I3DShapesToolTest
{
    class SteamHelper
    {
        private static List<string> steamGameDirs;
        
        public static void LoadSteamGameDirectories()
        {
            steamGameDirs = new List<string>();
            const string steam32 = "SOFTWARE\\VALVE\\";
            const string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
            var key32 = Registry.LocalMachine.OpenSubKey(steam32);
            var key64 = Registry.LocalMachine.OpenSubKey(steam64);
            if (key64?.ToString() == null || key64.ToString() == "")
            {
                foreach (var k32SubKey in key32.GetSubKeyNames())
                {
                    using (var subKey = key32.OpenSubKey(k32SubKey))
                    {
                        var steam32Path = subKey?.GetValue("InstallPath").ToString();
                        var config32Path = $"{steam32Path}/steamapps/libraryfolders.vdf";
                        const string driveRegex = @"[A-Z]:\\";
                        if (!File.Exists(config32Path)) continue;
                        var configLines = File.ReadAllLines(config32Path);
                        foreach (var item in configLines)
                        {
                            //Console.WriteLine($"32:  {item}");
                            var match = Regex.Match(item, driveRegex);
                            if (item == string.Empty || !match.Success) continue;
                            var matched = match.ToString();
                            var item2 = item.Substring(item.IndexOf(matched, StringComparison.Ordinal));
                            item2 = item2.Replace("\\\\", "\\");
                            item2 = item2.Replace("\"", "\\steamapps\\common\\");
                            steamGameDirs.Add(item2);
                        }
                        steamGameDirs.Add($"{steam32Path}\\steamapps\\common\\");
                    }
                }
            }
            foreach (var k64SubKey in key64.GetSubKeyNames())
            {
                using (var subKey = key64.OpenSubKey(k64SubKey))
                {
                    var steam64Path = subKey?.GetValue("InstallPath").ToString();
                    var config64Path = $"{steam64Path}/steamapps/libraryfolders.vdf";
                    const string driveRegex = @"[A-Z]:\\";
                    if (!File.Exists(config64Path)) continue;
                    var configLines = File.ReadAllLines(config64Path);
                    foreach (var item in configLines)
                    {
                        //Console.WriteLine($"64:  {item}");
                        var match = Regex.Match(item, driveRegex);
                        if (item == string.Empty || !match.Success) continue;
                        var matched = match.ToString();
                        var item2 = item.Substring(item.IndexOf(matched, StringComparison.Ordinal));
                        item2 = item2.Replace("\\\\", "\\");
                        item2 = item2.Replace("\"", "\\steamapps\\common\\");
                        steamGameDirs.Add(item2);
                    }
                    steamGameDirs.Add($"{steam64Path}\\steamapps\\common\\");
                }
            }
        }

        public static string GetGameDirectory(string gameFolderName)
        {
            foreach (var steamGameDir in steamGameDirs)
            {
                if (Directory.Exists(Path.Combine(steamGameDir, gameFolderName)))
                    return Path.Combine(steamGameDir, gameFolderName);
            }

            Assert.Inconclusive($"{gameFolderName} not found installed.");

            return null;
        }
    }
}
