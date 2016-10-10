using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents a version of Factorio.
    /// </summary>
    class FactorioVersion
    {
        public const string LatestKey = "latest";

        /// <summary>
        /// Special class.
        /// </summary>
        private sealed class LatestFactorioVersion : FactorioVersion
        {
            public override string VersionString => LatestKey;

            public override string DisplayName => "Latest";

            public LatestFactorioVersion()
                : base(true, false)
            { }
        }

        static LatestFactorioVersion latest;

        /// <summary>
        /// The special 'latest' version.
        /// </summary>
        public static FactorioVersion Latest => latest ?? (latest = new LatestFactorioVersion());

        /// <summary>
        /// Loads all installed versions of Factorio.
        /// </summary>
        /// <returns>Returns a list that contains all installed Factorio versions.</returns>
        public static List<FactorioVersion> GetInstalledVersions()
        {
            var versionList = new List<FactorioVersion>();

            DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
            if (factorioDirectory.Exists)
            {
                foreach (var directory in factorioDirectory.EnumerateDirectories())
                {
                    Version version;
                    bool result = Version.TryParse(directory.Name, out version);

                    if (result)
                    {
                        var factorioVersion = new FactorioVersion(directory, version);
                        versionList.Add(factorioVersion);
                    }
                }
            }

            return versionList;
        }

        public bool IsSpecialVersion { get; }

        public bool IsFileSystemEditable { get; }

        public Version Version { get; }

        public virtual string VersionString => Version.ToString(3);

        public virtual string DisplayName => "Factorio " + VersionString;

        public DirectoryInfo Directory { get; }

        public string ExecutablePath { get; }

        protected FactorioVersion(bool isSpecialVersion, bool isFileSystemEditable)
        {
            IsSpecialVersion = isSpecialVersion;
            IsFileSystemEditable = isFileSystemEditable;
        }

        public FactorioVersion(DirectoryInfo directory, Version version, bool forceLinkCreation = false)
        {
            IsSpecialVersion = false;
            IsFileSystemEditable = true;

            Version = version;
            Directory = directory;

            string osPlatform = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            ExecutablePath = Path.Combine(directory.FullName, "bin", osPlatform, "factorio.exe");

            CreateLinks(forceLinkCreation);
        }

        private void CreateSaveDirectoryLink(DirectoryInfo localSaveDirectory)
        {
            var globalSaveDirectory = new DirectoryInfo(App.Instance.GlobalSavePath);
            if (!globalSaveDirectory.Exists) globalSaveDirectory.Create();

            var info = new ProcessStartInfo("cmd")
            {
                Arguments = $"/K mklink /J \"{localSaveDirectory.FullName}\" \"{globalSaveDirectory.FullName}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(info);
        }

        /// <summary>
        /// Creates the directory junction for saves.
        /// </summary>
        /// <param name="forced">If true, an existing link/directory will be deleted.</param>
        public void CreateSaveDirectoryLink(bool forced)
        {
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "saves"));
            if (forced && localSaveDirectory.Exists)
            {
                localSaveDirectory.DeleteRecursiveReparsePoint();
                CreateSaveDirectoryLink(localSaveDirectory);
            }
            else if (!localSaveDirectory.Exists)
            {
                CreateSaveDirectoryLink(localSaveDirectory);
            }
        }

        private void CreateScenarioDirectoryLink(DirectoryInfo localScenarioDirectory)
        {
            var globalScenarioDirectory = new DirectoryInfo(App.Instance.GlobalScenarioPath);
            if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

            var info = new ProcessStartInfo("cmd")
            {
                Arguments = $"/K mklink /J \"{localScenarioDirectory.FullName}\" \"{globalScenarioDirectory.FullName}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(info);
        }


        /// <summary>
        /// Creates the directory junction for scenarios.
        /// </summary>
        /// <param name="forced">If true, an existing link/directory will be deleted.</param>
        public void CreateScenarioDirectoryLink(bool forced)
        {
            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "scenarios"));
            if (forced && localScenarioDirectory.Exists)
            {
                localScenarioDirectory.DeleteRecursiveReparsePoint();
                CreateScenarioDirectoryLink(localScenarioDirectory);
            }
            else if (!localScenarioDirectory.Exists)
            {
                CreateScenarioDirectoryLink(localScenarioDirectory);
            }
        }

        private void CreateModDirectoryLink(DirectoryInfo localModDirectory)
        {
            var globalModDirectory = App.Instance.Settings.GetModDirectory(Version);
            if (!globalModDirectory.Exists) globalModDirectory.Create();

            var info = new ProcessStartInfo("cmd")
            {
                Arguments = $"/K mklink /J \"{localModDirectory.FullName}\" \"{globalModDirectory.FullName}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(info);
        }

        /// <summary>
        /// Creates the directory junction for mods.
        /// </summary>
        /// <param name="forced">If true, an existing link/directory will be deleted.</param>
        public void CreateModDirectoryLink(bool forced)
        {
            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (forced && localModDirectory.Exists)
            {
                localModDirectory.DeleteRecursiveReparsePoint();
                CreateModDirectoryLink(localModDirectory);
            }
            else if (!localModDirectory.Exists)
            {
                CreateModDirectoryLink(localModDirectory);
            }
        }

        /// <summary>
        /// Creates all directory junctions.
        /// </summary>
        /// <param name="forced"></param>
        public void CreateLinks(bool forced)
        {
            CreateSaveDirectoryLink(forced);
            CreateScenarioDirectoryLink(forced);
            CreateModDirectoryLink(forced);
        }

        /// <summary>
        /// Deletes all directory junctions.
        /// </summary>
        public void DeleteLinks()
        {
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "saves"));
            if (localSaveDirectory.Exists) localSaveDirectory.Delete(false);

            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "scenarios"));
            if (localScenarioDirectory.Exists) localScenarioDirectory.Delete(false);

            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (localModDirectory.Exists) localModDirectory.Delete(false);
        }
    }
}
