using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A mod that is contained in a ZIP file.
    /// </summary>
    sealed class ZippedMod : Mod
    {
        /// <summary>
        /// The mods file.
        /// </summary>
        public FileInfo File { get; }

        private void SetInfo(FileInfo archiveFile)
        {
            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name == "info.json")
                    {
                        using (Stream stream = entry.Open())
                        {
                            base.ReadInfoFile(stream);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <param name="version">The mods version.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="file">The mods file.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public ZippedMod(string name, Version version, Version factorioVersion, FileInfo file, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
            : base(name, version, factorioVersion, parentCollection, modpackCollection)
        {
            File = file;

            SetInfo(File);
        }

        public override void DeleteFilesystemObjects()
        {
            if (File.Exists) File.Delete();
        }

        public override async Task MoveTo(DirectoryInfo destinationDirectory)
        {
            await File.MoveToAsync(Path.Combine(destinationDirectory.FullName, File.Name));
        }

        public override bool AlwaysKeepOnUpdate()
        {
            return App.Instance.Settings.KeepOldZippedModVersions;
        }
    }
}
