using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BacklightShifter {
    internal static class Storage {

        public static int GetLevel(PowerLineStatus powerStatus, int currentLevel) {
            Load();
            if (LevelPerPowerStatus.TryGetValue(powerStatus, out var level)) {
                return level;
            } else {
                LevelPerPowerStatus.Add(powerStatus, currentLevel);
                Save();
                Debug.WriteLine($"[Storage] Read: {powerStatus} {level}% (default)");
                return currentLevel;
            }
        }

        public static void SetLevel(PowerLineStatus powerStatus, int newLevel) {
            Load();
            if (LevelPerPowerStatus.TryGetValue(powerStatus, out var storedLevel)) {
                if (storedLevel != newLevel) {
                    LevelPerPowerStatus[powerStatus] = newLevel;
                    Debug.WriteLine($"[Storage] Write: {powerStatus} {newLevel}%");
                    Save();
                }
            } else {
                LevelPerPowerStatus.Add(powerStatus, newLevel);
                Debug.WriteLine($"[Storage] Write: {powerStatus} {newLevel}% (new)");
                Save();
            }
        }


        private static readonly Dictionary<PowerLineStatus, int> LevelPerPowerStatus = new Dictionary<PowerLineStatus, int>();

        private static bool isInitialized;

        private static void Load() {
            if (isInitialized) { return; }
            Debug.WriteLine($"[Storage] Load");

            GetPaths(out _, out var configFile);
            try {
                if (File.Exists(configFile)) {
                    var lines = File.ReadAllLines(configFile, Encoding.ASCII);
                    foreach (var line in lines) {
                        var parts = line.Split(':');
                        if (parts.Length == 2) {
                            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var keyInt)
                             && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) {
                                var key = (PowerLineStatus)keyInt;
                                if (!LevelPerPowerStatus.ContainsKey(key)) {
                                    LevelPerPowerStatus.Add(key, value);
                                }
                            }
                        }
                    }
                }
            } catch {
                // nothing to do in the case of exception and data is not critical anyhow
            }
            isInitialized = true;
        }

        private static void Save() {
            Debug.WriteLine($"[Storage] Save");
            GetPaths(out var configDir, out var configFile);
            try {
                if (!Directory.Exists(configDir)) { Directory.CreateDirectory(configDir); }
                var lines = new List<string>();
                foreach (var key in LevelPerPowerStatus.Keys) {
                    lines.Add(string.Format("{0}:{1}", (int)key, LevelPerPowerStatus[key]));
                }
                File.WriteAllLines(configFile, lines, Encoding.ASCII);
            } catch {
                // nothing to do in the case of exception and data is not critical anyhow
            }
        }

        private static void GetPaths(out string configDirPath, out string configFilePath) {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            configDirPath = Path.Combine(appData, "Medo64", "BacklightShifter");
            configFilePath = Path.Combine(configDirPath, "BacklightShifter.cfg");
        }

    }
}
