using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ModMyFactory.IO;

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

        protected FactorioVersion(bool isFileSystemEditable, DirectoryInfo directory, DirectoryInfo linkDirectory, Version version)
        {
            IsSpecialVersion = false;
            IsFileSystemEditable = isFileSystemEditable;
            Directory = directory;
            this.linkDirectory = linkDirectory;
            Version = version;

            SetExecutablePath();

            if (!linkDirectory.Exists) linkDirectory.Create();
            CreateLinks();
        }

        public FactorioVersion(DirectoryInfo directory, Version version)
        {
            IsSpecialVersion = false;
            IsFileSystemEditable = true;

            Version = version;
            Directory = directory;
            linkDirectory = directory;

            SetExecutablePath();

            CreateLinks();
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
        public void UpdateDirectory(DirectoryInfo newDirectory)
        {
            if (!IsSpecialVersion)
            {
                UpdateDirectoryInner(newDirectory);
                UpdateLinkDirectoryInner(newDirectory);
            }
        }

        private void CreateSaveDirectoryLinkInternal(string localSavePath)
        {
            var globalSaveDirectory = new DirectoryInfo(App.Instance.GlobalSavePath);
            if (!globalSaveDirectory.Exists) globalSaveDirectory.Create();

            var localSaveJunction = new JunctionInfo(localSavePath);
            localSaveJunction.Create(globalSaveDirectory.FullName);
        }

        /// <summary>
        /// Creates the directory junction for saves.
        /// </summary>
        public void CreateSaveDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "saves"));
                if (!Junction.Exists(localSaveDirectory.FullName) && localSaveDirectory.Exists) localSaveDirectory.Delete(true);

                CreateSaveDirectoryLinkInternal(localSaveDirectory.FullName);
            }
        }

        private void CreateScenarioDirectoryLinkInternal(string localScenarioPath)
        {
            var globalScenarioDirectory = new DirectoryInfo(App.Instance.GlobalScenarioPath);
            if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

            var localScenarioJunction = new JunctionInfo(localScenarioPath);
            localScenarioJunction.Create(globalScenarioDirectory.FullName);
        }

        /// <summary>
        /// Creates the directory junction for scenarios.
        /// </summary>
        public void CreateScenarioDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "scenarios"));
                if (!Junction.Exists(localScenarioDirectory.FullName) && localScenarioDirectory.Exists) localScenarioDirectory.Delete(true);

                CreateScenarioDirectoryLinkInternal(localScenarioDirectory.FullName);
            }
        }

        private void CreateModDirectoryLinkInternal(string localModPath)
        {
            var globalModDirectory = App.Instance.Settings.GetModDirectory(Version);
            if (!globalModDirectory.Exists) globalModDirectory.Create();

            var localModJunction = new JunctionInfo(localModPath);
            localModJunction.Create(globalModDirectory.FullName);
        }

        /// <summary>
        /// Creates the directory junction for mods.
        /// </summary>
        public void CreateModDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(linkDirectory.FullName, "mods"));
                if (!Junction.Exists(localModDirectory.FullName) && localModDirectory.Exists) localModDirectory.Delete(true);

                CreateModDirectoryLinkInternal(localModDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates all directory junctions.
        /// </summary>
        public void CreateLinks()
        {
            if (!IsSpecialVersion)
            {
                CreateSaveDirectoryLink();
                CreateScenarioDirectoryLink();
                CreateModDirectoryLink();
            }
        }

        /// <summary>
        /// Deletes all directory junctions.
        /// </summary>
        public void DeleteLinks()
        {
            if (!IsSpecialVersion)
            {
                JunctionInfo localSaveJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "saves"));
                if (localSaveJunction.Exists) localSaveJunction.Delete();

                JunctionInfo localScenarioJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "scenarios"));
                if (localScenarioJunction.Exists) localScenarioJunction.Delete();

                JunctionInfo localModJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "mods"));
                if (localModJunction.Exists) localModJunction.Delete();
            }
        }
    }
}
