using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.ViewModels;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A mod.
    /// </summary>
    abstract class Mod : NotifyPropertyChangedBase
    {
        private static void AddToDictionary(Dictionary<string, List<Mod>> modDictionary, Mod mod)
        {
            List<Mod> list;
            if (!modDictionary.TryGetValue(mod.Name, out list))
            {
                list = new List<Mod>();
                modDictionary.Add(mod.Name, list);
            }

            list.Add(mod);
        }

        private static Dictionary<string, List<Mod>> CreateModDictionary(ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            var modDictionary = new Dictionary<string, List<Mod>>();

            var modDirectory = App.Instance.Settings.GetModDirectory();
            if (modDirectory.Exists)
            {
                foreach (var directory in modDirectory.EnumerateDirectories())
                {
                    Version factorioDirVersion;
                    if (Version.TryParse(directory.Name, out factorioDirVersion))
                    {
                        // Zipped mods
                        foreach (var file in directory.EnumerateFiles("*.zip"))
                        {
                            Version factorioVersion;
                            string name;
                            Version version;
                            if (ArchiveFileValid(file, out factorioVersion, out name, out version))
                            {
                                if (factorioVersion == factorioDirVersion)
                                {
                                    var mod = new ZippedMod(name, version, factorioVersion, file, parentCollection, modpackCollection);
                                    AddToDictionary(modDictionary, mod);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"The mod {name}_{version}.zip is targeting Factorio version {factorioVersion} but resides in directory {factorioDirVersion}.");
                                }
                            }
                        }

                        // Extracted mods
                        foreach (var subDirectory in directory.EnumerateDirectories())
                        {
                            Version factorioVersion;
                            string name;
                            Version version;
                            if (DirectoryValid(subDirectory, out factorioVersion, out name, out version))
                            {
                                if (factorioVersion == factorioDirVersion)
                                {
                                    var mod = new ExtractedMod(name, version, factorioVersion, subDirectory, parentCollection, modpackCollection);
                                    AddToDictionary(modDictionary, mod);
                                }
                                else
                                {
                                    throw new InvalidOperationException($"The mod {name}_{version} is targeting Factorio version {factorioVersion} but resides in directory {factorioDirVersion}.");
                                }
                            }
                        }
                    }
                }
            }

            return modDictionary;
        }

        private static void PopulateModCollection(Dictionary<string, List<Mod>> modDictionary, ICollection<Mod> modCollection)
        {
            foreach (List<Mod> list in modDictionary.Values)
            {
                list.Sort((a, b) =>
                {
                    int result = a.FactorioVersion.CompareTo(b.FactorioVersion);
                    if (result == 0) result = a.Version.CompareTo(b.Version);
                    return result;
                });

                Mod currentMod = list[0];
                modCollection.Add(currentMod);
                for (int i = 1; i < list.Count; i++)
                {
                    Mod nextMod = list[i];

                    if ((App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion) && (nextMod.FactorioVersion != currentMod.FactorioVersion))
                    {
                        modCollection.Add(nextMod);
                        currentMod = nextMod;
                    }
                    else
                    {
                        currentMod.OldVersion = nextMod;
                        currentMod = nextMod;
                    }
                }
            }
        }

        /// <summary>
        /// Loads all mods from the selected mod directory to the specified parent collection.
        /// </summary>
        /// <param name="parentCollection">The collection to contain the mods.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public static void LoadMods(ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            var modDictionary = CreateModDictionary(parentCollection, modpackCollection);
            PopulateModCollection(modDictionary, parentCollection);
        }

        private static bool TryParseModName(string fileName, out string name, out Version version)
        {
            name = null;
            version = null;

            int index = fileName.LastIndexOf('_');
            if ((index < 1) || (index >= fileName.Length - 1)) return false;

            name = fileName.Substring(0, index);
            string versionString = fileName.Substring(index + 1);
            return Version.TryParse(versionString, out version);
        }

        private static bool TryParseInfoFile(Stream stream, out Version factorioVersion, out string name, out Version version)
        {
            factorioVersion = null;
            name = null;
            version = null;

            using (var reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();

                // Factorio version
                MatchCollection matches = Regex.Matches(content, "\"factorio_version\" *: *\"(?<factorio_version>[0-9]+\\.[0-9]+(\\.[0-9]+)?)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count == 0) return false;

                string factorioVersionString = matches[matches.Count - 1].Groups["factorio_version"].Value;
                factorioVersion = Version.Parse(factorioVersionString);
                factorioVersion = new Version(factorioVersion.Major, factorioVersion.Minor);

                // Version
                matches = Regex.Matches(content, "\"version\" *: *\"(?<version>[0-9]+(\\.[0-9]+){0,3})\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count == 0) return false;

                string versionString = matches[matches.Count - 1].Groups["version"].Value;
                version = Version.Parse(versionString);

                // Name
                matches = Regex.Matches(content, "\"name\" *: *\"(?<name>.*)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count == 0) return false;

                name = matches[matches.Count - 1].Groups["name"].Value;

                return true;
            }
        }

        /// <summary>
        /// Checks if an archive file contains a valid mod.
        /// </summary>
        /// <param name="archiveFile">The archive file to check.</param>
        /// <param name="validFactorioVersion">Out. The version of Factorio the mod contained in the archive file is targeting.</param>
        /// <param name="validName">Out. The name of the mod contained in the archive file.</param>
        /// <param name="validVersion">Out. The version of the mod contained in the archive file.</param>
        /// <returns>Returns true if the specified archive file contains a valid mod, otherwise false.</returns>
        public static bool ArchiveFileValid(FileInfo archiveFile, out Version validFactorioVersion, out string validName, out Version validVersion)
        {
            validFactorioVersion = null;
            validName = null;
            validVersion = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(archiveFile.NameWithoutExtension(), out fileName, out fileVersion)) return false;

            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name == "info.json")
                    {
                        using (Stream stream = entry.Open())
                        {
                            if (TryParseInfoFile(stream, out validFactorioVersion, out validName, out validVersion))
                                return (validName == fileName) && (validVersion == fileVersion);
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a directory contains a valid mod.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="validFactorioVersion">Out. The version of Factorio the mod contained in the archive file is targeting.</param>
        /// <param name="validName">Out. The name of the mod contained in the directory.</param>
        /// <param name="validVersion">Out. The version of the mod contained in the archive file.</param>
        /// <returns>Returns true if the specified directory contains a valid mod, otherwise false.</returns>
        public static bool DirectoryValid(DirectoryInfo directory, out Version validFactorioVersion, out string validName, out Version validVersion)
        {
            validFactorioVersion = null;
            validName = null;
            validVersion = null;

            string fileName;
            Version fileVersion;
            if (!TryParseModName(directory.Name, out fileName, out fileVersion)) return false;

            var file = directory.EnumerateFiles("info.json").FirstOrDefault();
            if (file != null)
            {
                using (Stream stream = file.OpenRead())
                {
                    if (TryParseInfoFile(stream, out validFactorioVersion, out validName, out validVersion))
                        return (validName == fileName) && (validVersion == fileVersion);
                }
            }

            return false;
        }


        private readonly ICollection<Mod> parentCollection;
        private readonly ICollection<Modpack> modpackCollection;

        string title;
        string description;
        string author;
        Version version;
        bool active;
        bool isSelected;

        /// <summary>
        /// The title of the mod.
        /// </summary>
        public string Title
        {
            get { return title; }
            private set
            {
                if (value != title)
                {
                    title = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description
        {
            get { return description; }
            private set
            {
                if (value != description)
                {
                    description = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Description)));

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ToolTip)));
                }
            }
        }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author
        {
            get { return author; }
            private set
            {
                if (value != author)
                {
                    author = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Author)));

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ToolTip)));
                }
            }
        }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public Version Version
        {
            get { return version; }
            protected set
            {
                if (value != version)
                {
                    version = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Version)));

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ToolTip)));
                }
            }
        }

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        public Version FactorioVersion { get; }

        /// <summary>
        /// Indicates whether the mod is currently active.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));

                    ModManager.SetActive(Name, FactorioVersion, value);
                    ModManager.SaveTemplates();
                }
            }
        }

        /// <summary>
        /// Additional information about this mod to be displayed in a tooltip.
        /// </summary>
        public string ToolTip
        {
            get
            {
                var authorAndVersion = $"Author: {Author}     Version: {Version}";
                return $"{authorAndVersion}\n\n{Description.Wrap(authorAndVersion.Length)}";
            }
        }

        /// <summary>
        /// Indicates whether this mod is selected in the list.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// An optinal older version of this mod that is also present.
        /// </summary>
        protected Mod OldVersion { get; private set; }

        /// <summary>
        /// A command that deletes this mod from the list and the filesystem.
        /// </summary>
        public RelayCommand<bool?> DeleteCommand { get; }

        /// <summary>
        /// Deletes this mod at file system level.
        /// </summary>
        protected abstract void DeleteFilesystemObjects();

        /// <summary>
        /// Deletes this mod from the list and the filesystem.
        /// </summary>
        /// <param name="showPrompt">Indicates whether a confirmation prompt is shown to the user.</param>
        public void Delete(bool showPrompt)
        {
            if (!showPrompt || (MessageBox.Show(
                App.Instance.GetLocalizedMessage("DeleteMod", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("DeleteMod", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
            {
                foreach (var modpack in modpackCollection)
                {
                    ModReference reference;
                    if (modpack.Contains(this, out reference))
                        modpack.Mods.Remove(reference);

                }

                DeleteFilesystemObjects();
                parentCollection.Remove(this);

                if (OldVersion != null)
                {
                    parentCollection.Add(OldVersion);
                }
                else
                {
                    ModManager.RemoveTemplate(Name);
                }

                ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                ModpackTemplateList.Instance.Save();
            }
        }

        /// <summary>
        /// Reads a provided info.json file to populate the mods attributes.
        /// All derived classes should call this method in their constructor and when updated.
        /// </summary>
        /// <param name="stream">A stream containing the contents of the info.json file.</param>
        protected void ReadInfoFile(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string content = reader.ReadToEnd();

                // Title
                MatchCollection matches = Regex.Matches(content, "\"title\" *: *\"(?<title>.*)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                Title = matches.Count > 0 ? matches[0].Groups["title"].Value : Name;

                // Description
                matches = Regex.Matches(content, "\"description\" *: *\"(?<description>.*)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    Description = matches[0].Groups["description"].Value.Replace("\\n", "\n");
                }

                // Author
                matches = Regex.Matches(content, "\"author\" *: *\"(?<author>.*)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    Author = matches[0].Groups["author"].Value;
                }
            }
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <param name="version">The mods version.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        protected Mod(string name, Version version, Version factorioVersion, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            Name = name;
            Version = version;
            FactorioVersion = factorioVersion;
            active = ModManager.GetActive(Name, FactorioVersion);

            this.parentCollection = parentCollection;
            this.modpackCollection = modpackCollection;

            DeleteCommand = new RelayCommand<bool?>(showPrompt => Delete(showPrompt ?? true));
        }

        /// <summary>
        /// Updates this mod to a provided new version.
        /// </summary>
        /// <param name="newVersion">The new version this mod is getting updated to.</param>
        public void Update(Mod newVersion)
        {
            if (App.Instance.Settings.KeepOldModVersions)
            {
                newVersion.OldVersion = this;
            }
            else
            {
                DeleteFilesystemObjects();
            }

            parentCollection.Remove(this);
        }

        /// <summary>
        /// Moves this mod to a specified directory.
        /// </summary>
        public abstract Task MoveTo(DirectoryInfo destinationDirectory);
    }
}
