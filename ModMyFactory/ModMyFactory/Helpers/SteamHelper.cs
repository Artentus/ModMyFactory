using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ModMyFactory.Helpers
{
    static class SteamHelper
    {
        static bool? steamInstalled = null;
        static string installPath = string.Empty;
        static List<DirectoryInfo> steamLibraries = null;

        /// <summary>
        /// Tries to read the Steam installation path from the Registry.
        /// </summary>
        /// <param name="path">Out. If the action was sucessful, the full path to the Steam installation directory.</param>
        /// <returns>Returns true if the path was found in the Registry, otherwise false.</returns>
        public static bool TryGetSteamInstallPath(out string path, bool cache = true)
        {
            if (cache && steamInstalled.HasValue)
            {
                path = installPath;
                return steamInstalled.Value;
            }

            RegistryKey softwareKey = null;
            try
            {
                string softwarePath = Environment.Is64BitProcess ? @"SOFTWARE\WOW6432Node" : "SOFTWARE";
                softwareKey = Registry.LocalMachine.OpenSubKey(softwarePath, false);

                using (var key = softwareKey.OpenSubKey(@"Valve\Steam"))
                {
                    var obj = key.GetValue("InstallPath");
                    installPath = obj as string;
                    steamInstalled = (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath));

                    path = installPath;
                    return steamInstalled.Value;
                }
            }
            catch
            {
                path = null;
                return false;
            }
            finally
            {
                if (softwareKey != null)
                    softwareKey.Close();
            }
        }

        /// <summary>
        /// Checks if Steam is installed on the system.
        /// </summary>
        /// <returns>Returns true if Steam is installed on the system, otherwise false.</returns>
        public static bool IsSteamInstalled(bool cache = true)
        {
            return TryGetSteamInstallPath(out var path, cache);
        }

        private static List<string> ReadLibraryPaths(string steamInstallPath)
        {
            var libraryPaths = new List<string>();

            var vdfFile = new FileInfo(Path.Combine(steamInstallPath, @"steamapps\libraryfolders.vdf"));
            if (!vdfFile.Exists) return libraryPaths;

            string content;
            using (var stream = vdfFile.OpenRead())
            {
                using (var reader = new StreamReader(stream))
                    content = reader.ReadToEnd();
            }
            
            var matches = Regex.Matches(content, "\"\\d\"\\s+\"(?<path>.+)\"");
            foreach (Match match in matches)
            {
                string path = match.Groups["path"].Value;
                path = path.Replace(@"\\", @"\");
                libraryPaths.Add(path);
            }

            return libraryPaths;
        }

        private static DirectoryInfo GetLibrary(string basePath)
        {
            return new DirectoryInfo(Path.Combine(basePath, @"steamapps\common"));
        }

        /// <summary>
        /// Gets a list of Steam libraries on the system.
        /// </summary>
        /// <returns>
        /// Returns a list of Steam libraries on the system.
        /// The list can be empty if Steam is not installed or no Steam games are installed.
        /// </returns>
        public static List<DirectoryInfo> ListSteamLibraries(bool cache = true)
        {
            if (cache && (steamLibraries != null))
                return steamLibraries;

            steamLibraries = new List<DirectoryInfo>();
            if (!TryGetSteamInstallPath(out string steamInstallPath, cache)) return steamLibraries;

            var mainDir = GetLibrary(steamInstallPath);
            if (mainDir.Exists) steamLibraries.Add(mainDir);

            var libraryPaths = ReadLibraryPaths(steamInstallPath);
            foreach (var path in libraryPaths)
            {
                var dir = GetLibrary(path);
                if (dir.Exists) steamLibraries.Add(dir);
            }

            return steamLibraries;
        }
    }
}
