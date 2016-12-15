using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;
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
        public FileInfo File { get; private set; }

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
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="file">The mods file.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        public ZippedMod(string name, Version factorioVersion, FileInfo file, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
            : base(name, factorioVersion, parentCollection, modpackCollection, messageOwner)
        {
            File = file;

            SetInfo(File);
        }

        protected override void DeleteFilesystemObjects()
        {
            File.Delete();
        }

        /// <summary>
        /// Updates this mod.
        /// </summary>
        /// <param name="newFile">The updated mod file.</param>
        public void Update(FileInfo newFile)
        {
            File.Delete();
            File = newFile;
            SetInfo(File);
        }

        public override async Task MoveTo(DirectoryInfo destinationDirectory)
        {
            await File.MoveToAsync(Path.Combine(destinationDirectory.FullName, File.Name));
        }
    }
}
