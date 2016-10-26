using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
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
        }

        static LatestFactorioVersion latest;

        /// <summary>
        /// The special 'latest' version.
        /// </summary>
        public static FactorioVersion Latest => latest ?? (latest = new LatestFactorioVersion());

        DirectoryInfo linkDirectory;

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

        private static bool TryExtractVersion(Stream stream, out Version version)
        {
            version = null;

            using (var reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();
                MatchCollection matches = Regex.Matches(content, @"[0-9]+\.[0-9]+\.[0-9]+",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count == 0) return false;

                string versionString = matches[0].Value;
                version = Version.Parse(versionString);
                return true;
            }
        }

        /// <summary>
        /// Checks if an archive file contains a valid installation of Factorio.
        /// </summary>
        /// <param name="archiveFile">The archive file to check.</param>
        /// <param name="validVersion">Out. The version of Factorio contained in the archive file.</param>
        /// <returns>Returns true if the archive file contains a valid Factorio installation, otherwise false.</returns>
        public static bool ArchiveFileValid(FileInfo archiveFile, out Version validVersion)
        {
            validVersion = null;

            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("data/base/info.json"))
                    {
                        using (Stream stream = entry.Open())
                        {
                            if (TryExtractVersion(stream, out validVersion)) return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a directory contains a valid installation of Factorio.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="validVersion">Out. The version of Factorio contained in the directory.</param>
        /// <returns>Returns true if the directory contains a valid Factorio installation, otherwise false.</returns>
        public static bool LocalInstallationValid(DirectoryInfo directory, out Version validVersion)
        {
            validVersion = null;

            FileInfo infoFile = new FileInfo(Path.Combine(directory.FullName, @"data\base\info.json"));
            if (infoFile.Exists)
            {
                using (Stream stream = infoFile.OpenRead())
                {
                    if (TryExtractVersion(stream, out validVersion)) return true;
                }
            }

            return false;
        }

        public bool IsSpecialVersion { get; }

        public bool IsFileSystemEditable { get; }

        public Version Version { get; }

        public virtual string VersionString => Version.ToString(3);

        public virtual string DisplayName => "Factorio " + VersionString;

        public DirectoryInfo Directory { get; private set; }

        public string ExecutablePath { get; private set; }

        private void SetExecutablePath()
        {
            string osPlatform = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            ExecutablePath = Path.Combine(Directory.FullName, "bin", osPlatform, "factorio.exe");
        }

        private FactorioVersion()
        {
            IsSpecialVersion = true;
            IsFileSystemEditable = false;
        }

        protected FactorioVersion(bool isFileSystemEditable, DirectoryInfo directory, DirectoryInfo linkDirectory, Version version, bool forceLinkCreation = false)
        {
            IsSpecialVersion = false;
            IsFileSystemEditable = isFileSystemEditable;
            Directory = directory;
            this.linkDirectory = linkDirectory;
            Version = version;

            SetExecutablePath();

            if (!linkDirectory.Exists) linkDirectory.Create();
            CreateLinks(forceLinkCreation);
        }

        public FactorioVersion(DirectoryInfo directory, Version version, bool forceLinkCreation = false)
        {
            IsSpecialVersion = false;
            IsFileSystemEditable = true;

            Version = version;
            Directory = directory;
            linkDirectory = directory;

            SetExecutablePath();

            CreateLinks(forceLinkCreation);
        }

        protected virtual void UpdateDirectoryInner(DirectoryInfo newDirectory)
        {
            Directory = newDirectory;
            SetExecutablePath();
        }

        protected virtual void UpdateLinkDirectoryInner(DirectoryInfo newDirectory)
        {
            linkDirectory = newDirectory;
        }

        /// <summary>
        /// Updates the directory of this version of Factorio.
        /// </summary>
        /// <param name="newDirectory"></param>
        public void UpdateDirectory(DirectoryInfo newDirectory)
        {
            UpdateDirectoryInner(newDirectory);
            UpdateLinkDirectoryInner(newDirectory);
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
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "saves"));
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
            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "scenarios"));
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
            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "mods"));
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
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "saves"));
            if (localSaveDirectory.Exists) localSaveDirectory.Delete(false);

            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "scenarios"));
            if (localScenarioDirectory.Exists) localScenarioDirectory.Delete(false);

            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "mods"));
            if (localModDirectory.Exists) localModDirectory.Delete(false);
        }
    }
}
