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

        public static string SteamAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");

        public override string VersionString => Key;

        public override string DisplayName => $"Steam ({Version.ToString(3)})";

        public FactorioSteamVersion(DirectoryInfo directory, Version version, bool forceLinkCreation = false)
            : base(false, directory, new DirectoryInfo(SteamAppDataPath), version, forceLinkCreation)
        { }
    }
}
