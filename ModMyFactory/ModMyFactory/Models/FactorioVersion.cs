using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using ModMyFactory.IO;
using WPFCore;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents a version of Factorio.
    /// </summary>
    class FactorioVersion : NotifyPropertyChangedBase
    {
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
                        folder.RenameToUnique();
                        versionList.Add(new FactorioVersion(folder));
                    }
                }
            }

            return versionList;
        }


        
        string name;
        readonly bool hasLinks;
        readonly bool canMove;
        DirectoryInfo linkDirectory;

        public bool IsNameEditable { get; }

        public bool CanUpdate { get; }

        private FactorioFolder Folder { get; }

        public string Name
        {
            get => name;
            set
            {
                if (!IsNameEditable)
                    throw new NotSupportedException();

                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));

                    SaveName(name);
                }
            }
        }

        public virtual Version Version => Folder?.Version;

        public virtual string DisplayName => $"{Name} ({Version ?? new Version(0,0)})";

        public virtual DirectoryInfo Directory => Folder?.Directory;

        public virtual FileInfo Executable => Folder?.Executable;

        public bool Is64Bit => Folder?.Is64Bit ?? false;

        protected FactorioVersion()
        {
            hasLinks = false;
            canMove = false;
            Folder = null;
            IsNameEditable = false;
            CanUpdate = false;

            name = LoadName();
        }

        protected FactorioVersion(FactorioFolder folder, bool canMove, DirectoryInfo linkDirectory)
        {
            hasLinks = true;
            this.canMove = canMove;
            Folder = folder;
            IsNameEditable = false;
            CanUpdate = false;

            name = LoadName();

            this.linkDirectory = linkDirectory;
            if (!linkDirectory.Exists) linkDirectory.Create();
            CreateLinks();
        }

        public FactorioVersion(FactorioFolder folder)
        {
            hasLinks = true;
            canMove = true;
            Folder = folder;
            IsNameEditable = true;
            CanUpdate = true;

            name = LoadName();
            
            linkDirectory = folder.Directory;
            CreateLinks();
        }

        protected virtual string LoadName()
        {
            return "Factorio";
        }

        private void SaveName(string name)
        {

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
