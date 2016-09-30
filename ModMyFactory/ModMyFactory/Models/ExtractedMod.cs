using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A mod that has been extracted to a directory.
    /// </summary>
    sealed class ExtractedMod : Mod
    {
        /// <summary>
        /// The mods directory.
        /// </summary>
        public DirectoryInfo Directory { get; }

        protected override string FallbackName => Directory.Name;

        private FileInfo GetInfoFileRecursive(DirectoryInfo directory)
        {
            var file = directory.EnumerateFiles("info.json").FirstOrDefault();
            if (file != null) return file;

            foreach (var subDirectory in directory.EnumerateDirectories())
            {
                file = GetInfoFileRecursive(subDirectory);
                if (file != null) return file;
            }

            return null;
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="directory">The mods directory.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        public ExtractedMod(DirectoryInfo directory, Version factorioVersion, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
            : base(factorioVersion, parentCollection, modpackCollection, messageOwner)
        {
            Directory = directory;

            FileInfo infoFile = GetInfoFileRecursive(Directory);
            using (Stream stream = infoFile.OpenRead())
            {
                base.ReadInfoFile(stream);
            }
        }

        protected override void DeleteFilesystemObjects()
        {
            Directory.Delete(true);
        }
    }
}
