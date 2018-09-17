using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        private static void ShowModExistsMessage(InfoFile infoFile)
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
        }

        /// <summary>
        /// Adds a mod to the managed mod directory.
        /// </summary>
        /// <param name="file">The mod file to be added.</param>
        /// <param name="parentCollection">The mod collection.</param>
        /// <param name="modpackCollection">The modpack collection.</param>
        /// <param name="copy">Specifies if the mod file should be copied or moved.</param>
        /// <param name="silent">Specifies if messages should be displayed.</param>
        /// <returns>Returns the added mod, or an existing mod if the mod that was tried to be added already existed in the managed mod directory.</returns>
        public static async Task<Mod> Add(ModFile file, ModCollection parentCollection, ModpackCollection modpackCollection, bool copy, bool silent = false)
        {
            var infoFile = file.InfoFile;

            var existingMod = parentCollection.FindByFactorioVersion(infoFile.Name, infoFile.FactorioVersion);
            if ((existingMod != null) && (existingMod.Version >= file.Version))
            {
                if (!silent) ShowModExistsMessage(infoFile);
                return existingMod;
            }
            
            if (!file.ResidesInModDirectory)
            {
                var modDirectory = App.Instance.Settings.GetModDirectory(infoFile.FactorioVersion);
                if (!modDirectory.Exists) modDirectory.Create();

                if (copy)
                {
                    file = await file.CopyToAsync(modDirectory.FullName);
                }
                else
                {
                    await file.MoveToAsync(modDirectory.FullName);
                }
            }

            if (existingMod != null)
            {
                await existingMod.UpdateAsync(file);
                return existingMod;
            }
            else
            {
                var newMod = new Mod(file, parentCollection, modpackCollection);
                parentCollection.Add(newMod);
                return newMod;
            }
        }

        /// <summary>
        /// Adds a mod to the managed mod directory.
        /// </summary>
        /// <param name="archiveFile">The mod file to be added.</param>
        /// <param name="parentCollection">The mod collection.</param>
        /// <param name="modpackCollection">The modpack collection.</param>
        /// <param name="copy">Specifies if the mod file should be copied or moved.</param>
        /// <param name="hasUid">Specifies if the mod file to be added contains a UID in its name.</param>
        /// <param name="silent">Specifies if messages should be displayed.</param>
        /// <returns>Returns the added mod, or an existing mod if the mod that was tried to be added already existed in the managed mod directory.</returns>
        public static async Task<Mod> Add(FileInfo archiveFile, ModCollection parentCollection, ModpackCollection modpackCollection, bool copy, bool hasUid = false, bool silent = false)
        {
            if (ModFile.TryLoadFromFile(archiveFile, out var modFile, hasUid))
            {
                return await Add(modFile, parentCollection, modpackCollection, copy, silent);
            }
            else
            {
                if (!silent)
                {
                    MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), archiveFile.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }
        }

        /// <summary>
        /// Adds a mod to the managed mod directory.
        /// </summary>
        /// <param name="directory">The mod directory to be added.</param>
        /// <param name="parentCollection">The mod collection.</param>
        /// <param name="modpackCollection">The modpack collection.</param>
        /// <param name="copy">Specifies if the mod file should be copied or moved.</param>
        /// <param name="hasUid">Specifies if the mod directory to be added contains a UID in its name.</param>
        /// <param name="silent">Specifies if messages should be displayed.</param>
        /// <returns>Returns the added mod, or an existing mod if the mod that was tried to be added already existed in the managed mod directory.</returns>
        public static async Task<Mod> Add(DirectoryInfo directory, ModCollection parentCollection, ModpackCollection modpackCollection, bool copy, bool hasUid = false, bool silent = false)
        {
            if (ModFile.TryLoadFromDirectory(directory, out var modFile, hasUid))
            {
                return await Add(modFile, parentCollection, modpackCollection, copy, silent);
            }
            else
            {
                if (!silent)
                {
                    MessageBox.Show(string.Format(App.Instance.GetLocalizedMessage("InvalidModFolder", MessageType.Error), directory.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModFolder", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }
        }

        /// <summary>
        /// Adds a mod to the managed mod directory.
        /// </summary>
        /// <param name="fileOrDirectory">The mod file to be added.</param>
        /// <param name="parentCollection">The mod collection.</param>
        /// <param name="modpackCollection">The modpack collection.</param>
        /// <param name="copy">Specifies if the mod file should be copied or moved.</param>
        /// <param name="hasUid">Specifies if the mod file to be added contains a UID in its name.</param>
        /// <param name="silent">Specifies if messages should be displayed.</param>
        /// <returns>Returns the added mod, or an existing mod if the mod that was tried to be added already existed in the managed mod directory.</returns>
        public static async Task<Mod> Add(FileSystemInfo fileOrDirectory, ModCollection parentCollection, ICollection<Modpack> modpackCollection, bool copy, bool hasUid = false, bool silent = false)
        {
            FileInfo file = fileOrDirectory as FileInfo;
            if (file != null) return await Add(file, parentCollection, modpackCollection, copy, hasUid, silent);

            DirectoryInfo directory = fileOrDirectory as DirectoryInfo;
            if (directory != null) return await Add(directory, parentCollection, modpackCollection, copy, hasUid, silent);

            throw new ArgumentException("Only files and directories are supported.", nameof(fileOrDirectory));
        }
    }
}
