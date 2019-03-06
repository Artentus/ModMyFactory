using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class SettingsViewModel : ViewModelBase
    {
        static SettingsViewModel instance;

        public static SettingsViewModel Instance => instance ?? (instance = new SettingsViewModel());
        
        #region Misc

        bool updateSearchOnStartup;
        bool includePreReleasesForUpdate;
        bool preSelectModVersions;
        bool alwaysUpdateZipped;
        bool keepOldModVersions;

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

        public bool PreSelectModVersions
        {
            get { return preSelectModVersions; }
            set
            {
                if (value != preSelectModVersions)
                {
                    preSelectModVersions = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(PreSelectModVersions)));
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
            
            UpdateSearchOnStartup = settings.UpdateSearchOnStartup;
            IncludePreReleasesForUpdate = settings.IncludePreReleasesForUpdate;

            PreSelectModVersions = settings.PreSelectModVersions;
            AlwaysUpdateZipped = settings.AlwaysUpdateZipped;
            KeepOldModVersions = settings.KeepOldModVersions;

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
