using System;
using System.Collections.Generic;
using System.IO;

namespace I3DShapesToolTest.Model
{
    /// <summary>
    /// 
    /// </summary>
    public static class GamePaths
    {
        private const string MyGamesDirectory = "My Games";

        private static readonly string MyGamesPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MyGamesDirectory);

        private static readonly IDictionary<FarmSimulatorVersion, string> VersionToNameSaveDirectory =
            new Dictionary<FarmSimulatorVersion, string>
            {
                {FarmSimulatorVersion.FarmingSimulator2019, "FarmingSimulator2019"},
                {FarmSimulatorVersion.FarmingSimulator2017, "FarmingSimulator2017"},
                {FarmSimulatorVersion.FarmingSimulator2015, "FarmingSimulator2015"},
            };

        private static readonly IDictionary<FarmSimulatorVersion, string> VersionToNameGameDirectory =
            new Dictionary<FarmSimulatorVersion, string>
            {
                {FarmSimulatorVersion.FarmingSimulator2019, "Farming Simulator 19"},
                {FarmSimulatorVersion.FarmingSimulator2017, "Farming Simulator 17"},
                {FarmSimulatorVersion.FarmingSimulator2015, "Farming Simulator 15"},
                {FarmSimulatorVersion.FarmingSimulator2013, "Farming Simulator 2013"},
            };

        public static string GetSavesPath(FarmSimulatorVersion version)
        {
            if (!VersionToNameSaveDirectory.ContainsKey(version))
            {
                throw new NotSupportedException();
            }
            return Path.Combine(MyGamesPath, VersionToNameSaveDirectory[version]);
        }

        public static string GetGamePath(FarmSimulatorVersion version)
        {
            return SteamHelper.GetGameDirectory(VersionToNameGameDirectory[version]);
        }

        public static string GetGameDataPath(FarmSimulatorVersion version)
        {
            var gamePath = GetGamePath(version);
            if (gamePath == null)
            {
                return null;
            }

            var gameDataPath = Path.Combine(gamePath, GameConstants.DataDirectory);
            return !Directory.Exists(gameDataPath) ? null : gameDataPath;
        }

        public static string GetGameMapsPath(FarmSimulatorVersion version)
        {
            var gameDataPath = GetGameDataPath(version);
            if (gameDataPath == null)
            {
                return null;
            }
            var gameMapsPath = Path.Combine(gameDataPath, GameConstants.MapDirectory);
            return !Directory.Exists(gameMapsPath) ? null : gameMapsPath;
        }
    }
}
