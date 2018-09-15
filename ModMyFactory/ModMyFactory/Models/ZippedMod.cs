using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        /// <summary>
        /// A mod that is contained in a ZIP file.
        /// </summary>
        private sealed class ZippedMod : Mod
        {
            /// <summary>
            /// The mods file.
            /// </summary>
            public FileInfo File { get; private set; }

            public override bool ExtractUpdates => false;

            /// <summary>
            /// Creates a mod.
            /// </summary>
            public ZippedMod(FileInfo file, InfoFile infoFile, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
                : base(infoFile, parentCollection, modpackCollection)
            {
                File = file;
            }

            protected override void DeleteFile()
            {
                if (File.Exists) File.Delete();
            }

            public override async Task MoveTo(DirectoryInfo destinationDirectory, bool copy)
            {
                string destination = Path.Combine(destinationDirectory.FullName, $"{Name}_{Version}.zip");
                if (copy)
                {
                    await Task.Run(() => File.CopyTo(destination));
                    File = new FileInfo(destination);
                }
                else
                {
                    await File.MoveToAsync(destination);
                }
            }

            public override bool AlwaysKeepOnUpdate()
            {
                return App.Instance.Settings.KeepOldZippedModVersions;
            }
        }
    }
}
