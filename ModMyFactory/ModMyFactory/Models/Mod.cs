using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ModMyFactory.MVVM;
using ModMyFactory.Helpers;
using ModMyFactory.ViewModels;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A mod.
    /// </summary>
    abstract class Mod : NotifyPropertyChangedBase
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

        /// <summary>
        /// Checks if a collection contains a mod.
        /// </summary>
        /// <param name="collection">The collection to be searched.</param>
        /// <param name="name">The name of the mod.</param>
        /// <param name="version">The mods version.</param>
        /// <returns>Return true if the collection contains the mod, otherwise false.</returns>
        public static bool ContainedInCollection(ICollection<Mod> collection, string name, Version version)
        {
            return collection.Any(mod =>
                string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (mod.Version == version));
        }

        /// <summary>
        /// Checks if a collection contains a mod.
        /// </summary>
        /// <param name="collection">The collection to be searched.</param>
        /// <param name="name">The name of the mod.</param>
        /// <param name="factorioVersion">The mods Factorio version.</param>
        /// <returns>Return true if the collection contains the mod, otherwise false.</returns>
        public static bool ContainedInCollectionByFactorioVersion(ICollection<Mod> collection, string name, Version factorioVersion)
        {
            return collection.Any(mod =>
                string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (mod.FactorioVersion == factorioVersion));
        }

        /// <summary>
        /// Finds a mod in a collection.
        /// </summary>
        /// <param name="collection">The collection to be searched.</param>
        /// <param name="name">The name of the mod.</param>
        /// <param name="factorioVersion">The mods Factorio version.</param>
        /// <returns>Returns the mod searched for.</returns>
        public static Mod FindInCollection(ICollection<Mod> collection, string name, Version factorioVersion)
        {
            return collection.First(mod =>
                string.Equals(mod.Name, name, StringComparison.InvariantCultureIgnoreCase)
                && (mod.FactorioVersion == factorioVersion));
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

        private static void RemoveTemplate(string name)
        {
            templateLists.ForEach(list => list.Remove(name));
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
                            string name = Path.GetFileNameWithoutExtension(file.Name).Split('_')[0];
                            var mod = new ZippedMod(name, version, file, parentCollection, modpackCollection, messageOwner);
                            parentCollection.Add(mod);
                        }

                        foreach (var subDirectory in directory.EnumerateDirectories())
                        {
                            string name = subDirectory.Name.Split('_')[0];
                            var mod = new ExtractedMod(name, version, subDirectory, parentCollection, modpackCollection, messageOwner);
                            parentCollection.Add(mod);
                        }
                    }
                }
            }
        }

        string title;
        string description;
        string author;
        Version version;
        bool active;

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
            private set
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

                    Mod.SetActive(Name, FactorioVersion, value);
                    Mod.SaveTemplates();
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
                return $"{authorAndVersion}\n\n{StringHelper.Wrap(Description, authorAndVersion.Length)}";
            }
        }

        /// <summary>
        /// A command that deletes this mod from the list and the filesystem.
        /// </summary>
        public RelayCommand DeleteCommand { get; }

        /// <summary>
        /// Deletes this mod at file system level.
        /// </summary>
        protected abstract void DeleteFilesystemObjects();

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
                DeleteFilesystemObjects();
                parentCollection.Remove(this);
                RemoveTemplate(Name);

                MainViewModel.Instance.ModpackTemplateList.Update(MainViewModel.Instance.Modpacks);
                MainViewModel.Instance.ModpackTemplateList.Save();
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

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        protected Mod(string name, Version factorioVersion, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
        {
            Name = name;
            FactorioVersion = factorioVersion;
            active = Mod.GetActive(Name, FactorioVersion);

            DeleteCommand = new RelayCommand(() => Delete(parentCollection, modpackCollection, messageOwner));
        }
    }
}
