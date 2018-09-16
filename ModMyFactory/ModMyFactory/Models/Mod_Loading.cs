using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        private static void AddFileToDictionary(Dictionary<string, ModFileCollection> dictionary, FileSystemInfo file)
        {
            if (ModFile.TryLoad(file, out var modFile))
            {
                ModFileCollection collection;
                if (!dictionary.TryGetValue(modFile.Name, out collection))
                {
                    collection = new ModFileCollection();
                    dictionary.Add(modFile.Name, collection);
                }

                collection.Add(modFile);
            }
        }

        private static Dictionary<string, ModFileCollection> CreateFileDictionary(params DirectoryInfo[] directories)
        {
            var dictionary = new Dictionary<string, ModFileCollection>();

            foreach (var directory in directories)
            {
                foreach (var file in directory.EnumerateFileSystemInfos())
                    AddFileToDictionary(dictionary, file);
            }

            return dictionary;
        }
        
        private static void LoadModsFromFileDictionary(Dictionary<string, ModFileCollection> fileDictionary, ModCollection parentCollection, ModpackCollection modpackCollection)
        {
            foreach (var modFileList in fileDictionary.Select(kvp => kvp.Value))
            {
                var mod = new Mod(modFileList, parentCollection, modpackCollection);
                parentCollection.Add(mod);
            }
        }

        /// <summary>
        /// Loads all mods from the selected mod directory into the specified parent collection.
        /// </summary>
        /// <param name="parentCollection">The collection to contain the mods.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public static void LoadMods(ModCollection parentCollection, ModpackCollection modpackCollection)
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
