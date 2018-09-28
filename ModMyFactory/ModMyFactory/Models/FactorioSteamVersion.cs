using System;
using System.IO;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents the Steam version of Factorio.
    /// </summary>
    sealed class FactorioSteamVersion : FactorioVersion
    {
        public static string SteamAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");

        /// <summary>
        /// Tries to load the Steam version of Factorio specified in the settings.
        /// </summary>
        /// <param name="steamVersion">Out. The Steam version.</param>
        /// <returns>Returns true if the Steam version has been loaded sucessfully, otherwise false.</returns>
        public static bool TryLoad(out FactorioVersion steamVersion)
        {
            if (string.IsNullOrEmpty(App.Instance.Settings.SteamVersionPath))
            {
                steamVersion = null;
                return false;
            }

            var directory = new DirectoryInfo(App.Instance.Settings.SteamVersionPath);
            if (FactorioFolder.TryLoad(directory, out var folder))
            {
                steamVersion = new FactorioSteamVersion(folder);
                return true;
            }
            else
            {
                steamVersion = null;
                return false;
            }
        }

        protected override string LoadName()
        {
            return "Steam";
        }

        private FactorioSteamVersion(FactorioFolder folder)
            : base(folder, false, new DirectoryInfo(SteamAppDataPath))
        { }
    }
}
