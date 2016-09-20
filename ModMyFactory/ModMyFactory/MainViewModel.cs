using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;
using ModMyFactory.Lang;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    sealed class MainViewModel : ViewModelBase<MainWindow>
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        FactorioVersion selectedVersion;

        public ICollectionView AvailableCultures { get; }

        public ICollectionView FactorioVersions { get; }

        public FactorioVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));

                    App.Instance.Settings.SelectedVersion = selectedVersion?.Version.ToString(3) ?? string.Empty;
                    App.Instance.Settings.Save();
                }
            }
        }

        public ICollectionView Mods { get; }

        public ICollectionView Modpacks { get; }

        public RelayCommand StartGameCommand { get; }

        public RelayCommand OpenVersionManagerCommand { get; }

        public RelayCommand OpenSettingsCommand { get; }

        public RelayCommand BrowseFactorioWebsiteCommand { get; }

        public RelayCommand BrowseModWebsiteCommand { get; }

        public RelayCommand OpenAboutWindowCommand { get; }

        private MainViewModel()
        {
            List<CultureEntry> availableCultures = App.Instance.GetAvailableCultures();
            AvailableCultures = CollectionViewSource.GetDefaultView(availableCultures);
            ((ListCollectionView)AvailableCultures).CustomSort = new CultureEntrySorter();
            availableCultures.First(entry => string.Equals(entry.LanguageCode, App.Instance.Settings.SelectedLanguage, StringComparison.InvariantCultureIgnoreCase)).Select();

            var factorioVersions = new ObservableCollection<FactorioVersion>();
            FactorioVersion.GetInstalledVersions().ForEach(item => factorioVersions.Add(item));
            FactorioVersions = CollectionViewSource.GetDefaultView(factorioVersions);
            ((ListCollectionView)FactorioVersions).CustomSort = new FactorioVersionSorter();

            Version version;
            bool result = Version.TryParse(App.Instance.Settings.SelectedVersion, out version);
            if (result)
            {
                FactorioVersion factorioVersion = factorioVersions.FirstOrDefault(item => item.Version == version);
                if (factorioVersion != null)
                {
                    selectedVersion = factorioVersion;
                }
                else
                {
                    App.Instance.Settings.SelectedVersion = string.Empty;
                    App.Instance.Settings.Save();
                }
            }

            var mods = new ObservableCollection<Mod>();
            Mods = CollectionViewSource.GetDefaultView(mods);
            ((ListCollectionView)Mods).CustomSort = new ModSorter();

            var modpacks = new ObservableCollection<Modpack>();
            Modpacks = CollectionViewSource.GetDefaultView(modpacks);
            ((ListCollectionView)Modpacks).CustomSort = new ModpackSorter();

            StartGameCommand = new RelayCommand(StartGame, () => SelectedVersion != null);
            OpenVersionManagerCommand = new RelayCommand(OpenVersionManager);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            BrowseFactorioWebsiteCommand = new RelayCommand(() => Process.Start("https://www.factorio.com/"));
            BrowseModWebsiteCommand = new RelayCommand(() => Process.Start("https://mods.factorio.com/"));
            OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);
            


            Mod mod1 = new Mod("aaa", new FileInfo("a"));
            Mod mod2 = new Mod("bbb", new FileInfo("b"));
            Mod mod3 = new Mod("ccc", new FileInfo("c"));
            Mod mod4 = new Mod("ddd", new FileInfo("d"));
            Mod mod5 = new Mod("eee", new FileInfo("e"));
            mods.Add(mod1);
            mods.Add(mod2);
            mods.Add(mod3);
            mods.Add(mod4);
            mods.Add(mod5);
            Modpack modpack1 = new Modpack("aaa");
            Modpack modpack2 = new Modpack("bbb");
            Modpack modpack3 = new Modpack("ccc");
            //modpack.Mods.Add(mod1);
            //modpack.Mods.Add(mod2);
            modpacks.Add(modpack1);
            modpacks.Add(modpack2);
            modpacks.Add(modpack3);
        }

        private void StartGame()
        {
            string modDirectory = Path.Combine(App.Instance.Settings.GetModDirectory().FullName, SelectedVersion.Version.ToString(3));
            Process.Start(SelectedVersion.ExecutablePath, $"--mod-directory \"{modDirectory}\"");
        }

        private void OpenVersionManager()
        {
            var versionManagementWindow = new VersionManagementWindow() { Owner = Window };
            versionManagementWindow.ShowDialog();
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow() { Owner = Window };
            settingsWindow.ViewModel.Reset();

            bool? result = settingsWindow.ShowDialog();
            if (result != null && result.Value)
            {
                if (settingsWindow.ViewModel.FactorioDirectoryIsAppData)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.AppData;
                    App.Instance.Settings.FactorioDirectory = string.Empty;
                }
                else if (settingsWindow.ViewModel.FactorioDirectoryIsAppDirectory)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.ApplicationDirectory;
                    App.Instance.Settings.FactorioDirectory = string.Empty;
                }
                else if (settingsWindow.ViewModel.FactorioDirectoryIsCustom)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.Custom;
                    App.Instance.Settings.FactorioDirectory = settingsWindow.ViewModel.FactorioDirectory;
                }

                if (settingsWindow.ViewModel.ModDirectoryIsAppData)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.AppData;
                    App.Instance.Settings.ModDirectory = string.Empty;
                }
                else if (settingsWindow.ViewModel.ModDirectoryIsAppDirectory)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.ApplicationDirectory;
                    App.Instance.Settings.ModDirectory = string.Empty;
                }
                else if (settingsWindow.ViewModel.ModDirectoryIsCustom)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.Custom;
                    App.Instance.Settings.ModDirectory = settingsWindow.ViewModel.ModDirectory;
                }

                App.Instance.Settings.Save();
            }
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }
    }
}
