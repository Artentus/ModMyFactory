using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    /// <summary>
    /// A mod.
    /// </summary>
    class Mod : NotifyPropertyChangedBase
    {
        static List<ModTemplateList> templateLists;

        /// <summary>
        /// Loads all available mod templates.
        /// </summary>
        public static void LoadTemplates()
        {
            templateLists = new List<ModTemplateList>();

            var modDirectory = App.Instance.Settings.GetModDirectory();
            if (modDirectory.Exists)
            {
                foreach (var directory in modDirectory.EnumerateDirectories())
                {
                    Version version;
                    if (Version.TryParse(directory.Name, out version))
                    {
                        var templateList = ModTemplateList.Load(Path.Combine(directory.FullName, "mod-list.json"));
                        templateList.Version = version;
                        templateLists.Add(templateList);
                    }
                }
            }
        }

        private static bool Contains(Version version, out ModTemplateList list)
        {
            list = templateLists.Find(l => l.Version == version);
            return list != null;
        }

        private static bool GetActive(string name, Version version)
        {
            ModTemplateList list;
            if (Contains(version, out list))
            {
                return list.GetActive(name);
            }
            else
            {
                list = ModTemplateList.Load(Path.Combine(App.Instance.Settings.GetModDirectory(version).FullName, "mod-list.json"));
                list.Version = version;
                templateLists.Add(list);

                return list.GetActive(name);
            }
        }

        private static void SetActive(string name, Version version, bool value)
        {
            ModTemplateList list;
            if (Contains(version, out list))
            {
                list.SetActive(name, value);
            }
            else
            {
                list = ModTemplateList.Load(Path.Combine(App.Instance.Settings.GetModDirectory(version).FullName, "mod-list.json"));
                list.Version = version;
                templateLists.Add(list);

                list.SetActive(name, value);
            }
        }

        /// <summary>
        /// Starts updating all mod templates.
        /// While updating all save commands will be ignored.
        /// </summary>
        public static void BeginUpdateTemplates()
        {
            templateLists.ForEach(list => list.BeginUpdate());
        }

        /// <summary>
        /// Finishes updating all mod templates.
        /// </summary>
        /// <param name="force">If true forces to end updating, otherwise EndUpdate will have to be called as many times as BeginUpdate has been called.</param>
        public static void EndUpdateTemplates(bool force = false)
        {
            templateLists.ForEach(list => list.EndUpdate(force));
        }

        /// <summary>
        /// Saves all mod templates to their files.
        /// </summary>
        public static void SaveTemplates()
        {
            templateLists.ForEach(list => list.Save());
        }

        /// <summary>
        /// Loads all mods from the selected mod directory to the specified parent collection.
        /// </summary>
        /// <param name="parentCollection">The collection to contain the mods.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        public static void LoadMods(ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
        {
            var modDirectory = App.Instance.Settings.GetModDirectory();
            if (modDirectory.Exists)
            {
                foreach (var directory in modDirectory.EnumerateDirectories())
                {
                    Version version;
                    if (Version.TryParse(directory.Name, out version))
                    {
                        foreach (var file in directory.EnumerateFiles("*.zip"))
                        {
                            var mod = new Mod(file, version, parentCollection, modpackCollection, messageOwner);
                            parentCollection.Add(mod);
                        }
                    }
                }
            }
        }

        readonly string innerName;
        bool active;

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The mod file.
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        public Version FactorioVersion { get; }

        /// <summary>
        /// A command that deletes this mod from the list and the filesystem.
        /// </summary>
        public RelayCommand DeleteCommand { get; }

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

                    Mod.SetActive(innerName, FactorioVersion, value);
                    Mod.SaveTemplates();
                }
            }
        }

        private void Delete(ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
        {
            if (MessageBox.Show(messageOwner, "Do you really want to delete this mod?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var modpack in modpackCollection)
                {
                    ModReference reference;
                    if (modpack.Contains(this, out reference))
                        modpack.Mods.Remove(reference);

                }
                File.Delete();
                parentCollection.Remove(this);

                MainViewModel.Instance.ModpackTemplateList.Update(MainViewModel.Instance.Modpacks);
                MainViewModel.Instance.ModpackTemplateList.Save();
            }
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="file">The mod file.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        public Mod(FileInfo file, Version factorioVersion, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
        {
            File = file;
            FactorioVersion = factorioVersion;

            using (ZipArchive archive = ZipFile.OpenRead(File.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("info.json"))
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string content = reader.ReadToEnd();

                                // Name
                                MatchCollection matches = Regex.Matches(content, "\"name\" *: *\"(?<name>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                innerName = matches.Count > 0 ? matches[0].Groups["name"].Value : File.Name;
                                matches = Regex.Matches(content, "\"title\" *: *\"(?<title>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                Name = matches.Count > 0 ? matches[0].Groups["title"].Value : innerName;

                                // Description
                                matches = Regex.Matches(content, "\"description\" *: *\"(?<description>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    Description = matches[0].Groups["description"].Value;
                                }

                                // Author
                                matches = Regex.Matches(content, "\"author\" *: *\"(?<author>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    Author = matches[0].Groups["author"].Value;
                                }

                                // Version
                                matches = Regex.Matches(content, "\"version\" *: *\"(?<version>[0-9]+(\\.[0-9]+){0,3})\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    string versionString = matches[0].Groups["version"].Value;
                                    Version = Version.Parse(versionString);
                                }
                                else
                                {
                                    Version = new Version(1, 0);
                                }
                            }
                        }

                        break;
                    }
                }
            }

            DeleteCommand = new RelayCommand(() => Delete(parentCollection, modpackCollection, messageOwner));

            active = Mod.GetActive(innerName, FactorioVersion);
        }
    }
}
