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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Export;
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

        string token;

        bool LoggedInWithToken => GlobalCredentials.LoggedIn && !string.IsNullOrEmpty(token);

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

                    App.Instance.Settings.SelectedVersion = selectedVersion.VersionString;
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
                }
            }

        }

        public RelayCommand DownloadModsCommand { get; }

        public RelayCommand AddModsFromFilesCommand { get; }

        public RelayCommand AddModFromFolderCommand { get; }

        public RelayCommand CreateModpackCommand { get; }

        public RelayCommand CreateLinkCommand { get; }

        public RelayCommand ExportModpacksCommand { get; }

        public RelayCommand ImportModpacksCommand { get; }

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


                var installedVersions = FactorioVersion.GetInstalledVersions();
                FactorioVersions = new ObservableCollection<FactorioVersion>(installedVersions) { FactorioVersion.Latest };
                FactorioVersion steamVersion;
                if (FactorioSteamVersion.TryLoad(out steamVersion)) FactorioVersions.Add(steamVersion);

                FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
                FactorioVersionsView.CustomSort = new FactorioVersionSorter();
                FactorioVersionsView.Filter = item => !((FactorioVersion)item).IsSpecialVersion;


                string versionString = App.Instance.Settings.SelectedVersion;
                if (!string.IsNullOrEmpty(versionString))
                {
                    FactorioVersion factorioVersion = FactorioVersions.FirstOrDefault(item => item.VersionString == versionString);
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

                Mods = new ModCollection();
                ModsView = (ListCollectionView)(new CollectionViewSource() { Source = Mods }).View;
                ModsView.CustomSort = new ModSorter();
                ModsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Mod.FactorioVersion)));
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
                CreateLinkCommand = new RelayCommand(CreateLink);

                ExportModpacksCommand = new RelayCommand(ExportModpacks);
                ImportModpacksCommand = new RelayCommand(async () => await ImportModpacks());

                StartGameCommand = new RelayCommand(StartGame, () => SelectedVersion != null);

                // 'Edit' menu
                OpenFactorioFolderCommand = new RelayCommand(() =>
                {
                    var factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                    if (!factorioDirectory.Exists) factorioDirectory.Create();
                    Process.Start(factorioDirectory.FullName);
                });
                OpenModFolderCommand = new RelayCommand(() =>
                {
                    var modDirectory = App.Instance.Settings.GetModDirectory();
                    if (!modDirectory.Exists) modDirectory.Create();
                    Process.Start(modDirectory.FullName);
                });
                OpenSavegameFolderCommand = new RelayCommand(() =>
                {
                    string savesPath = Path.Combine(App.Instance.AppDataPath, "saves");
                    if (!Directory.Exists(savesPath)) Directory.CreateDirectory(savesPath);
                    Process.Start(savesPath);
                });
                OpenScenarioFolderCommand = new RelayCommand(() =>
                {
                    string scenariosPath = Path.Combine(App.Instance.AppDataPath, "scenarios");
                    if (!Directory.Exists(scenariosPath)) Directory.CreateDirectory(scenariosPath);
                    Process.Start(scenariosPath);
                });

                UpdateModsCommand = new RelayCommand(async () => await UpdateMods());

                OpenVersionManagerCommand = new RelayCommand(OpenVersionManager);

                OpenSettingsCommand = new RelayCommand(async () => await OpenSettings());

                // 'Info' menu
                BrowseFactorioWebsiteCommand = new RelayCommand(() => Process.Start("https://www.factorio.com/"));
                BrowseModWebsiteCommand = new RelayCommand(() => Process.Start("https://mods.factorio.com/"));
                BrowseForumThreadCommand =  new RelayCommand(() => Process.Start("https://forums.factorio.com/viewtopic.php?f=137&t=33370"));

                UpdateCommand = new RelayCommand<bool>(async silent => await Update(silent), () => !updating);
                OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);


                // New ModMyFactory instance started.
                Program.NewInstanceStarted += NewInstanceStartedHandler;
            }
        }

        #region AddMods

        private async Task DownloadMods()
        {
            if (OnlineModsViewModel.Instance.Mods != null)
            {
                var modsWindow = new OnlineModsWindow() { Owner = Window };
                modsWindow.ShowDialog();
            }
            else
            {
                List<ModInfo> modInfos = await ModHelper.FetchMods(Window);

                if (modInfos != null)
                {
                    var modsWindow = new OnlineModsWindow() { Owner = Window };
                    modsWindow.ViewModel.Mods = modInfos;

                    modsWindow.ShowDialog();
                }
            }
        }

        private async Task AddModFromFile(FileInfo archiveFile, Window messageOwner)
        {
            Version factorioVersion;
            string name;
            if (Mod.ArchiveFileValid(archiveFile, out factorioVersion, out name))
            {
                if (!Mods.ContainsByFactorioVersion(name, factorioVersion))
                {
                    await Task.Run(() =>
                    {
                        var versionDirectory = App.Instance.Settings.GetModDirectory(factorioVersion);
                        if (!versionDirectory.Exists) versionDirectory.Create();

                        var modFilePath = Path.Combine(versionDirectory.FullName, archiveFile.Name);
                        archiveFile.MoveTo(modFilePath);
                    });

                    var mod = new ZippedMod(name, factorioVersion, archiveFile, Mods, Modpacks, Window);
                    Mods.Add(mod);
                }
                else
                {
                    switch (App.Instance.Settings.ManagerMode)
                    {
                        case ManagerMode.PerFactorioVersion:
                            MessageBox.Show(messageOwner,
                                string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), name, factorioVersion),
                                App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case ManagerMode.Global:
                            MessageBox.Show(messageOwner,
                                string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), name),
                                App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show(messageOwner,
                    string.Format(App.Instance.GetLocalizedMessage("InvalidModArchive", MessageType.Error), archiveFile.Name),
                    App.Instance.GetLocalizedMessageTitle("InvalidModArchive", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddModsFromFilesInner(string[] fileNames, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken, Window messageOwner)
        {
            int fileCount = fileNames.Length;
            int counter = 0;
            foreach (string fileName in fileNames)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                progress.Report(new Tuple<double, string>((double)counter / fileCount, Path.GetFileName(fileName)));

                var archiveFile = new FileInfo(fileName);
                await AddModFromFile(archiveFile, messageOwner);

                counter++;
            }

            progress.Report(new Tuple<double, string>(1, string.Empty));
        }

        private async Task AddModsFromFiles()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = App.Instance.GetLocalizedResourceString("ZipDescription") + @" (*.zip)|*.zip";
            bool? result = dialog.ShowDialog(Window);

            if (result.HasValue && result.Value)
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("ProcessingModsAction");

                var cancellationSource = new CancellationTokenSource();
                progressWindow.ViewModel.CanCancel = true;
                progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                var progress = new Progress<Tuple<double, string>>(info =>
                {
                    progressWindow.ViewModel.Progress = info.Item1;
                    progressWindow.ViewModel.ProgressDescription = info.Item2;
                });

                Task processModsTask = AddModsFromFilesInner(dialog.FileNames, progress, cancellationSource.Token, progressWindow);

                Task closeWindowTask = processModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await processModsTask;
                await closeWindowTask;
            }
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
                if (Mod.DirectoryValid(directory, out factorioVersion, out name))
                {
                    if (!Mods.ContainsByFactorioVersion(name, factorioVersion))
                    {
                        var versionDirectory = App.Instance.Settings.GetModDirectory(factorioVersion);
                        if (!versionDirectory.Exists) versionDirectory.Create();

                        var modDirectoryPath = Path.Combine(versionDirectory.FullName, directory.Name);
                        moveDirectoryTask = directory.MoveToAsync(modDirectoryPath);
                    }
                    else
                    {
                        switch (App.Instance.Settings.ManagerMode)
                        {
                            case ManagerMode.PerFactorioVersion:
                                MessageBox.Show(Window,
                                    string.Format(App.Instance.GetLocalizedMessage("ModExistsPerVersion", MessageType.Information), name, factorioVersion),
                                    App.Instance.GetLocalizedMessageTitle("ModExistsPerVersion", MessageType.Information),
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            case ManagerMode.Global:
                                MessageBox.Show(Window,
                                    string.Format(App.Instance.GetLocalizedMessage("ModExists", MessageType.Information), name),
                                    App.Instance.GetLocalizedMessageTitle("ModExists", MessageType.Information),
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                        }
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InvalidModFolder", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InvalidModFolder", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("ProcessingModAction");
                progressWindow.ViewModel.ProgressDescription = directory.Name;
                progressWindow.ViewModel.IsIndeterminate = true;

                moveDirectoryTask = moveDirectoryTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();
                await moveDirectoryTask;

                Mods.Add(new ExtractedMod(name, factorioVersion, directory, Mods, Modpacks, Window));
            }
        }

        #endregion

        private bool ContainsModpack(string name)
        {
            return Modpacks.Any(item => item.Name == name);
        }

        private void CreateNewModpack()
        {
            string name = App.Instance.GetLocalizedResourceString("NewModpackName");
            string newName = name;
            int counter = 0;
            while (ContainsModpack(newName))
            {
                counter++;
                newName = $"{name} {counter}";
            }

            Modpack modpack = new Modpack(newName, Modpacks, Window);
            modpack.ParentView = ModpacksView;
            Modpacks.Add(modpack);

            modpack.Editing = true;
            Window.ModpacksListBox.ScrollIntoView(modpack);
        }

        private void CreateLink()
        {
            var propertiesWindow = new LinkPropertiesWindow() { Owner = Window };
            bool? result = propertiesWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var dialog = new VistaSaveFileDialog();
                dialog.Filter = App.Instance.GetLocalizedResourceString("LnkDescription") + @" (*.lnk)|*.lnk";
                dialog.AddExtension = true;
                dialog.DefaultExt = ".lnk";
                result = dialog.ShowDialog(Window);
                if (result.HasValue && result.Value)
                {
                    string applicationPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                    string iconPath = Path.Combine(App.Instance.ApplicationDirectoryPath, "Factorio_Icon.ico");
                    string versionString = propertiesWindow.ViewModel.SelectedVersion.VersionString;
                    string modpackName = propertiesWindow.ViewModel.SelectedModpack?.Name;

                    string arguments = $"--factorio-version=\"{versionString}\"";
                    if (!string.IsNullOrEmpty(modpackName)) arguments += $" --modpack=\"{modpackName}\"";
                    ShellHelper.CreateShortcut(dialog.FileName, applicationPath, arguments, iconPath);
                }
            }
        }

        private void ExportModpacks()
        {
            var exportWindow = new ModpackExportWindow() { Owner = Window };
            bool? result = exportWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var dialog = new VistaSaveFileDialog();
                dialog.Filter = App.Instance.GetLocalizedResourceString("FmpDescription") + @" (*.fmp)|*.fmp";
                dialog.AddExtension = true;
                dialog.DefaultExt = ".fmp";
                result = dialog.ShowDialog(Window);
                if (result.HasValue && result.Value)
                {
                    ExportTemplate template = ModpackExport.CreateTemplate(
                        exportWindow.ModpackListBox.SelectedItems.Cast<Modpack>(),
                        exportWindow.ViewModel.IncludeVersionInfo);
                    ModpackExport.ExportTemplate(template, dialog.FileName);
                }
            }
        }

        private bool LogIn()
        {
            bool failed = false;
            if (LoggedInWithToken) // Credentials and token available.
            {
                // ToDo: check if token is still valid (does it actually expire?).
            }
            else if (GlobalCredentials.LoggedIn) // Only credentials available.
            {
                GlobalCredentials.LoggedIn = ModWebsite.LogIn(GlobalCredentials.Username, GlobalCredentials.Password, out token);
                failed = !GlobalCredentials.LoggedIn;
            }

            while (!LoggedInWithToken)
            {
                var loginWindow = new LoginWindow
                {
                    Owner = Window,
                    FailedText = { Visibility = failed ? Visibility.Visible : Visibility.Collapsed }
                };
                bool? loginResult = loginWindow.ShowDialog();
                if (loginResult == null || loginResult == false) return false;
                GlobalCredentials.Username = loginWindow.UsernameBox.Text;
                GlobalCredentials.Password = loginWindow.PasswordBox.SecurePassword;

                GlobalCredentials.LoggedIn = ModWebsite.LogIn(GlobalCredentials.Username, GlobalCredentials.Password, out token);
                failed = !GlobalCredentials.LoggedIn;
            }

            return true;
        }

        #region ModpackImport

        private ModRelease GetNewestRelease(ExtendedModInfo info)
        {
            return info.Releases.MaxBy(release => release.Version, new VersionComparer());
        }

        private async Task<Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>> GetModsToDownload(ExportTemplate template)
        {
            var toDownload = new List<ModRelease>();
            var conflicting = new List<Tuple<Mod, ModExportTemplate>>();

            if (template.IncludesVersionInfo)
            {
                foreach (var modTemplate in template.Mods)
                {
                    if (!Mods.Contains(modTemplate.Name, modTemplate.Version))
                    {
                        Mod[] mods = Mods.Find(modTemplate.Name);

                        ExtendedModInfo modInfo = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
                        ModRelease release = modInfo.Releases.FirstOrDefault(r => r.Version == modTemplate.Version);

                        if (release != null)
                        {
                            if (mods.Length == 0)
                            {
                                toDownload.Add(release);
                            }
                            else
                            {
                                if ((App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion) &&
                                    mods.All(mod => mod.FactorioVersion != release.FactorioVersion))
                                {
                                    toDownload.Add(release);
                                }
                                else
                                {
                                    conflicting.Add(new Tuple<Mod, ModExportTemplate>(mods[0], modTemplate));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var modTemplate in template.Mods)
                {
                    Mod[] mods = Mods.Find(modTemplate.Name);

                    if (mods.Length == 0)
                    {
                        ExtendedModInfo modInfo = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
                        ModRelease newestRelease = GetNewestRelease(modInfo);
                        toDownload.Add(newestRelease);
                    }
                    else
                    {
                        ExtendedModInfo modInfo = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
                        ModRelease newestRelease = GetNewestRelease(modInfo);

                        if (!Mods.Contains(modTemplate.Name, newestRelease.Version))
                        {
                            if ((App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion) &&
                            mods.All(mod => mod.FactorioVersion != newestRelease.FactorioVersion))
                            {
                                toDownload.Add(newestRelease);
                            }
                            else
                            {
                                conflicting.Add(new Tuple<Mod, ModExportTemplate>(mods[0], modTemplate));
                            }
                        }
                    }
                }
            }

            return new Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>(toDownload, conflicting);
        }

        private async Task DownloadModAsyncInner(ModRelease modRelease, IProgress<double> progress, CancellationToken cancellationToken)
        {
            Mod mod = await ModWebsite.DownloadReleaseAsync(modRelease, GlobalCredentials.Username, token, progress, cancellationToken, Mods, Modpacks, Window);
            if (!cancellationToken.IsCancellationRequested && (mod != null)) Mods.Add(mod);
        }

        private async Task DownloadModsAsyncInner(List<ModRelease> modReleases, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            int modCount = modReleases.Count;
            double baseProgressValue = 0;
            foreach (var release in modReleases)
            {
                if (cancellationToken.IsCancellationRequested) return;

                double modProgressValue = 0;
                var modProgress = new Progress<double>(value =>
                {
                    modProgressValue = value / modCount;
                    progress.Report(new Tuple<double, string>(baseProgressValue + modProgressValue, release.FileName));
                });

                await DownloadModAsyncInner(release, modProgress, cancellationToken);

                baseProgressValue += modProgressValue;
            }
        }

        private async Task DownloadModsAsync(List<ModRelease> modReleases)
        {
            if (LogIn())
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

                var progress = new Progress<Tuple<double, string>>(info =>
                {
                    progressWindow.ViewModel.Progress = info.Item1;
                    progressWindow.ViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), info.Item2);
                });

                var cancellationSource = new CancellationTokenSource();
                progressWindow.ViewModel.CanCancel = true;
                progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                Task updateTask = DownloadModsAsyncInner(modReleases, progress, cancellationSource.Token);
                Task closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await updateTask;
                await closeWindowTask;
            }
        }

        private async Task ImportModpackFile(FileInfo modpackFile)
        {
            ExportTemplate template = ModpackExport.ImportTemplate(modpackFile);

            var toDownloadResult = await GetModsToDownload(template);
            List<ModRelease> toDownload = toDownloadResult.Item1;
            List<Tuple<Mod, ModExportTemplate>> conflicting = toDownloadResult.Item2;

            if (conflicting.Count > 0)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("HasConflicts", MessageType.Warning) + "\n"
                    + string.Join("\n", conflicting.Select(conflict => $"{conflict.Item1.Name} ({conflict.Item1.Version}) <-> {conflict.Item2.Name}"
                    + (template.IncludesVersionInfo ? $" ({conflict.Item2.Version})" : " (latest)"))),
                    App.Instance.GetLocalizedMessageTitle("HasConflicts", MessageType.Warning),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (toDownload.Count > 0)
                await DownloadModsAsync(toDownload);

            foreach (var modpackTemplate in template.Modpacks)
            {
                var existingModpack = Modpacks.FirstOrDefault(item => item.Name == modpackTemplate.Name);

                if (existingModpack == null)
                {
                    Modpack modpack = new Modpack(modpackTemplate.Name, Modpacks, Window);
                    modpack.ParentView = ModpacksView;

                    foreach (var modTemplate in modpackTemplate.Mods)
                    {
                        if (template.IncludesVersionInfo)
                        {
                            Mod mod = Mods.Find(modTemplate.Name, modTemplate.Version);
                            if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                        }
                        else
                        {
                            Mod mod = Mods.Find(modTemplate.Name).MaxBy(item => item.Version, new VersionComparer());
                            if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                        }
                    }

                    Modpacks.Add(modpack);
                }
                else
                {
                    foreach (var modTemplate in modpackTemplate.Mods)
                    {
                        if (template.IncludesVersionInfo)
                        {
                            Mod mod = Mods.Find(modTemplate.Name, modTemplate.Version);
                            if ((mod != null) && !existingModpack.Contains(mod)) existingModpack.Mods.Add(new ModReference(mod, existingModpack));
                        }
                        else
                        {
                            Mod mod = Mods.Find(modTemplate.Name).MaxBy(item => item.Version, new VersionComparer());
                            if ((mod != null) && !existingModpack.Contains(mod)) existingModpack.Mods.Add(new ModReference(mod, existingModpack));
                        }
                    }
                }
            }
            foreach (var modpackTemplate in template.Modpacks)
            {
                var existingModpack = Modpacks.FirstOrDefault(item => item.Name == modpackTemplate.Name);

                if (existingModpack != null)
                {
                    foreach (var innerTemplate in modpackTemplate.Modpacks)
                    {
                        Modpack modpack = Modpacks.FirstOrDefault(item => item.Name == innerTemplate);
                        if ((modpack != null) && !existingModpack.Contains(modpack)) existingModpack.Mods.Add(new ModpackReference(modpack, existingModpack));
                    }
                }
            }
        }

        private async Task ImportModpacksInner(IEnumerable<FileInfo> modpackFiles)
        {
            foreach (FileInfo file in modpackFiles)
                await ImportModpackFile(file);
        }

        private async Task ImportModpacks()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = App.Instance.GetLocalizedResourceString("FmpDescription") + @" (*.fmp)|*.fmp";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var fileList = new List<FileInfo>();
                foreach (var fileName in dialog.FileNames)
                {
                    var file = new FileInfo(fileName);
                    fileList.Add(file);
                }
                
                if (fileList.Count > 0)
                    await ImportModpacksInner(fileList);
            }
        }

        #endregion

        private void StartGame()
        {
            Process.Start(SelectedVersion.ExecutablePath);
        }

        #region ModUpdate

        private ModRelease GetNewestRelease(ExtendedModInfo info, Mod current)
        {
            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                return info.Releases.Where(release => release.FactorioVersion == current.FactorioVersion)
                    .MaxBy(release => release.Version, new VersionComparer());
            }
            else
            {
                return info.Releases.MaxBy(release => release.Version, new VersionComparer());
            }
        }

        private async Task<List<ModUpdateInfo>> GetModUpdatesAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var modUpdates = new List<ModUpdateInfo>();

            int modCount = Mods.Count;
            int modIndex = 0;
            foreach (var mod in Mods)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                progress.Report(new Tuple<double, string>((double)modIndex / modCount, mod.Title));

                ExtendedModInfo extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod);
                if (extendedInfo != null)
                {
                    ModRelease newestRelease = GetNewestRelease(extendedInfo, mod);
                    if ((newestRelease != null) && (newestRelease.Version > mod.Version))
                    {
                        modUpdates.Add(new ModUpdateInfo(mod.Title, mod.Name, mod.Version, newestRelease.Version, mod, newestRelease));
                    }
                }

                modIndex++;
            }

            return modUpdates;
        }

        private async Task UpdateModAsyncInner(ModUpdateInfo modUpdate, IProgress<double> progress, CancellationToken cancellationToken)
        {
            FileInfo modFile = await ModWebsite.UpdateReleaseAsync(modUpdate.NewestRelease, GlobalCredentials.Username, token, progress, cancellationToken);
            var zippedMod = modUpdate.Mod as ZippedMod;
            var extractedMod = modUpdate.Mod as ExtractedMod;
            if (zippedMod != null)
            {
                if (zippedMod.FactorioVersion == modUpdate.NewestRelease.FactorioVersion)
                {
                    zippedMod.Update(modFile);
                }
                else
                {
                    var newMod = new ZippedMod(zippedMod.Name, modUpdate.NewestRelease.FactorioVersion, modFile, Mods, Modpacks, Window);
                    Mods.Add(newMod);
                    foreach (var modpack in Modpacks)
                    {
                        ModReference reference;
                        if (modpack.Contains(zippedMod, out reference))
                        {
                            modpack.Mods.Remove(reference);
                            modpack.Mods.Add(new ModReference(newMod, modpack));
                        }
                    }
                    zippedMod.File.Delete();
                    Mods.Remove(zippedMod);

                    ModpackTemplateList.Instance.Update(Modpacks);
                    ModpackTemplateList.Instance.Save();
                }
            }
            else if (extractedMod != null)
            {
                DirectoryInfo modDirectory = await Task.Run<DirectoryInfo>(() =>
                {
                    DirectoryInfo modsDirectory = App.Instance.Settings.GetModDirectory(modUpdate.NewestRelease.FactorioVersion);
                    ZipFile.ExtractToDirectory(modFile.FullName, modsDirectory.FullName);
                    modFile.Delete();

                    return new DirectoryInfo(Path.Combine(modsDirectory.FullName, modFile.NameWithoutExtension()));
                });

                if (extractedMod.FactorioVersion == modUpdate.NewestRelease.FactorioVersion)
                {
                    extractedMod.Update(modDirectory);
                }
                else
                {
                    var newMod = new ExtractedMod(extractedMod.Name, modUpdate.NewestRelease.FactorioVersion, modDirectory, Mods, Modpacks, Window);
                    Mods.Add(newMod);
                    foreach (var modpack in Modpacks)
                    {
                        ModReference reference;
                        if (modpack.Contains(extractedMod, out reference))
                        {
                            modpack.Mods.Remove(reference);
                            modpack.Mods.Add(new ModReference(newMod, modpack));
                        }
                    }
                    extractedMod.Directory.Delete(true);
                    Mods.Remove(extractedMod);
                }
            }
        }

        private async Task UpdateModsAsyncInner(List<ModUpdateInfo> modUpdates, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            int modCount = modUpdates.Count(item => item.IsSelected);
            double baseProgressValue = 0;
            foreach (var modUpdate in modUpdates)
            {
                if (cancellationToken.IsCancellationRequested) return;

                if (modUpdate.IsSelected)
                {
                    double modProgressValue = 0;
                    var modProgress = new Progress<double>(value =>
                    {
                        modProgressValue = value / modCount;
                        progress.Report(new Tuple<double, string>(baseProgressValue + modProgressValue, modUpdate.Title));
                    });

                    await UpdateModAsyncInner(modUpdate, modProgress, cancellationToken);

                    baseProgressValue += modProgressValue;
                }
            }
        }

        private async Task UpdateMods()
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("SearchingForUpdatesAction");

            var progress = new Progress<Tuple<double, string>>(info =>
            {
                progressWindow.ViewModel.Progress = info.Item1;
                progressWindow.ViewModel.ProgressDescription = info.Item2;
            });

            var cancellationSource = new CancellationTokenSource();
            progressWindow.ViewModel.CanCancel = true;
            progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            Task<List<ModUpdateInfo>> searchForUpdatesTask = GetModUpdatesAsync(progress, cancellationSource.Token);
            Task closeWindowTask = searchForUpdatesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
            progressWindow.ShowDialog();

            List<ModUpdateInfo> modUpdates = await searchForUpdatesTask;
            await closeWindowTask;

            if (!cancellationSource.IsCancellationRequested)
            {
                if (modUpdates.Count > 0)
                {
                    var updateWindow = new ModUpdateWindow() { Owner = Window };
                    updateWindow.ViewModel.ModsToUpdate = modUpdates;
                    bool? result = updateWindow.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        if (LogIn())
                        {
                            progressWindow = new ProgressWindow() { Owner = Window };
                            progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("UpdatingModsAction");

                            progress = new Progress<Tuple<double, string>>(info =>
                            {
                                progressWindow.ViewModel.Progress = info.Item1;
                                progressWindow.ViewModel.ProgressDescription = info.Item2;
                            });

                            cancellationSource = new CancellationTokenSource();
                            progressWindow.ViewModel.CanCancel = true;
                            progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                            Task updateTask = UpdateModsAsyncInner(modUpdates, progress, cancellationSource.Token);
                            closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                            progressWindow.ShowDialog();

                            await updateTask;
                            await closeWindowTask;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("NoModUpdates", MessageType.Information),
                        App.Instance.GetLocalizedMessageTitle("NoModUpdates", MessageType.Information),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        private void OpenVersionManager()
        {
            var versionManagementWindow = new VersionManagementWindow() { Owner = Window };
            versionManagementWindow.ShowDialog();
        }

        #region Settings

        private async Task MoveDirectories(DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory)
        {
            bool moveFactorioDirectory = !newFactorioDirectory.DirectoryEquals(oldFactorioDirectory);
            bool moveModDirectory = !newModDirectory.DirectoryEquals(oldModDirectory);
            if (oldFactorioDirectory.Exists && moveFactorioDirectory)
            {
                foreach (var version in FactorioVersions)
                {
                    if (!version.IsSpecialVersion)
                    {
                        version.DeleteLinks();

                        DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                        var versionDirectory = new DirectoryInfo(Path.Combine(factorioDirectory.FullName, version.Version.ToString(3)));
                        version.UpdateDirectory(versionDirectory);
                    }
                }
                await oldFactorioDirectory.MoveToAsync(newFactorioDirectory.FullName);
            }
            if (oldModDirectory.Exists && moveModDirectory)
            {
                await oldModDirectory.MoveToAsync(newModDirectory.FullName);
            }

            foreach (var version in FactorioVersions)
            {
                if (!version.IsSpecialVersion)
                    version.CreateLinks(true);
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
                progressWindow.ViewModel.ActionName = App.Instance.GetLocalizedResourceString("MovingDirectoriesAction");
                progressWindow.ViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("MovingFilesDescription");
                progressWindow.ViewModel.IsIndeterminate = true;

                Task moveDirectoriesTask = MoveDirectories(oldFactorioDirectory, oldModDirectory, newFactorioDirectory, newModDirectory);

                Task closeWindowTask = moveDirectoriesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await moveDirectoriesTask;
                await closeWindowTask;
            }
        }

        #endregion

        private async Task Update(bool silent)
        {
            updating = true;

            UpdateSearchResult result = await App.Instance.SearchForUpdateAsync();
            if (result.UpdateAvailable)
            {
                string currentVersionString = App.Instance.AssemblyVersion.ToString(3);
                string newVersionString = result.Version.ToString(3);
                if (MessageBox.Show(Window,
                        string.Format(App.Instance.GetLocalizedMessage("Update", MessageType.Question), currentVersionString, newVersionString),
                        App.Instance.GetLocalizedMessageTitle("Update", MessageType.Question),
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Process.Start(result.UpdateUrl);
                }
            }
            else if (!silent)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("NoUpdate", MessageType.Information),
                    App.Instance.GetLocalizedMessageTitle("NoUpdate", MessageType.Information),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            updating = false;
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }

        private async void NewInstanceStartedHandler(object sender, InstanceStartedEventArgs e)
        {
            await Window.Dispatcher.InvokeAsync(async () => await OnNewInstanceStarted(e.CommandLine));
        }

        private async Task OnNewInstanceStarted(CommandLine commandLine)
        {
            Window.Activate();

            var fileList = new List<FileInfo>();
            foreach (string argument in commandLine.Arguments)
            {
                if (argument.EndsWith(".fmp") && File.Exists(argument))
                {
                    var file = new FileInfo(argument);
                    fileList.Add(file);
                }
            }

            if (fileList.Count > 0)
                await ImportModpacksInner(fileList);
        }
    }
}
