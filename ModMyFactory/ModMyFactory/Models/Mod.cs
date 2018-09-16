using System;
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
    sealed partial class Mod : NotifyPropertyChangedBase
    {
        private readonly ModCollection parentCollection;
        private readonly ModpackCollection modpackCollection;
        private readonly ModFileCollection oldVersions;

        bool active;
        bool isSelected;
        ModFile file;

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
        /// The mods file.
        /// </summary>
        public ModFile File
        {
            get => file;
            private set
            {
                if (value != file)
                {
                    file = value;

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Version)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioVersion)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FriendlyName)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Author)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Description)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Dependencies)));
                }
            }
        }

        private InfoFile InfoFile => File.InfoFile;

        /// <summary>
        /// The unique name of the mod.
        /// </summary>
        public string Name => InfoFile.Name;

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public Version Version => InfoFile.Version;

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        public Version FactorioVersion => InfoFile.FactorioVersion;

        /// <summary>
        /// The friendly of the mod.
        /// </summary>
        public string FriendlyName => InfoFile.FriendlyName;

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author => InfoFile.Author;

        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description => InfoFile.Description;

        /// <summary>
        /// This mods dependencies.
        /// </summary>
        public ModDependency[] Dependencies => InfoFile.Dependencies;

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

        private Mod(ModCollection parentCollection, ModpackCollection modpackCollection)
        {
            this.parentCollection = parentCollection;
            this.modpackCollection = modpackCollection;

            DeleteCommand = new RelayCommand<bool?>(showPrompt => Delete(showPrompt ?? true));
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        private Mod(ModFileCollection files, ModCollection parentCollection, ModpackCollection modpackCollection)
            : this(parentCollection, modpackCollection)
        {
            file = files.Latest;
            files.Remove(file);
            oldVersions = files;

            active = ModManager.GetActive(Name, FactorioVersion);
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        private Mod(ModFile file, ModCollection parentCollection, ModpackCollection modpackCollection)
            : this(parentCollection, modpackCollection)
        {
            this.file = file;
            oldVersions = new ModFileCollection();

            active = ModManager.GetActive(Name, FactorioVersion);
        }















        /// <summary>
        /// Specifies if updates for this mod should be extracted.
        /// </summary>
        public abstract bool ExtractUpdates { get; }

        

        
        
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

        

        public abstract bool AlwaysKeepOnUpdate();

        /// <summary>
        /// Moves this mod to a specified directory.
        /// </summary>
        public abstract Task MoveTo(DirectoryInfo destinationDirectory, bool copy);

        /// <summary>
        /// Exports the mods file.
        /// </summary>
        public abstract Task ExportFile(string destination, int uid = -1);
    }
}
