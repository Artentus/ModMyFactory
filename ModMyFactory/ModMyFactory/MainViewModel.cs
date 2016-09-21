using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
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
        GridLength modGridLength;
        GridLength modpackGridLength;

        public ListCollectionView AvailableCulturesView { get; }

        public List<CultureEntry> AvailableCultures { get; }

        public ListCollectionView FactorioVersionsView { get; }

        public ObservableCollection<FactorioVersion> FactorioVersions { get; }

        public FactorioVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));

                    App.Instance.Settings.SelectedVersion = selectedVersion.Version;
                    App.Instance.Settings.Save();
                }
            }
        }

        public ListCollectionView ModsView { get; }

        public ObservableCollection<Mod> Mods { get; }

        public ListCollectionView ModpacksView { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        public GridLength ModGridLength
        {
            get { return modGridLength; }
            set
            {
                if (value != modGridLength)
                {
                    modGridLength = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModGridLength)));

                    App.Instance.Settings.ModGridLength = modGridLength;
                    App.Instance.Settings.Save();
                }
            }
            
        }

        public GridLength ModpackGridLength
        {
            get { return modpackGridLength; }
            set
            {
                if (value != modpackGridLength)
                {
                    modpackGridLength = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModpackGridLength)));

                    App.Instance.Settings.ModpackGridLength = modpackGridLength;
                    App.Instance.Settings.Save();
                }
            }

        }

        public RelayCommand CreateModpackCommand { get; }

        public RelayCommand StartGameCommand { get; }

        public RelayCommand OpenVersionManagerCommand { get; }

        public RelayCommand OpenSettingsCommand { get; }

        public RelayCommand BrowseFactorioWebsiteCommand { get; }

        public RelayCommand BrowseModWebsiteCommand { get; }

        public RelayCommand OpenAboutWindowCommand { get; }

        private MainViewModel()
        {
            AvailableCultures = App.Instance.GetAvailableCultures();
            AvailableCulturesView = (ListCollectionView)CollectionViewSource.GetDefaultView(AvailableCultures);
            AvailableCulturesView.CustomSort = new CultureEntrySorter();
            AvailableCultures.First(entry => string.Equals(entry.LanguageCode, App.Instance.Settings.SelectedLanguage, StringComparison.InvariantCultureIgnoreCase)).Select();

            FactorioVersions = new ObservableCollection<FactorioVersion>();
            FactorioVersion.GetInstalledVersions().ForEach(item => FactorioVersions.Add(item));
            FactorioVersionsView = (ListCollectionView)CollectionViewSource.GetDefaultView(FactorioVersions);
            FactorioVersionsView.CustomSort = new FactorioVersionSorter();

            Version version = App.Instance.Settings.SelectedVersion;
            if (version != null)
            {
                FactorioVersion factorioVersion = FactorioVersions.FirstOrDefault(item => item.Version == version);
                if (factorioVersion != null)
                {
                    selectedVersion = factorioVersion;
                }
                else
                {
                    App.Instance.Settings.SelectedVersion = default(Version);
                    App.Instance.Settings.Save();
                }
            }

            Mods = new ObservableCollection<Mod>();
            ModsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Mods);
            ModsView.CustomSort = new ModSorter();

            Modpacks = new ObservableCollection<Modpack>();
            ModpacksView = (ListCollectionView)CollectionViewSource.GetDefaultView(Modpacks);
            ModpacksView.CustomSort = new ModpackSorter();

            modGridLength = App.Instance.Settings.ModGridLength;
            modpackGridLength = App.Instance.Settings.ModpackGridLength;

            CreateModpackCommand = new RelayCommand(CreateNewModpack);
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
            Mods.Add(mod1);
            Mods.Add(mod2);
            Mods.Add(mod3);
            Mods.Add(mod4);
            Mods.Add(mod5);
            Modpack modpack1 = new Modpack("aaa", Modpacks, Application.Current.MainWindow);
            modpack1.ParentViews.Add(ModpacksView);
            Modpack modpack2 = new Modpack("bbb", Modpacks, Application.Current.MainWindow);
            modpack2.ParentViews.Add(ModpacksView);
            Modpack modpack3 = new Modpack("ccc", Modpacks, Application.Current.MainWindow);
            modpack3.ParentViews.Add(ModpacksView);
            //modpack.Mods.Add(mod1);
            //modpack.Mods.Add(mod2);
            Modpacks.Add(modpack1);
            Modpacks.Add(modpack2);
            Modpacks.Add(modpack3);
        }

        private void CreateNewModpack()
        {
            string newName = "NewModpack";
            int count = 0;
            bool contains = true;
            while (contains)
            {
                contains = false;
                count++;
                foreach (var item in Modpacks)
                {
                    if (item.Name == (newName + count))
                        contains = true;
                }
            }
            newName += count;

            Modpack modpack = new Modpack(newName, Modpacks, Window);
            modpack.ParentViews.Add(ModpacksView);
            Modpacks.Add(modpack);
        }

        private void StartGame()
        {
            Process.Start(SelectedVersion.ExecutablePath);
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

                DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory();
                if (!modDirectory.Exists) modDirectory.Create();
                foreach (var version in FactorioVersions)
                    version.CreateModDirectoryLink(true);
            }
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }
    }
}
