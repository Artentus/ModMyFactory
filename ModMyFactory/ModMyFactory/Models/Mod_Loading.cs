using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        private static string GetNameWithoutUid(string name)
        {
            int index = name.IndexOf('+');
            return name.Substring(index + 1);
        }

        private static bool TryParseModName(string fileName, out string name, out Version version, bool hasUid)
        {
            name = null;
            version = null;

            int index = fileName.LastIndexOf('_');
            if ((index < 1) || (index >= fileName.Length - 1)) return false;

            name = fileName.Substring(0, index);
            if (hasUid) name = GetNameWithoutUid(name);
            string versionString = fileName.Substring(index + 1);
            return Version.TryParse(versionString, out version);
        }

        private static bool TryReadInfoFile(Stream stream, out InfoFile infoFile)
        {
            infoFile = null;

            using (var reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();
                try
                {
                    infoFile = InfoFile.FromString(content);
                }
                catch
                {
                    return false;
                }

                return infoFile.IsValid;
            }
        }

        private static bool TryReadInfoFileFromArchive(FileInfo archiveFile, out InfoFile infoFile)
        {
            infoFile = null;

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name == "info.json")
                        {
                            using (Stream stream = entry.Open())
                            {
                                return TryReadInfoFile(stream, out infoFile) && infoFile.IsValid;
                            }
                        }
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                App.Instance.WriteExceptionLog(ex);
            }

            return false;
        }

        private static bool TryReadInfoFileFromDirectory(DirectoryInfo directory, out InfoFile infoFile)
        {
            infoFile = null;

            var file = directory.EnumerateFiles("info.json").FirstOrDefault();
            if (file != null)
            {
                using (Stream stream = file.OpenRead())
                {
                    return TryReadInfoFile(stream, out infoFile) && infoFile.IsValid;
                }
            }

            return false;
        }
        
        private static bool ArchiveFileValid(FileInfo archiveFile, out InfoFile infoFile, bool hasUid)
        {
            infoFile = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(archiveFile.NameWithoutExtension(), out fileName, out fileVersion, hasUid)) return false;

            if (TryReadInfoFileFromArchive(archiveFile, out infoFile))
            {
                return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
            }
            else
            {
                return false;
            }
        }
        
        private static bool DirectoryValid(DirectoryInfo directory, out InfoFile infoFile, bool hasUid)
        {
            infoFile = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(directory.Name, out fileName, out fileVersion, hasUid)) return false;

            if (TryReadInfoFileFromDirectory(directory, out infoFile))
            {
                return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
            }
            else
            {
                return false;
            }
        }

        private static bool TryLoad(FileInfo archiveFile, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, out ZippedMod mod)
        {
            if (ArchiveFileValid(archiveFile, out InfoFile infoFile, false))
            {
                mod = new ZippedMod(archiveFile, infoFile, parentCollection, modpackCollection);
                return true;
            }
            else
            {
                mod = null;
                return false;
            }
        }

        private static bool TryLoad(DirectoryInfo directory, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, out ExtractedMod mod)
        {
            if (DirectoryValid(directory, out InfoFile infoFile, false))
            {
                mod = new ExtractedMod(directory, infoFile, parentCollection, modpackCollection);
                return true;
            }
            else
            {
                mod = null;
                return false;
            }
        }

        private static bool TryLoad(FileSystemInfo fileOrDirectory, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, out Mod mod)
        {
            FileInfo file = fileOrDirectory as FileInfo;
            if (file != null)
            {
                bool result = TryLoad(file, parentCollection, modpackCollection, out ZippedMod zippedMod);
                mod = zippedMod;
                return result;
            }

            DirectoryInfo directory = fileOrDirectory as DirectoryInfo;
            if (directory != null)
            {
                bool result = TryLoad(directory, parentCollection, modpackCollection, out ExtractedMod extractedMod);
                mod = extractedMod;
                return result;
            }

            throw new ArgumentException("Only files and directories are supported.", nameof(fileOrDirectory));
        }

        public static async Task<Mod> Add(FileInfo archiveFile, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false)
        {
            if (ArchiveFileValid(archiveFile, out InfoFile infoFile, hasUid))
            {
                if (parentCollection.ContainsByFactorioVersion(infoFile.Name, infoFile.FactorioVersion))
                {
                    switch (App.Instance.Settings.ManagerMode)
                    {
                        case ManagerMode.PerFactorioVersion:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), infoFile.Title, infoFile.FactorioVersion),
                                App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case ManagerMode.Global:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), infoFile.Title),
                                App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }

                    return null;
                }

                Mod mod = new ZippedMod(archiveFile, infoFile, parentCollection, modpackCollection);

                var modDirectory = App.Instance.Settings.GetModDirectory(infoFile.FactorioVersion);
                if (!modDirectory.Exists) modDirectory.Create();
                await mod.MoveTo(modDirectory, copy);

                return mod;
            }
            else
            {
                MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), archiveFile.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<Mod> Add(DirectoryInfo directory, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false)
        {
            if (DirectoryValid(directory, out InfoFile infoFile, hasUid))
            {
                if (parentCollection.ContainsByFactorioVersion(infoFile.Name, infoFile.FactorioVersion))
                {
                    switch (App.Instance.Settings.ManagerMode)
                    {
                        case ManagerMode.PerFactorioVersion:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), infoFile.Title, infoFile.FactorioVersion),
                                App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case ManagerMode.Global:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), infoFile.Title),
                                App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }

                    return null;
                }

                Mod mod = new ExtractedMod(directory, infoFile, parentCollection, modpackCollection);

                var modDirectory = App.Instance.Settings.GetModDirectory(infoFile.FactorioVersion);
                if (!modDirectory.Exists) modDirectory.Create();
                await mod.MoveTo(modDirectory, copy);

                return mod;
            }
            else
            {
                MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), directory.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<Mod> Add(FileSystemInfo fileOrDirectory, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false)
        {
            FileInfo file = fileOrDirectory as FileInfo;
            if (file != null) return await Add(file, parentCollection, modpackCollection, copy, hasUid);

            DirectoryInfo directory = fileOrDirectory as DirectoryInfo;
            if (directory != null) return await Add(directory, parentCollection, modpackCollection, copy, hasUid);

            throw new ArgumentException("Only files and directories are supported.", nameof(fileOrDirectory));
        }




        protected sealed class ModFile
        {
            public Version Version { get; }

            public FileSystemInfo File { get; }

            public bool Exists => File.Exists;

            public void Delete()
            {
                File.DeleteRecursive();
            }

            public ModFile(Version version, FileSystemInfo file)
            {
                Version = version;
                File = File;
            }
        }

        private static void AddFileToDictionary(Dictionary<string, List<ModFile>> dictionary, FileSystemInfo file)
        {
            if (TryParseModName(file.NameWithoutExtension(), out string modName, out Version modVersion, false))
            {
                List<ModFile> list;
                if (!dictionary.TryGetValue(modName, out list))
                {
                    list = new List<ModFile>();
                    dictionary.Add(modName, list);
                }

                list.Add(new ModFile(modVersion, file));
            }
        }

        private static Dictionary<string, List<ModFile>> CreateFileDictionary(params DirectoryInfo[] directories)
        {
            var dictionary = new Dictionary<string, List<ModFile>>();

            foreach (var directory in directories)
            {
                foreach (var file in directory.EnumerateFileSystemInfos())
                    AddFileToDictionary(dictionary, file);
            }

            return dictionary;
        }

        private static void LoadModFromFileList(List<ModFile>  modFileList, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            for (int i = modFileList.Count - 1; i >= 0; i--)
            {
                var modFile = modFileList[i].File;
                if (Mod.TryLoad(modFile, parentCollection, modpackCollection, out Mod mod))
                {
                    modFileList.RemoveAt(i);
                    mod.OldVersions = modFileList;
                    parentCollection.Add(mod);
                    return;
                }
            }
        }

        private static void LoadModsFromFileDictionary(Dictionary<string, List<ModFile>> fileDictionary, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            foreach (var modFileList in fileDictionary.Select(kvp => kvp.Value))
            {
                modFileList.Sort((a, b) => b.Version.CompareTo(a.Version));
                LoadModFromFileList(modFileList, parentCollection, modpackCollection);
            }
        }

        /// <summary>
        /// Loads all mods from the selected mod directory into the specified parent collection.
        /// </summary>
        /// <param name="parentCollection">The collection to contain the mods.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public static void LoadMods(ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            var modDirectory = App.Instance.Settings.GetModDirectory();

            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                foreach (var subDirectory in modDirectory.EnumerateDirectories())
                {
                    if (Version.TryParse(subDirectory.Name, out var factorioVersion))
                    {
                        var fileDictionary = CreateFileDictionary(subDirectory);
                        LoadModsFromFileDictionary(fileDictionary, parentCollection, modpackCollection);
                    }
                }
            }
            else
            {
                var selectedDirectories = new List<DirectoryInfo>();
                foreach (var subDirectory in modDirectory.EnumerateDirectories())
                {
                    if (Version.TryParse(subDirectory.Name, out var factorioVersion))
                        selectedDirectories.Add(subDirectory);
                }

                var fileDictionary = CreateFileDictionary(selectedDirectories.ToArray());
                LoadModsFromFileDictionary(fileDictionary, parentCollection, modpackCollection);
            }
        }
    }
}
