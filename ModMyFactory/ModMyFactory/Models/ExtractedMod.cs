using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        /// <summary>
        /// A mod that has been extracted to a directory.
        /// </summary>
        private sealed class ExtractedMod : Mod
        {
            /// <summary>
            /// The mods directory.
            /// </summary>
            public DirectoryInfo Directory { get; private set; }

            public override bool ExtractUpdates => !App.Instance.Settings.AlwaysUpdateZipped;

            /// <summary>
            /// Creates a mod.
            /// </summary>
            public ExtractedMod(DirectoryInfo directory, InfoFile infoFile, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
                : base(infoFile, parentCollection, modpackCollection)
            {
                Directory = directory;
            }

            protected override void DeleteFile()
            {
                if (Directory.Exists) Directory.Delete(true);
            }

            public override async Task MoveTo(DirectoryInfo destinationDirectory, bool copy)
            {
                var newDirectory = new DirectoryInfo(Path.Combine(destinationDirectory.FullName, $"{Name}_{Version}.zip"));
                if (copy)
                {
                    await Directory.CopyToAsync(newDirectory.FullName);
                }
                else
                {
                    await Directory.MoveToAsync(newDirectory.FullName);
                }
                Directory = newDirectory;
            }

            public override async Task ExportFile(string destination, int uid = -1)
            {
                string dirName = Directory.Name;
                if (uid >= 0) dirName = $"{uid}+{dirName}";

                string newPath = Path.Combine(destination, dirName);
                await Directory.CopyToAsync(newPath);
            }

            public override bool AlwaysKeepOnUpdate()
            {
                return App.Instance.Settings.KeepOldExtractedModVersions;
            }
        }
    }
}
