using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class SettingsViewModel : ViewModelBase
    {
        static SettingsViewModel instance;

        public static SettingsViewModel Instance => instance ?? (instance = new SettingsViewModel());

        #region ManagerMode

        bool managerModeIsPerFactorioVersion;
        bool managerModeIsGlobal;

        public bool ManagerModeIsPerFactorioVersion
        {
            get { return managerModeIsPerFactorioVersion; }
            set
            {
                if (value != managerModeIsPerFactorioVersion)
                {
                    managerModeIsPerFactorioVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ManagerModeIsPerFactorioVersion)));
                }
            }
        }

        public bool ManagerModeIsGlobal
        {
            get { return managerModeIsGlobal; }
            set
            {
                if (value != managerModeIsGlobal)
                {
                    managerModeIsGlobal = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ManagerModeIsGlobal)));
                }
            }
        }

        public ManagerMode ManagerMode => ManagerModeIsGlobal ? ManagerMode.Global : ManagerMode.PerFactorioVersion;

        #endregion

        #region Misc

        bool updateSearchOnStartup;
        bool includePreReleasesForUpdate;
        bool alwaysUpdateZipped;
        bool keepOldModVersions;
        bool keepExtracted;
        bool keepZipped;
        bool keepWhenNewFactorioVersion;
        bool updateIntermediate;

        public bool UpdateSearchOnStartup
        {
            get { return updateSearchOnStartup; }
            set
            {
                if (value != updateSearchOnStartup)
                {
                    updateSearchOnStartup = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UpdateSearchOnStartup)));
                }
            }
        }

        public bool IncludePreReleasesForUpdate
        {
            get { return includePreReleasesForUpdate; }
            set
            {
                if (value != includePreReleasesForUpdate)
                {
                    includePreReleasesForUpdate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IncludePreReleasesForUpdate)));
                }
            }
        }

        public bool AlwaysUpdateZipped
        {
            get { return alwaysUpdateZipped; }
            set
            {
                if (value != alwaysUpdateZipped)
                {
                    alwaysUpdateZipped = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AlwaysUpdateZipped)));
                }
            }
        }

        public bool KeepOldModVersions
        {
            get { return keepOldModVersions; }
            set
            {
                if (value != keepOldModVersions)
                {
                    keepOldModVersions = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(KeepOldModVersions)));
                }

                if (keepOldModVersions)
                {
                    KeepExtracted = true;
                    KeepZipped = true;
                    KeepWhenNewFactorioVersion = true;
                }
            }
        }

        public bool KeepExtracted
        {
            get { return keepExtracted; }
            set
            {
                if (value != keepExtracted)
                {
                    keepExtracted = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(KeepExtracted)));
                }
            }
        }

        public bool KeepZipped
        {
            get { return keepZipped; }
            set
            {
                if (value != keepZipped)
                {
                    keepZipped = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(KeepZipped)));
                }
            }
        }

        public bool KeepWhenNewFactorioVersion
        {
            get { return keepWhenNewFactorioVersion; }
            set
            {
                if (value != keepWhenNewFactorioVersion)
                {
                    keepWhenNewFactorioVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(KeepWhenNewFactorioVersion)));
                }
            }
        }

        public bool UpdateIntermediate
        {
            get { return updateIntermediate; }
            set
            {
                if (value != updateIntermediate)
                {
                    updateIntermediate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UpdateIntermediate)));
                }
            }
        }

        #endregion

        #region ModDependencies

        bool activateDependencies;
        bool activateOptionalDependencies;

        public bool ActivateDependencies
        {
            get => activateDependencies;
            set
            {
                if (value != activateDependencies)
                {
                    activateDependencies = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActivateDependencies)));

                    if (!activateDependencies)
                        ActivateOptionalDependencies = false;
                }
            }
        }

        public bool ActivateOptionalDependencies
        {
            get => activateOptionalDependencies;
            set
            {
                if (value != activateOptionalDependencies)
                {
                    activateOptionalDependencies = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActivateOptionalDependencies)));
                }
            }
        }

        #endregion
        
        bool settingsValid;

        public bool SettingsValid
        {
            get { return settingsValid; }
            private set
            {
                if (value != settingsValid)
                {
                    settingsValid = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SettingsValid)));
                }
            }
        }
        
        public void Reset()
        {
            Settings settings = App.Instance.Settings;

            ManagerModeIsPerFactorioVersion = false;
            ManagerModeIsGlobal = false;
            switch (settings.ManagerMode)
            {
                case ManagerMode.PerFactorioVersion:
                    ManagerModeIsPerFactorioVersion = true;
                    break;
                case ManagerMode.Global:
                    ManagerModeIsGlobal = true;
                    break;
            }

            UpdateSearchOnStartup = settings.UpdateSearchOnStartup;
            IncludePreReleasesForUpdate = settings.IncludePreReleasesForUpdate;

            AlwaysUpdateZipped = settings.AlwaysUpdateZipped;
            KeepExtracted = settings.KeepOldExtractedModVersions;
            KeepZipped = settings.KeepOldZippedModVersions;
            KeepWhenNewFactorioVersion = settings.KeepOldModVersionsWhenNewFactorioVersion;
            KeepOldModVersions = settings.KeepOldModVersions;
            UpdateIntermediate = settings.DownloadIntermediateUpdates;

            ActivateOptionalDependencies = settings.ActivateOptionalDependencies;
            ActivateDependencies = settings.ActivateDependencies;
            
            ValidateSettings();
        }
        
        private void ValidateSettings()
        {
            SettingsValid = true;
        }
    }
}
