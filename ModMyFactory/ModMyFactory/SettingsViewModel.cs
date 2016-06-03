using System;
using System.ComponentModel;
using System.IO;

namespace ModMyFactory
{
    sealed class SettingsViewModel : NotifyPropertyChangedBase
    {
        static SettingsViewModel instance;

        public static SettingsViewModel Instance => instance ?? (instance = new SettingsViewModel());

        string modDatabaseFile;

        public string ModDatabaseFile
        {
            get { return modDatabaseFile; }
            set
            {
                if (value != modDatabaseFile)
                {
                    modDatabaseFile = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModDatabaseFile)));
                }
            }
        }

        public RelayCommand SelectDatabaseFileCommand { get; }

        private SettingsViewModel()
        {
            SelectDatabaseFileCommand = new RelayCommand(SelectDatabaseFile);
        }

        private void SelectDatabaseFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = false;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Filter = "Factorio mod database file|mod-list.json";

            var dir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio", "mods"));
            if (dir.Exists) dialog.InitialDirectory = dir.FullName;

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                ModDatabaseFile = dialog.FileName;
        }
    }
}
