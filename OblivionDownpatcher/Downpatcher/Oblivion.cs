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
                var version = FileVersionInfo.GetVersionInfo(Path.Combine(gameDir.FullName, "Oblivion.exe"));
                string fullVer = version.ProductVersion;
                switch(version.ProductVersion) {
                    case "1.0.228":
                        fullVer = "1.0";
                        fullVer += (NoCDPatcher.IsCDPatched(Path.Combine(gameDir.FullName, "Oblivion.exe")) ? " (No CD)" : " (CD)");
                        break;
                    case "1.2.0416":
                        fullVer = "1.2";
                        break;
                }
                return fullVer;
            }
            catch(Exception ex) {
                return "Unknown";
            }
        }
    }
}
