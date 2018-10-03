using ModMyFactory.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents the Steam version of Factorio.
    /// </summary>
    sealed class FactorioSteamVersion : FactorioVersion
    {
        const int AppId = 427520;
        
        public static string SteamAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");
        
        /// <summary>
        /// Tries to load the Steam version of Factorio.
        /// </summary>
        /// <param name="steamVersion">Out. The Steam version.</param>
        /// <returns>Returns true if the Steam version has been loaded sucessfully, otherwise false.</returns>
        public static bool TryLoad(out FactorioVersion steamVersion)
        {
            steamVersion = null;

            var steamLibraries = SteamHelper.ListSteamLibraries(true);
            if ((steamLibraries == null) || (steamLibraries.Count == 0)) return false;

            foreach (var library in steamLibraries)
            {
                var factorioDir = library.EnumerateDirectories("Factorio").FirstOrDefault();
                if (factorioDir != null)
                {
                    if (FactorioFolder.TryLoad(factorioDir, out var folder))
                    {
                        if (folder.Is64Bit == Environment.Is64BitOperatingSystem)
                        {
                            steamVersion = new FactorioSteamVersion(folder);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the Factorio Steam version is present on the system.
        /// </summary>
        /// <returns>Returns true if the Factorio Steam version is present on the system, otherwise false.</returns>
        public static bool IsPresent()
        {
            var steamLibraries = SteamHelper.ListSteamLibraries(false);
            if ((steamLibraries == null) || (steamLibraries.Count == 0)) return false;

            foreach (var library in steamLibraries)
            {
                var factorioDir = library.EnumerateDirectories("Factorio").FirstOrDefault();
                if (factorioDir != null)
                {
                    if (FactorioFolder.TryLoad(factorioDir, out var folder))
                    {
                        if (folder.Is64Bit == Environment.Is64BitOperatingSystem)
                            return true;
                    }
                }
            }

            return false;
        }

        protected override string LoadName()
        {
            return "Steam";
        }

        /// <summary>
        /// Runs Factorio.
        /// </summary>
        /// <param name="args">Optional. Command line args.</param>
        public override void Run(string args = null)
        {
            string steamPath;
            if (!SteamHelper.TryGetSteamInstallPath(out steamPath)) return;

            var startInfo = new ProcessStartInfo(Path.Combine(steamPath, "Steam.exe"));
            startInfo.Arguments = $"-applaunch {AppId}";
            if (!string.IsNullOrWhiteSpace(args)) startInfo.Arguments += $" {args}";

            Process.Start(startInfo);
        }

        /// <summary>
        /// Deletes this Factorio installation.
        /// </summary>
        public override void Delete()
        {
            DeleteLinks();

            App.Instance.Settings.LoadSteamVersion = false;
            App.Instance.Settings.Save();
        }

        private FactorioSteamVersion(FactorioFolder folder)
            : base(folder, false, new DirectoryInfo(SteamAppDataPath))
        { }
    }
}
