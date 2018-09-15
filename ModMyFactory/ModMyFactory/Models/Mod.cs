using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.ViewModels;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A mod.
    /// </summary>
    abstract partial class Mod : NotifyPropertyChangedBase
    {
        private readonly ICollection<Mod> parentCollection;
        private readonly ICollection<Modpack> modpackCollection;

        bool active;
        bool isSelected;

        /// <summary>
        /// The title of the mod.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        public Version FactorioVersion { get; private set; }

        /// <summary>
        /// This mods dependencies.
        /// </summary>
        public IReadOnlyCollection<ModDependency> Dependencies { get; }

        private List<ModFile> OldVersions { get; set; }
        
        /// <summary>
        /// Specifies if updates for this mod should be extracted.
        /// </summary>
        public abstract bool ExtractUpdates { get; }

        /// <summary>
        /// Indicates whether the mod is currently active.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));

                    ModManager.SetActive(Name, FactorioVersion, value);
                }
            }
        }

        /// <summary>
        /// Additional information about this mod to be displayed in a tooltip.
        /// </summary>
        public string ToolTip
        {
            get
            {
                var authorAndVersion = $"Author: {Author}     Version: {Version}";
                return $"{authorAndVersion}\n\n{Description.Wrap(authorAndVersion.Length)}";
            }
        }

        /// <summary>
        /// Indicates whether this mod is selected in the list.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// A command that deletes this mod from the list and the filesystem.
        /// </summary>
        public RelayCommand<bool?> DeleteCommand { get; }

        private void DeleteOldVersions()
        {
            foreach (var file in OldVersions)
            {
                if (file.Exists)
                    file.Delete();
            }
        }

        protected abstract void DeleteFile();

        /// <summary>
        /// Deletes this mod at file system level.
        /// </summary>
        public void DeleteFilesystemObjects()
        {
            DeleteFile();
            DeleteOldVersions();
        }
        
        /// <summary>
        /// Deletes this mod from the list and the filesystem.
        /// </summary>
        /// <param name="showPrompt">Indicates whether a confirmation prompt is shown to the user.</param>
        public void Delete(bool showPrompt)
        {
            if (!showPrompt || (MessageBox.Show(
                App.Instance.GetLocalizedMessage("DeleteMod", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("DeleteMod", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
            {
                foreach (var modpack in modpackCollection)
                {
                    ModReference reference;
                    if (modpack.Contains(this, out reference))
                        modpack.Mods.Remove(reference);

                }

                DeleteFilesystemObjects();
                parentCollection.Remove(this);

                ModManager.RemoveTemplate(Name, FactorioVersion);
                ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                ModpackTemplateList.Instance.Save();
            }
        }

        public void PrepareUpdate(Version newFactorioVersion)
        {
            FactorioVersion = newFactorioVersion;
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        protected Mod(InfoFile infoFile, ICollection<Mod> parentCollection, ICollection<Modpack> modpackCollection)
        {
            Name = infoFile.Name;
            Version = infoFile.Version;
            FactorioVersion = infoFile.FactorioVersion;
            Title = infoFile.Title;
            Description = infoFile.Description;
            Author = infoFile.Author;
            Dependencies = new ReadOnlyCollection<ModDependency>(infoFile.Dependencies);
            active = ModManager.GetActive(Name, FactorioVersion);

            this.parentCollection = parentCollection;
            this.modpackCollection = modpackCollection;

            DeleteCommand = new RelayCommand<bool?>(showPrompt => Delete(showPrompt ?? true));
        }

        public abstract bool AlwaysKeepOnUpdate();

        /// <summary>
        /// Moves this mod to a specified directory.
        /// </summary>
        public abstract Task MoveTo(DirectoryInfo destinationDirectory, bool copy);
    }
}
