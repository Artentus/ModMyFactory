using System;
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

        bool managerModeIsPerFactorioVersion;
        bool managerModeIsGlobal;

        bool factorioDirectoryIsAppData;
        bool factorioDirectoryIsAppDirectory;
        bool factorioDirectoryIsCustom;

        bool modDirectoryIsAppData;
        bool modDirectoryIsAppDirectory;
        bool modDirectoryIsCustom;

        string factorioDirectory;
        string modDirectory;
        bool settingsValid;

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

        private SettingsViewModel()
        {
            SelectFactorioDirectoryCommand = new RelayCommand(SelectFactorioDirectory);
            SelectModDirectoryCommand = new RelayCommand(SelectModDirectory);
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

        private bool PathValid(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

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
                && (!ModDirectoryIsCustom || PathValid(ModDirectory));
        }
    }
}
