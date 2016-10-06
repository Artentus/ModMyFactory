using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Helpers;
using ModMyFactory.Lang;
using ModMyFactory.MVVM;
using Ookii.Dialogs.Wpf;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.ViewModels
{
    sealed class MainViewModel : ViewModelBase<MainWindow>
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        FactorioVersion selectedVersion;
        string modsFilter;
        string modpacksFilter;
        GridLength modGridLength;
        GridLength modpackGridLength;
        bool updating;

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

        public string ModsFilter
        {
            get { return modsFilter; }
            set
            {
                if (value != modsFilter)
                {
                    modsFilter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsFilter)));

                    ModsView.Refresh();
                }
            }
        }

        public ModCollection Mods { get; }

        public ListCollectionView ModpacksView { get; }

        public string ModpacksFilter
        {
            get { return modpacksFilter; }
            set
            {
                if (value != modpacksFilter)
                {
                    modpacksFilter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModpacksFilter)));

                    ModpacksView.Refresh();
                }
            }
        }

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

        public RelayCommand DownloadModsCommand { get; }

        public RelayCommand AddModsFromFilesCommand { get; }

        public RelayCommand AddModFromFolderCommand { get; }

        public RelayCommand CreateModpackCommand { get; }

        public RelayCommand ExportLinkCommand { get; }

        public RelayCommand StartGameCommand { get; }

        public RelayCommand OpenFactorioFolderCommand { get; }

        public RelayCommand OpenModFolderCommand { get; }

        public RelayCommand OpenSavegameFolderCommand { get; }

        public RelayCommand OpenScenarioFolderCommand { get; }

        public RelayCommand UpdateModsCommand { get; }

        public RelayCommand OpenVersionManagerCommand { get; }

        public RelayCommand OpenSettingsCommand { get; }

        public RelayCommand BrowseFactorioWebsiteCommand { get; }

        public RelayCommand BrowseModWebsiteCommand { get; }

        public RelayCommand BrowseForumThreadCommand { get; }

        public RelayCommand<bool> UpdateCommand { get; }

        public RelayCommand OpenAboutWindowCommand { get; }

        private bool ModFilter(object item)
        {
            Mod mod = item as Mod;
            if (mod == null) return false;

            if (string.IsNullOrWhiteSpace(modsFilter)) return true;
            return Thread.CurrentThread.CurrentUICulture.CompareInfo.IndexOf(mod.Title, modsFilter, CompareOptions.IgnoreCase) >= 0;
        }

        private bool ModpackFilter(object item)
        {
            Modpack modpack = item as Modpack;
            if (modpack == null) return false;

            if (string.IsNullOrWhiteSpace(modpacksFilter)) return true;
            return Thread.CurrentThread.CurrentUICulture.CompareInfo.IndexOf(modpack.Name, modpacksFilter, CompareOptions.IgnoreCase) >= 0;
        }

        private MainViewModel()
        {
            if (!App.IsInDesignMode) // Make view model designer friendly.
            {
                AvailableCultures = App.Instance.GetAvailableCultures();
                AvailableCulturesView = (ListCollectionView)CollectionViewSource.GetDefaultView(AvailableCultures);
                AvailableCulturesView.CustomSort = new CultureEntrySorter();
                AvailableCultures.First(entry =>
                    string.Equals(entry.LanguageCode, App.Instance.Settings.SelectedLanguage, StringComparison.InvariantCultureIgnoreCase)).Select();

                FactorioVersions = new ObservableCollection<FactorioVersion>() { FactorioVersion.Latest };
                FactorioVersion.GetInstalledVersions().ForEach(item => FactorioVersions.Add(item));
                FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
                FactorioVersionsView.CustomSort = new FactorioVersionSorter();
                FactorioVersionsView.Filter = item => !((FactorioVersion)item).IsSpecialVersion;

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

                Mods = new ModCollection();
                ModsView = (ListCollectionView)(new CollectionViewSource() { Source = Mods }).View;
                ModsView.CustomSort = new ModSorter();
                ModsView.GroupDescriptions.Add(new PropertyGroupDescription("FactorioVersion"));
                ModsView.Filter = ModFilter;

                Modpacks = new ObservableCollection<Modpack>();
                ModpacksView = (ListCollectionView)(new CollectionViewSource() { Source = Modpacks }).View;
                ModpacksView.CustomSort = new ModpackSorter();
                ModpacksView.Filter = ModpackFilter;

                Mod.LoadMods(Mods, Modpacks, Application.Current.MainWindow);
                ModpackTemplateList.Instance.PopulateModpackList(Mods, Modpacks, ModpacksView, Application.Current.MainWindow);
                Modpacks.CollectionChanged += (sender, e) =>
                {
                    ModpackTemplateList.Instance.Update(Modpacks);
                    ModpackTemplateList.Instance.Save();
                };

                modGridLength = App.Instance.Settings.ModGridLength;
                modpackGridLength = App.Instance.Settings.ModpackGridLength;


                // 'File' menu
                DownloadModsCommand = new RelayCommand(async () => await DownloadMods());
                AddModsFromFilesCommand = new RelayCommand(async () => await AddModsFromFiles());
                AddModFromFolderCommand = new RelayCommand(async () => await AddModFromFolder());
                CreateModpackCommand = new RelayCommand(CreateNewModpack);
                ExportLinkCommand = new RelayCommand(CreateLink);

                StartGameCommand = new RelayCommand(StartGame, () => SelectedVersion != null);

                // 'Edit' menu
                OpenFactorioFolderCommand = new RelayCommand(() => Process.Start(App.Instance.Settings.GetFactorioDirectory().FullName));
                OpenModFolderCommand = new RelayCommand(() => Process.Start(App.Instance.Settings.GetModDirectory().FullName));
                OpenSavegameFolderCommand = new RelayCommand(() => Process.Start(Path.Combine(App.Instance.AppDataPath, "saves")));
                OpenScenarioFolderCommand = new RelayCommand(() => Process.Start(Path.Combine(App.Instance.AppDataPath, "scenarios")));

                UpdateModsCommand = new RelayCommand(async () => await UpdateMods());

                OpenVersionManagerCommand = new RelayCommand(OpenVersionManager);

                OpenSettingsCommand = new RelayCommand(async () => await OpenSettings());

                // 'Info' menu
                BrowseFactorioWebsiteCommand = new RelayCommand(() => Process.Start("https://www.factorio.com/"));
                BrowseModWebsiteCommand = new RelayCommand(() => Process.Start("https://mods.factorio.com/"));
                BrowseForumThreadCommand =  new RelayCommand(() => Process.Start("https://forums.factorio.com/viewtopic.php?f=137&t=33370"));

                UpdateCommand = new RelayCommand<bool>(async silent => await Update(silent), () => !updating);
                OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);
            }
        }

        private async Task DownloadMods()
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            progressWindow.ViewModel.ActionName = "Fetching mods";
            progressWindow.ViewModel.ProgressDescription = "Parsing page 1.";
            progressWindow.ViewModel.CanCancel = true;

            var progress = new Progress<Tuple<double, string>>(value =>
            {
                progressWindow.ViewModel.Progress = value.Item1;
                progressWindow.ViewModel.ProgressDescription = value.Item2;
            });
            var cancellationSource = new CancellationTokenSource();
            progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            Task<List<ModInfo>> fetchModsTask = ModWebsite.GetModsAsync(progress, cancellationSource.Token);

            Task closeWindowTask = fetchModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
            progressWindow.ShowDialog();

            List<ModInfo> modInfos = await fetchModsTask;
            await closeWindowTask;

            if (!cancellationSource.IsCancellationRequested)
            {
                var modsWindow = new OnlineModsWindow() { Owner = Window };
                modsWindow.ViewModel.Mods = modInfos;
                modsWindow.ViewModel.InstalledMods = Mods;

                modsWindow.ShowDialog();
            }
        }

        private bool TryParseInfoFile(Stream stream, out Version version, out string name)
        {
            version = null;
            name = null;

            using (var reader = new StreamReader(stream))
            {
                // Factorio version
                string content = reader.ReadToEnd();
                MatchCollection matches = Regex.Matches(content, "\"factorio_version\" *: *\"(?<version>[0-9]+\\.[0-9]+(\\.[0-9]+)?)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count != 1) return false;

                string versionString = matches[0].Groups["version"].Value;
                version = Version.Parse(versionString);
                version = new Version(version.Major, version.Minor);

                // Name
                matches = Regex.Matches(content, "\"name\" *: *\"(?<name>.*)\"",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                if (matches.Count != 1) return false;

                name = matches[0].Groups["name"].Value;

                return true;
            }
        }

        private bool ArchiveFileValid(FileInfo archiveFile, out Version validVersion, out string validName)
        {
            validVersion = null;
            validName = null;

            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name == "info.json")
                    {
                        using (Stream stream = entry.Open())
                        {
                            if (TryParseInfoFile(stream, out validVersion, out validName)) return true;
                        }
                    }
                }
            }

            return false;
        }

        private async Task AddModsFromFiles()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "ZIP-Archives (*.zip)|*.zip";
            bool? result = dialog.ShowDialog(Window);

            if (result.HasValue && result.Value)
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = "Processing mods";

                IProgress<Tuple<double, string>> progress1 = new Progress<Tuple<double, string>>(info =>
                {
                    progressWindow.ViewModel.Progress = info.Item1;
                    progressWindow.ViewModel.ProgressDescription = info.Item2;
                });
                IProgress<Tuple<string, Version, FileInfo>> progress2 = new Progress<Tuple<string, Version, FileInfo>>(info =>
                {
                    var mod = new ZippedMod(info.Item1, info.Item2, info.Item3, Mods, Modpacks, Window);
                    Mods.Add(mod);
                });

                Task processModsTask = Task.Run(() =>
                {
                    int fileCount = dialog.FileNames.Length;
                    int counter = 0;
                    foreach (string fileName in dialog.FileNames)
                    {
                        var archiveFile = new FileInfo(fileName);
                        Version factorioVersion;
                        string name;

                        progress1.Report(new Tuple<double, string>((double)counter / fileCount, archiveFile.Name));

                        if (ArchiveFileValid(archiveFile, out factorioVersion, out name))
                        {
                            if (!Mods.ContainsByFactorioVersion(name, factorioVersion))
                            {
                                var versionDirectory = App.Instance.Settings.GetModDirectory(factorioVersion);
                                if (!versionDirectory.Exists) versionDirectory.Create();

                                var modFilePath = Path.Combine(versionDirectory.FullName, archiveFile.Name);
                                archiveFile.MoveTo(modFilePath);
                                progress2.Report(new Tuple<string, Version, FileInfo>(name, factorioVersion, archiveFile));
                            }
                            else
                            {
                                // ToDo: Message for mod existing.
                            }
                        }
                        else
                        {
                            // ToDo: Message for invalid mod file.
                        }

                        counter++;
                    }

                    progress1.Report(new Tuple<double, string>(1, string.Empty));
                });

                Task closeWindowTask = processModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await processModsTask;
                await closeWindowTask;
            }
        }

        private bool DirectoryValid(DirectoryInfo directory, out Version validVersion, out string validName)
        {
            validVersion = null;
            validName = null;

            var file = directory.EnumerateFiles("info.json").FirstOrDefault();
            if (file != null)
            {
                using (Stream stream = file.OpenRead())
                {
                    if (TryParseInfoFile(stream, out validVersion, out validName)) return true;
                }
            }

            return false;
        }

        private async Task AddModFromFolder()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog(Window);

            if (result.HasValue && result.Value)
            {
                var directory = new DirectoryInfo(dialog.SelectedPath);

                Task moveDirectoryTask;
                Version factorioVersion;
                string name;
                if (DirectoryValid(directory, out factorioVersion, out name))
                {
                    if (Mods.ContainsByFactorioVersion(name, factorioVersion))
                    {
                        MessageBox.Show(Window, $"The mod '{name}' for Factorio {factorioVersion} already exists!",
                            "Error adding mod", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var versionDirectory = App.Instance.Settings.GetModDirectory(factorioVersion);
                    if (!versionDirectory.Exists) versionDirectory.Create();

                    var modDirectoryPath = Path.Combine(versionDirectory.FullName, directory.Name);
                    moveDirectoryTask = directory.MoveToAsync(modDirectoryPath);
                }
                else
                {
                    MessageBox.Show(Window, "The selected directory does not contain a valid mod.", "Error adding mod",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = "Processing mod";
                progressWindow.ViewModel.ProgressDescription = directory.Name;
                progressWindow.ViewModel.IsIndeterminate = true;

                moveDirectoryTask = moveDirectoryTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();
                await moveDirectoryTask;

                Mods.Add(new ExtractedMod(name, factorioVersion, directory, Mods, Modpacks, Window));
            }
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
            modpack.ParentView = ModpacksView;
            Modpacks.Add(modpack);

            modpack.Editing = true;
            Window.ModpacksListBox.ScrollIntoView(modpack);
        }

        private void CreateLink()
        {
            var propertiesWindow = new LinkPropertiesWindow();
            bool? result = propertiesWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var dialog = new VistaSaveFileDialog();
                dialog.Filter = "Shortcuts (*.lnk)|*.lnk";
                dialog.AddExtension = true;
                dialog.DefaultExt = ".lnk";
                result = dialog.ShowDialog(Window);
                if (result.HasValue && result.Value)
                {
                    string applicationPath = Assembly.GetExecutingAssembly().Location;
                    string iconPath = Path.Combine(Environment.CurrentDirectory, "Factorio_Icon.ico");
                    string versionString = propertiesWindow.ViewModel.SelectedVersion.IsSpecialVersion
                        ? propertiesWindow.ViewModel.SelectedVersion.DisplayName
                        : propertiesWindow.ViewModel.SelectedVersion.Version.ToString(3);
                    string modpackName = propertiesWindow.ViewModel.SelectedModpack?.Name;

                    string arguments = $"--factorio-version={versionString}";
                    if (!string.IsNullOrEmpty(modpackName)) arguments += $" --modpack=\"{modpackName}\"";
                    ShellHelper.CreateShortcut(dialog.FileName, applicationPath, arguments, iconPath);
                }
            }
        }

        private void StartGame()
        {
            Process.Start(SelectedVersion.ExecutablePath);
        }

        private async Task UpdateMods()
        {
            // ToDo: update mods.
        }

        private void OpenVersionManager()
        {
            var versionManagementWindow = new VersionManagementWindow() { Owner = Window };
            versionManagementWindow.ShowDialog();
        }

        private async Task MoveDirectories(DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory)
        {
            bool moveFactorioDirectory = !newFactorioDirectory.DirectoryEquals(oldFactorioDirectory);
            bool moveModDirectory = !newModDirectory.DirectoryEquals(oldModDirectory);
            if (moveFactorioDirectory)
            {
                foreach (var version in FactorioVersions)
                    version.DeleteLinks();
                await oldFactorioDirectory.MoveToAsync(newFactorioDirectory.FullName);
            }
            if (moveModDirectory)
            {
                await oldModDirectory.MoveToAsync(newModDirectory.FullName);
            }

            if (moveFactorioDirectory)
            {
                foreach (var version in FactorioVersions)
                    version.CreateLinks(false);
            }
            else if (moveModDirectory)
            {
                foreach (var version in FactorioVersions)
                    version.CreateModDirectoryLink(true);
            }
        }

        private async Task OpenSettings()
        {
            var settingsWindow = new SettingsWindow() { Owner = Window };
            settingsWindow.ViewModel.Reset();

            bool? result = settingsWindow.ShowDialog();
            if (result != null && result.Value)
            {
                DirectoryInfo oldFactorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                DirectoryInfo oldModDirectory = App.Instance.Settings.GetModDirectory();

                if (settingsWindow.ViewModel.ManagerModeIsPerFactorioVersion)
                {
                    App.Instance.Settings.ManagerMode = ManagerMode.PerFactorioVersion;
                }
                else if (settingsWindow.ViewModel.ManagerModeIsGlobal)
                {
                    App.Instance.Settings.ManagerMode = ManagerMode.Global;
                }
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

                DirectoryInfo newFactorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                DirectoryInfo newModDirectory = App.Instance.Settings.GetModDirectory();

                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = "Moving directories";
                progressWindow.ViewModel.ProgressDescription = "Moving files...";
                progressWindow.ViewModel.IsIndeterminate = true;

                Task moveDirectoriesTask = MoveDirectories(oldFactorioDirectory, oldModDirectory, newFactorioDirectory, newModDirectory);

                Task closeWindowTask = moveDirectoriesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await moveDirectoriesTask;
                await closeWindowTask;
            }
        }

        private async Task Update(bool silent)
        {
            updating = true;

            UpdateSearchResult result = await App.Instance.SearchForUpdateAsync();
            if (result.UpdateAvailable)
            {
                string currentVersionString = App.Instance.AssemblyVersion.ToString(3);
                string newVersionString = result.Version.ToString(3);
                if (MessageBox.Show(Window,
                        $"You are running an old version of ModMyFactory (current version is {currentVersionString}, newest version is {newVersionString}).\nDo you want to download the newest version?",
                        "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Process.Start(result.UpdateUrl);
                }
            }
            else if (!silent)
            {
                MessageBox.Show(Window, "You are running the newest version of ModMyFactory.", "No update available",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            updating = false;
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }
    }
}
