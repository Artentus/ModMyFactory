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
        /// <param name="is64Bit">Out. Specifies if the valid installation contains a 64 bit executable.</param>
        /// <returns>Returns true if the archive file contains a valid Factorio installation, otherwise false.</returns>
        public static bool ArchiveFileValid(FileInfo archiveFile, out Version validVersion, out bool is64Bit)
        {
            validVersion = null;
            is64Bit = false;

            bool hasValidVersion = false;
            bool hasValidPlatform = false;

            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!hasValidVersion && entry.FullName.EndsWith("data/base/info.json"))
                    {
                        using (Stream stream = entry.Open())
                        {
                            if (TryExtractVersion(stream, out validVersion)) hasValidVersion = true;
                        }
                    }
                    else if (!hasValidPlatform && entry.FullName.EndsWith("Win32/factorio.exe"))
                    {
                        hasValidPlatform = true;
                        is64Bit = true;
                    }
                    else if (!hasValidPlatform && entry.FullName.EndsWith("x64/factorio.exe"))
                    {
                        hasValidPlatform = true;
                        is64Bit = false;
                    }

                    if (hasValidVersion && hasValidPlatform) break;
                }
            }

            return hasValidVersion && hasValidPlatform;
        }

        /// <summary>
        /// Checks if a directory contains a valid installation of Factorio.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="validVersion">Out. The version of Factorio contained in the directory.</param>
        /// <param name="is64Bit">Out. Specifies if the valid installation contains a 64 bit executable.</param>
        /// <returns>Returns true if the directory contains a valid Factorio installation, otherwise false.</returns>
        public static bool LocalInstallationValid(DirectoryInfo directory, out Version validVersion, out bool is64Bit)
        {
            validVersion = null;
            is64Bit = false;

            FileInfo infoFile = new FileInfo(Path.Combine(directory.FullName, @"data\base\info.json"));
            if (infoFile.Exists)
            {
                using (Stream stream = infoFile.OpenRead())
                {
                    if (!TryExtractVersion(stream, out validVersion)) return false;
                }
            }
            else
            {
                return false;
            }

            DirectoryInfo win32Dir = new DirectoryInfo(Path.Combine(directory.FullName, @"bin\Win32"));
            DirectoryInfo win64Dir = new DirectoryInfo(Path.Combine(directory.FullName, @"bin\x64"));
            if (win32Dir.Exists)
            {
                is64Bit = false;
            }
            else if (win64Dir.Exists)
            {
                is64Bit = true;
            }
            else
            {
                return false;
            }

            return true;
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
            string osPlatform = Environment.Is64BitOperatingSystem ? "x64" : "Win32";
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
            if (localSaveJunction.Exists)
            {
                localSaveJunction.SetDestination(globalSaveDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localSaveJunction.FullName))
                    System.IO.Directory.Delete(localSaveJunction.FullName, true);

                localSaveJunction.Create(globalSaveDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for saves.
        /// </summary>
        public void CreateSaveDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                string localSavePath = Path.Combine(linkDirectory.FullName, "saves");
                CreateSaveDirectoryLinkInternal(localSavePath);
            }
        }

        private void CreateScenarioDirectoryLinkInternal(string localScenarioPath)
        {
            var globalScenarioDirectory = new DirectoryInfo(App.Instance.GlobalScenarioPath);
            if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

            var localScenarioJunction = new JunctionInfo(localScenarioPath);
            if (localScenarioJunction.Exists)
            {
                localScenarioJunction.SetDestination(globalScenarioDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localScenarioJunction.FullName))
                    System.IO.Directory.Delete(localScenarioJunction.FullName, true);

                localScenarioJunction.Create(globalScenarioDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for scenarios.
        /// </summary>
        public void CreateScenarioDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                string localScenarioPath = Path.Combine(linkDirectory.FullName, "scenarios");
                CreateScenarioDirectoryLinkInternal(localScenarioPath);
            }
        }

        private void CreateModDirectoryLinkInternal(string localModPath)
        {
            var globalModDirectory = App.Instance.Settings.GetModDirectory(Version);
            if (!globalModDirectory.Exists) globalModDirectory.Create();

            var localModJunction = new JunctionInfo(localModPath);
            if (localModJunction.Exists)
            {
                localModJunction.SetDestination(globalModDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localModJunction.FullName))
                    System.IO.Directory.Delete(localModJunction.FullName, true);

                localModJunction.Create(globalModDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for mods.
        /// </summary>
        public void CreateModDirectoryLink()
        {
            if (!IsSpecialVersion)
            {
                string localModPath = Path.Combine(linkDirectory.FullName, "mods");
                CreateModDirectoryLinkInternal(localModPath);
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
