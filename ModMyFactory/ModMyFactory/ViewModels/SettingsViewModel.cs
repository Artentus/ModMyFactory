﻿using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class SettingsViewModel : ViewModelBase
    {
        static SettingsViewModel instance;

        public static SettingsViewModel Instance => instance ?? (instance = new SettingsViewModel());
        
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

        #region FactorioDirectory

        bool factorioDirectoryIsAppData;
        bool factorioDirectoryIsAppDirectory;
        bool factorioDirectoryIsCustom;
        string factorioDirectory;

        public bool FactorioDirectoryIsAppData
        {
            get { return factorioDirectoryIsAppData; }
            set
            {
                if (value != factorioDirectoryIsAppData)
                {
                    factorioDirectoryIsAppData = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioDirectoryIsAppData)));
                    ValidateSettings();
                }
            }
        }

        public bool FactorioDirectoryIsAppDirectory
        {
            get { return factorioDirectoryIsAppDirectory; }
            set
            {
                if (value != factorioDirectoryIsAppDirectory)
                {
                    factorioDirectoryIsAppDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioDirectoryIsAppDirectory)));
                    ValidateSettings();
                }
            }
        }

        public bool FactorioDirectoryIsCustom
        {
            get { return factorioDirectoryIsCustom; }
            set
            {
                if (value != factorioDirectoryIsCustom)
                {
                    factorioDirectoryIsCustom = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioDirectoryIsCustom)));
                    ValidateSettings();
                }
            }
        }

        public string FactorioDirectory
        {
            get { return factorioDirectory; }
            set
            {
                if (value != factorioDirectory)
                {
                    factorioDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioDirectory)));
                    ValidateSettings();
                }
            }
        }

        public DirectoryOption FactorioDirectoryOption
        {
            get
            {
                if (factorioDirectoryIsAppDirectory)
                    return DirectoryOption.ApplicationDirectory;
                else if (factorioDirectoryIsCustom)
                    return DirectoryOption.Custom;
                else
                    return DirectoryOption.AppData;
            }
        }

        #endregion

        #region ModDirectory

        bool modDirectoryIsAppData;
        bool modDirectoryIsAppDirectory;
        bool modDirectoryIsCustom;
        string modDirectory;

        public bool ModDirectoryIsAppData
        {
            get { return modDirectoryIsAppData; }
            set
            {
                if (value != modDirectoryIsAppData)
                {
                    modDirectoryIsAppData = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModDirectoryIsAppData)));
                    ValidateSettings();
                }
            }
        }

        public bool ModDirectoryIsAppDirectory
        {
            get { return modDirectoryIsAppDirectory; }
            set
            {
                if (value != modDirectoryIsAppDirectory)
                {
                    modDirectoryIsAppDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModDirectoryIsAppDirectory)));
                    ValidateSettings();
                }
            }
        }

        public bool ModDirectoryIsCustom
        {
            get { return modDirectoryIsCustom; }
            set
            {
                if (value != modDirectoryIsCustom)
                {
                    modDirectoryIsCustom = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModDirectoryIsCustom)));
                    ValidateSettings();
                }
            }
        }

        public string ModDirectory
        {
            get { return modDirectory; }
            set
            {
                if (value != modDirectory)
                {
                    modDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModDirectory)));
                    ValidateSettings();
                }
            }
        }

        public DirectoryOption ModDirectoryOption
        {
            get
            {
                if (modDirectoryIsAppDirectory)
                    return DirectoryOption.ApplicationDirectory;
                else if (modDirectoryIsCustom)
                    return DirectoryOption.Custom;
                else
                    return DirectoryOption.AppData;
            }
        }

        #endregion

        #region SavegameDirectory

        bool savegameDirectoryIsAppData;
        bool savegameDirectoryIsAppDirectory;
        bool savegameDirectoryIsCustom;
        string savegameDirectory;

        public bool SavegameDirectoryIsAppData
        {
            get { return savegameDirectoryIsAppData; }
            set
            {
                if (value != savegameDirectoryIsAppData)
                {
                    savegameDirectoryIsAppData = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SavegameDirectoryIsAppData)));
                    ValidateSettings();
                }
            }
        }

        public bool SavegameDirectoryIsAppDirectory
        {
            get { return savegameDirectoryIsAppDirectory; }
            set
            {
                if (value != savegameDirectoryIsAppDirectory)
                {
                    savegameDirectoryIsAppDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SavegameDirectoryIsAppDirectory)));
                    ValidateSettings();
                }
            }
        }

        public bool SavegameDirectoryIsCustom
        {
            get { return savegameDirectoryIsCustom; }
            set
            {
                if (value != savegameDirectoryIsCustom)
                {
                    savegameDirectoryIsCustom = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SavegameDirectoryIsCustom)));
                    ValidateSettings();
                }
            }
        }

        public string SavegameDirectory
        {
            get { return savegameDirectory; }
            set
            {
                if (value != savegameDirectory)
                {
                    savegameDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SavegameDirectory)));
                    ValidateSettings();
                }
            }
        }

        public DirectoryOption SavegameDirectoryOption
        {
            get
            {
                if (savegameDirectoryIsAppDirectory)
                    return DirectoryOption.ApplicationDirectory;
                else if (savegameDirectoryIsCustom)
                    return DirectoryOption.Custom;
                else
                    return DirectoryOption.AppData;
            }
        }

        #endregion

        #region ScenarioDirectory

        bool scenarioDirectoryIsAppData;
        bool scenarioDirectoryIsAppDirectory;
        bool scenarioDirectoryIsCustom;
        string scenarioDirectory;

        public bool ScenarioDirectoryIsAppData
        {
            get { return scenarioDirectoryIsAppData; }
            set
            {
                if (value != scenarioDirectoryIsAppData)
                {
                    scenarioDirectoryIsAppData = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ScenarioDirectoryIsAppData)));
                    ValidateSettings();
                }
            }
        }

        public bool ScenarioDirectoryIsAppDirectory
        {
            get { return scenarioDirectoryIsAppDirectory; }
            set
            {
                if (value != scenarioDirectoryIsAppDirectory)
                {
                    scenarioDirectoryIsAppDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ScenarioDirectoryIsAppDirectory)));
                    ValidateSettings();
                }
            }
        }

        public bool ScenarioDirectoryIsCustom
        {
            get { return scenarioDirectoryIsCustom; }
            set
            {
                if (value != scenarioDirectoryIsCustom)
                {
                    scenarioDirectoryIsCustom = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(scenarioDirectoryIsCustom)));
                    ValidateSettings();
                }
            }
        }

        public string ScenarioDirectory
        {
            get { return scenarioDirectory; }
            set
            {
                if (value != scenarioDirectory)
                {
                    scenarioDirectory = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ScenarioDirectory)));
                    ValidateSettings();
                }
            }
        }

        public DirectoryOption ScenarioDirectoryOption
        {
            get
            {
                if (scenarioDirectoryIsAppDirectory)
                    return DirectoryOption.ApplicationDirectory;
                else if (scenarioDirectoryIsCustom)
                    return DirectoryOption.Custom;
                else
                    return DirectoryOption.AppData;
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

        public RelayCommand SelectFactorioDirectoryCommand { get; }

        public RelayCommand SelectModDirectoryCommand { get; }

        public RelayCommand SelectSavegameDirectoryCommand { get; }

        public RelayCommand SelectScenarioDirectoryCommand { get; }

        private SettingsViewModel()
        {
            SelectFactorioDirectoryCommand = new RelayCommand(SelectFactorioDirectory);
            SelectModDirectoryCommand = new RelayCommand(SelectModDirectory);
            SelectSavegameDirectoryCommand = new RelayCommand(SelectSavegameDirectory);
            SelectScenarioDirectoryCommand = new RelayCommand(SelectScenarioDirectory);
        }

        public void Reset()
        {
            Settings settings = App.Instance.Settings;
            
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

            FactorioDirectoryIsAppData = false;
            FactorioDirectoryIsAppDirectory = false;
            FactorioDirectoryIsCustom = false;
            FactorioDirectory = string.Empty;
            switch (settings.FactorioDirectoryOption)
            {
                case DirectoryOption.AppData:
                    FactorioDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    FactorioDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    FactorioDirectoryIsCustom = true;
                    FactorioDirectory = settings.FactorioDirectory;
                    break;
            }

            ModDirectoryIsAppData = false;
            ModDirectoryIsAppDirectory = false;
            ModDirectoryIsCustom = false;
            ModDirectory = string.Empty;
            switch (settings.ModDirectoryOption)
            {
                case DirectoryOption.AppData:
                    ModDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    ModDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    ModDirectoryIsCustom = true;
                    ModDirectory = settings.ModDirectory;
                    break;
            }

            SavegameDirectoryIsAppData = false;
            SavegameDirectoryIsAppDirectory = false;
            SavegameDirectoryIsCustom = false;
            SavegameDirectory = string.Empty;
            switch (settings.SavegameDirectoryOption)
            {
                case DirectoryOption.AppData:
                    SavegameDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    SavegameDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    SavegameDirectoryIsCustom = true;
                    SavegameDirectory = settings.SavegameDirectory;
                    break;
            }

            ScenarioDirectoryIsAppData = false;
            ScenarioDirectoryIsAppDirectory = false;
            ScenarioDirectoryIsCustom = false;
            ScenarioDirectory = string.Empty;
            switch (settings.ScenarioDirectoryOption)
            {
                case DirectoryOption.AppData:
                    ScenarioDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    ScenarioDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    ScenarioDirectoryIsCustom = true;
                    ScenarioDirectory = settings.ScenarioDirectory;
                    break;
            }

            ValidateSettings();
        }

        private void SelectFactorioDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog((Window)View);
            if (result != null && result.Value)
                FactorioDirectory = dialog.SelectedPath;
        }

        private void SelectModDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog((Window)View);
            if (result != null && result.Value)
                ModDirectory = dialog.SelectedPath;
        }

        private void SelectSavegameDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog((Window)View);
            if (result != null && result.Value)
                SavegameDirectory = dialog.SelectedPath;
        }

        private void SelectScenarioDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog((Window)View);
            if (result != null && result.Value)
                ScenarioDirectory = dialog.SelectedPath;
        }

        private bool PathValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return false;
            }

            return Path.IsPathRooted(path);
        }

        private void ValidateSettings()
        {
            SettingsValid =
                (!FactorioDirectoryIsCustom || PathValid(FactorioDirectory))
                && (!ModDirectoryIsCustom || PathValid(ModDirectory))
                && (!SavegameDirectoryIsCustom || PathValid(SavegameDirectory))
                && (!ScenarioDirectoryIsCustom || PathValid(ScenarioDirectory));
        }
    }
}
