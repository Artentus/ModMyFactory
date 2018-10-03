using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ModMyFactory.FactorioUpdate;
using ModMyFactory.IO;
using ModMyFactory.ViewModels;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents a version of Factorio.
    /// </summary>
    class FactorioVersion : NotifyPropertyChangedBase, IEditableObject
    {
        private static int counter = 0;

        /// <summary>
        /// Loads all installed versions of Factorio.
        /// </summary>
        /// <returns>Returns a list that contains all installed Factorio versions.</returns>
        public static List<FactorioVersion> LoadInstalledVersions()
        {
            var versionList = new List<FactorioVersion>();

            DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
            if (factorioDirectory.Exists)
            {
                foreach (var directory in factorioDirectory.EnumerateDirectories())
                {
                    if (FactorioFolder.TryLoad(directory, out var folder))
                    {
                        if (folder.Is64Bit == Environment.Is64BitOperatingSystem)
                        {
                            folder.RenameToUnique();
                            versionList.Add(new FactorioVersion(folder));
                        }
                    }
                }
            }

            return versionList;
        }



        FactorioFolder folder;
        string name;
        string editingName;
        readonly bool hasLinks;
        readonly bool canMove;
        DirectoryInfo linkDirectory;
        bool editing;

        public bool IsNameEditable { get; }

        public bool CanUpdate { get; }

        private FactorioFolder Folder
        {
            get => folder;
            set
            {
                if (value != folder)
                {
                    folder = value;

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Version)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Directory)));
                }
            }
        }

        private string GetUniqueName(string baseName)
        {
            int counter = 0;
            string candidateName = baseName;

            while (MainViewModel.Instance.FactorioVersions.Contains(candidateName))
            {
                counter++;
                candidateName = $"{baseName} ({counter})";
            }

            return candidateName;
        }

        public string Name
        {
            get => name;
            set
            {
                if (!IsNameEditable)
                    throw new NotSupportedException();

                if (value != name)
                {
                    string newName = GetUniqueName(value);
                    name = newName;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));

                    EditingName = newName;
                    SaveName(name);
                }
            }
        }

        public string EditingName
        {
            get { return editingName; }
            set
            {
                if (value != editingName)
                {
                    editingName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(EditingName)));
                }
            }
        }

        public virtual Version Version => Folder?.Version;

        public virtual string DisplayName => $"{Name} ({Version ?? new Version(0,0)})";

        public virtual DirectoryInfo Directory => Folder?.Directory;

        public bool Is64Bit => Folder?.Is64Bit ?? false;

        /// <summary>
        /// Indicates whether the user currently edits the name of this Factorio version.
        /// </summary>
        public bool Editing
        {
            get { return editing; }
            set
            {
                if (value != editing)
                {
                    editing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Editing)));

                    if (editing)
                    {
                        VersionManagementViewModel.Instance.FactorioVersionsView.EditItem(this);
                    }
                    else
                    {
                        Name = EditingName;
                        VersionManagementViewModel.Instance.FactorioVersionsView.CommitEdit();
                    }
                }
            }
        }

        /// <summary>
        /// A command that finishes renaming this Factorio version.
        /// </summary>
        public RelayCommand EndEditCommand { get; }

        protected FactorioVersion()
        {
            hasLinks = false;
            canMove = false;
            folder = null;
            IsNameEditable = false;
            CanUpdate = false;

            name = LoadName();
            editingName = name;
            EndEditCommand = new RelayCommand(EndEdit, () => Editing);
        }

        protected FactorioVersion(FactorioFolder folder, bool canMove, DirectoryInfo linkDirectory)
        {
            hasLinks = true;
            this.canMove = canMove;
            this.folder = folder;
            IsNameEditable = false;
            CanUpdate = false;

            name = LoadName();
            editingName = name;
            EndEditCommand = new RelayCommand(EndEdit, () => Editing);

            this.linkDirectory = linkDirectory;
            if (!linkDirectory.Exists) linkDirectory.Create();
            CreateLinks();
        }

        public FactorioVersion(FactorioFolder folder)
        {
            hasLinks = true;
            canMove = true;
            this.folder = folder;
            IsNameEditable = true;
            CanUpdate = true;

            name = LoadName();
            editingName = name;
            EndEditCommand = new RelayCommand(EndEdit, () => Editing);

            linkDirectory = folder.Directory;
            CreateLinks();
        }

        private FileInfo GetNameFile()
        {
            if (Directory == null) return null;
            return new FileInfo(Path.Combine(Directory.FullName, "name.cfg"));
        }

        protected virtual string LoadName()
        {
            var file = GetNameFile();
            if (file == null)
            {
                string name = (counter == 0) ? "Factorio" : $"Factorio {counter}";
                counter++;
                return name;
            }
            else if (!file.Exists)
            {
                string name = (counter == 0) ? "Factorio" : $"Factorio {counter}";
                counter++;
                SaveName(name);
                return name;
            }
            else
            {
                using (var stream = file.OpenRead())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private void SaveName(string name)
        {
            var file = GetNameFile();
            if (file != null)
            {
                using (var stream = file.Open(FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(name);
                    }
                }
            }
        }

        public void BeginEdit()
        {
            Editing = true;
        }

        public void EndEdit()
        {
            Editing = false;
        }

        public void CancelEdit()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Moves this Factorio installation to a different location if possible.
        /// </summary>
        public async Task MoveToAsync(DirectoryInfo destination)
        {
            if (canMove)
            {
                await Folder.MoveToAsync(destination);
                linkDirectory = Folder.Directory;
            }
        }

        /// <summary>
        /// Runs Factorio.
        /// </summary>
        /// <param name="args">Optional. Command line args.</param>
        public virtual void Run(string args = null)
        {
            var startInfo = new ProcessStartInfo(Folder.Executable.FullName);
            if (!string.IsNullOrWhiteSpace(args)) startInfo.Arguments = args;
            startInfo.WorkingDirectory = Folder.Executable.Directory.FullName;
            Process.Start(startInfo);
        }

        /// <summary>
        /// Updates this Factorio installation.
        /// </summary>
        /// <param name="packageFiles">A list of update packages to apply, in order.</param>
        public async Task UpdateAsync(List<FileInfo> packageFiles, IProgress<double> progress)
        {
            if (!CanUpdate)
                throw new NotSupportedException();
            
            await FactorioUpdater.ApplyUpdatePackagesAsync(this, packageFiles, progress);

            if (!FactorioFolder.TryLoad(Directory, out var newFolder))
                throw new CriticalUpdaterException(UpdaterErrorType.InstallationCorrupt);
            Folder = newFolder;
        }

        /// <summary>
        /// Deletes this Factorio installation.
        /// </summary>
        public virtual void Delete()
        {
            if (canMove)
            {
                if (hasLinks && (linkDirectory != null))
                    DeleteLinks();

                if ((Directory != null) && Directory.Exists)
                    Directory.Delete(true);
            }
        }
        
        /// <summary>
        /// Expands the 'executable', 'read-data' and 'write-data' variables in the specified path.
        /// </summary>
        public string ExpandPathVariables(string path)
        {
            const string executableVariable = "__PATH__executable__";
            const string readDataVariable = "__PATH__read-data__";
            const string writeDataVariable = "__PATH__write-data__";

            string executablePath = Path.Combine(Folder.Executable.Directory.FullName).Trim(Path.DirectorySeparatorChar);
            string readDataPath = Path.Combine(Directory.FullName, "data").Trim(Path.DirectorySeparatorChar);
            string writeDataPath = Directory.FullName.Trim(Path.DirectorySeparatorChar);

            path = path.Replace(executableVariable, executablePath);
            path = path.Replace(readDataVariable, readDataPath);
            path = path.Replace(writeDataVariable, writeDataPath);

            return path;
        }

        private void CreateSaveDirectoryLinkInternal(string localSavePath)
        {
            var globalSaveDirectory = App.Instance.Settings.GetSavegameDirectory();
            if (!globalSaveDirectory.Exists) globalSaveDirectory.Create();

            var localSaveJunction = new JunctionInfo(localSavePath);
            if (localSaveJunction.Exists)
            {
                localSaveJunction.SetDestination(globalSaveDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localSaveJunction.FullName))
                    System.IO.Directory.Delete(localSaveJunction.FullName, true);

                localSaveJunction.Create(globalSaveDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for saves.
        /// </summary>
        public void CreateSaveDirectoryLink()
        {
            if (hasLinks)
            {
                string localSavePath = Path.Combine(linkDirectory.FullName, "saves");
                CreateSaveDirectoryLinkInternal(localSavePath);
            }
        }

        private void CreateScenarioDirectoryLinkInternal(string localScenarioPath)
        {
            var globalScenarioDirectory = App.Instance.Settings.GetScenarioDirectory();
            if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

            var localScenarioJunction = new JunctionInfo(localScenarioPath);
            if (localScenarioJunction.Exists)
            {
                localScenarioJunction.SetDestination(globalScenarioDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localScenarioJunction.FullName))
                    System.IO.Directory.Delete(localScenarioJunction.FullName, true);

                localScenarioJunction.Create(globalScenarioDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for scenarios.
        /// </summary>
        public void CreateScenarioDirectoryLink()
        {
            if (hasLinks)
            {
                string localScenarioPath = Path.Combine(linkDirectory.FullName, "scenarios");
                CreateScenarioDirectoryLinkInternal(localScenarioPath);
            }
        }

        private void CreateModDirectoryLinkInternal(string localModPath)
        {
            var globalModDirectory = App.Instance.Settings.GetModDirectory(Version);
            if (!globalModDirectory.Exists) globalModDirectory.Create();

            var localModJunction = new JunctionInfo(localModPath);
            if (localModJunction.Exists)
            {
                localModJunction.SetDestination(globalModDirectory.FullName);
            }
            else
            {
                if (System.IO.Directory.Exists(localModJunction.FullName))
                    System.IO.Directory.Delete(localModJunction.FullName, true);

                localModJunction.Create(globalModDirectory.FullName);
            }
        }

        /// <summary>
        /// Creates the directory junction for mods.
        /// </summary>
        public void CreateModDirectoryLink()
        {
            if (hasLinks)
            {
                string localModPath = Path.Combine(linkDirectory.FullName, "mods");
                CreateModDirectoryLinkInternal(localModPath);
            }
        }

        /// <summary>
        /// Creates all directory junctions.
        /// </summary>
        public void CreateLinks()
        {
            if (hasLinks)
            {
                CreateSaveDirectoryLink();
                CreateScenarioDirectoryLink();
                CreateModDirectoryLink();
            }
        }

        /// <summary>
        /// Deletes all directory junctions.
        /// </summary>
        public void DeleteLinks()
        {
            if (hasLinks)
            {
                JunctionInfo localSaveJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "saves"));
                if (localSaveJunction.Exists) localSaveJunction.Delete();

                JunctionInfo localScenarioJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "scenarios"));
                if (localScenarioJunction.Exists) localScenarioJunction.Delete();

                JunctionInfo localModJunction = new JunctionInfo(Path.Combine(linkDirectory.FullName, "mods"));
                if (localModJunction.Exists) localModJunction.Delete();
            }
        }
    }
}
