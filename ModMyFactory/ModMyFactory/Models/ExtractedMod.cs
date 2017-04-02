using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModMyFactory.Helpers;

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
        public DirectoryInfo Directory { get; private set; }

        private void SetInfo(DirectoryInfo directory)
        {
            FileInfo infoFile = directory.EnumerateFiles("info.json").First();
            using (Stream stream = infoFile.OpenRead())
            {
                base.ReadInfoFile(stream);
            }
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <param name="version">The mods version.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="directory">The mods directory.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public ExtractedMod(string name, Version version, Version factorioVersion, DirectoryInfo directory, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
            : base(name, version, factorioVersion, parentCollection, modpackCollection)
        {
            Directory = directory;

            SetInfo(Directory);
        }

        protected override void DeleteFilesystemObjects()
        {
            Directory.Delete(true);
        }

        public override async Task MoveTo(DirectoryInfo destinationDirectory)
        {
            var newDirectory = new DirectoryInfo(Path.Combine(destinationDirectory.FullName, Directory.Name));
            await Directory.MoveToAsync(newDirectory.FullName);
            Directory = newDirectory;
        }
    }
}
