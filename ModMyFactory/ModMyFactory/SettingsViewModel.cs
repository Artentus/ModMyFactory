using System.ComponentModel;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace ModMyFactory
{
    sealed class SettingsViewModel : NotifyPropertyChangedBase
    {
        static SettingsViewModel instance;

        public static SettingsViewModel Instance => instance ?? (instance = new SettingsViewModel());

        bool factorioDirectoryIsAppData;
        bool factorioDirectoryIsAppDirectory;
        bool factorioDirectoryIsCustom;

        bool modDirectoryIsAppData;
        bool modDirectoryIsAppDirectory;
        bool modDirectoryIsCustom;

        string factorioDirectory;
        string modDirectory;
        bool settingsValid;

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
            switch (App.Instance.Settings.FactorioDirectoryOption)
            {
                case DirectoryOption.AppData:
                    factorioDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    factorioDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    factorioDirectoryIsCustom = true;
                    factorioDirectory = App.Instance.Settings.FactorioDirectory;
                    break;
            }

            switch (App.Instance.Settings.ModDirectoryOption)
            {
                case DirectoryOption.AppData:
                    modDirectoryIsAppData = true;
                    break;
                case DirectoryOption.ApplicationDirectory:
                    modDirectoryIsAppDirectory = true;
                    break;
                case DirectoryOption.Custom:
                    factorioDirectoryIsCustom = true;
                    modDirectory = App.Instance.Settings.ModDirectory;
                    break;
            }

            ValidateSettings();

            SelectFactorioDirectoryCommand = new RelayCommand(SelectFactorioDirectory);
            SelectModDirectoryCommand = new RelayCommand(SelectModDirectory);
        }

        private void SelectFactorioDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog();
            if (result != null && result.Value)
                FactorioDirectory = dialog.SelectedPath;
        }

        private void SelectModDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog();
            if (result != null && result.Value)
                ModDirectory = dialog.SelectedPath;
        }

        private void ValidateSettings()
        {
            bool settingsValid = true;

            if (FactorioDirectoryIsCustom && !Directory.Exists(FactorioDirectory)) settingsValid = false;
            if (ModDirectoryIsCustom && !Directory.Exists(ModDirectory)) settingsValid = false;

            SettingsValid = settingsValid;
        }
    }
}
