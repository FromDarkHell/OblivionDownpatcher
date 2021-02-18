using System;
using System.IO;
using System.Diagnostics;
using DownpatcherSharp;
using OblivionDownpatcher.NoCD;

namespace OblivionDownpatcher.Downpatcher {
    public class Oblivion : Game {
        public Oblivion(string filePath, string gameName) : base(filePath, gameName) { }
        public override string getCurrentPatch() {
            try {
                string exePath = Path.Combine(gameDir.FullName, "Oblivion.exe");

                var version = FileVersionInfo.GetVersionInfo(exePath);
                string fullVer = version.ProductVersion;

                if (NoCDPatcher.CanApplyCDPatch(exePath))
                    fullVer += (NoCDPatcher.IsCDPatched(exePath) ? " (No CD)" : " (CD)");

                return fullVer;
            }
            catch (Exception) { return "Unknown"; }
        }

        public void ApplyINIFix() {
            string patchedINIPath = Path.Combine(gameDir.FullName, "Oblivion.ini");
            if (!File.Exists(patchedINIPath)) { // The user doesn't have an `Oblivion.ini` in their root install.
                string baseINIPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Oblivion", "Oblivion.ini");
                if (File.Exists(baseINIPath)) {
                    File.Copy(baseINIPath, patchedINIPath);
                }
            }
            // Now we edit the INI to have `bUseMyGamesDirectory=0`
            string data = File.ReadAllText(patchedINIPath);
            data = data.Replace("bUseMyGamesDirectory=0", "bUseMyGamesDirectory=1");
            File.WriteAllText(patchedINIPath, data);
        }
    }
}
