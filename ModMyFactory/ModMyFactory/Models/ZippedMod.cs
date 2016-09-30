using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;

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

        protected override string FallbackName => Path.GetFileNameWithoutExtension(File.Name);

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="file">The mods file.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        /// <param name="parentCollection">The collection containing this mod.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        /// <param name="messageOwner">The window that ownes the deletion message box.</param>
        public ZippedMod(FileInfo file, Version factorioVersion, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection, Window messageOwner)
            : base(factorioVersion, parentCollection, modpackCollection, messageOwner)
        {
            File = file;

            using (ZipArchive archive = ZipFile.OpenRead(File.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("info.json"))
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

        protected override void DeleteFilesystemObjects()
        {
            File.Delete();
        }
    }
}
