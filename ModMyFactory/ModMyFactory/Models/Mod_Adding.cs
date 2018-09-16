using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        public static async Task<Mod> Add(FileInfo archiveFile, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false, bool showPrompts = true)
        {
            if (ArchiveFileValid(archiveFile, out InfoFile infoFile, hasUid))
            {
                if (parentCollection.ContainsByFactorioVersion(infoFile.Name, infoFile.FactorioVersion))
                {
                    switch (App.Instance.Settings.ManagerMode)
                    {
                        case ManagerMode.PerFactorioVersion:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), infoFile.FriendlyName, infoFile.FactorioVersion),
                                App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case ManagerMode.Global:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), infoFile.FriendlyName),
                                App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }

                    return null;
                }

                Mod mod = new ZippedMod(archiveFile, infoFile, parentCollection, modpackCollection);

                var modDirectory = App.Instance.Settings.GetModDirectory(infoFile.FactorioVersion);
                if (!modDirectory.Exists) modDirectory.Create();
                await mod.MoveTo(modDirectory, copy);

                return mod;
            }
            else
            {
                MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), archiveFile.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<Mod> Add(DirectoryInfo directory, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false, bool showPrompts = true)
        {
            if (DirectoryValid(directory, out InfoFile infoFile, hasUid))
            {
                if (parentCollection.ContainsByFactorioVersion(infoFile.Name, infoFile.FactorioVersion))
                {
                    switch (App.Instance.Settings.ManagerMode)
                    {
                        case ManagerMode.PerFactorioVersion:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), infoFile.FriendlyName, infoFile.FactorioVersion),
                                App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case ManagerMode.Global:
                            MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), infoFile.FriendlyName),
                                App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }

                    return null;
                }

                Mod mod = new ExtractedMod(directory, infoFile, parentCollection, modpackCollection);

                var modDirectory = App.Instance.Settings.GetModDirectory(infoFile.FactorioVersion);
                if (!modDirectory.Exists) modDirectory.Create();
                await mod.MoveTo(modDirectory, copy);

                return mod;
            }
            else
            {
                MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), directory.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<Mod> Add(FileSystemInfo fileOrDirectory, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false, bool showPrompts = true)
        {
            FileInfo file = fileOrDirectory as FileInfo;
            if (file != null) return await Add(file, parentCollection, modpackCollection, copy, hasUid, showPrompts);

            DirectoryInfo directory = fileOrDirectory as DirectoryInfo;
            if (directory != null) return await Add(directory, parentCollection, modpackCollection, copy, hasUid, showPrompts);

            throw new ArgumentException("Only files and directories are supported.", nameof(fileOrDirectory));
        }
    }
}
