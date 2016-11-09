using System;
using System.IO;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents the Steam version of Factorio.
    /// </summary>
    sealed class FactorioSteamVersion : FactorioVersion
    {
        public const string Key = "steam";

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

            var steamVersionDirectory = new DirectoryInfo(App.Instance.Settings.SteamVersionPath);
            Version version;
            if (steamVersionDirectory.Exists && FactorioVersion.LocalInstallationValid(steamVersionDirectory, out version))
            {
                steamVersion = new FactorioSteamVersion(steamVersionDirectory, version);
                return true;
            }
            else
            {
                App.Instance.Settings.SteamVersionPath = string.Empty;
                App.Instance.Settings.Save();

                steamVersion = null;
                return false;
            }
        }

        public static string SteamAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");

        public override string VersionString => Key;

        public override string DisplayName => $"Steam ({Version.ToString(3)})";

        public FactorioSteamVersion(DirectoryInfo directory, Version version)
            : base(false, directory, new DirectoryInfo(SteamAppDataPath), version)
        { }

        protected override void UpdateLinkDirectoryInner(DirectoryInfo newDirectory)
        {
            base.UpdateLinkDirectoryInner(new DirectoryInfo(SteamAppDataPath));
        }
    }
}
