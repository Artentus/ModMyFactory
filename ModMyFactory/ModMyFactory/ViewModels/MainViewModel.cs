using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Export;
using ModMyFactory.Helpers;
using ModMyFactory.Lang;
using Ookii.Dialogs.Wpf;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class MainViewModel : ViewModelBase
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        public MainWindow Window => (MainWindow)View;

        FactorioVersion selectedVersion;
        string modsFilter;
        string modpacksFilter;
        GridLength modGridLength;
        GridLength modpackGridLength;
        bool? allModsActive;
        bool? allModpacksActive;
        bool allModsSelectedChanging;
        bool allModpacksSelectedChanging;
        bool updating;

        public ListCollectionView AvailableCulturesView { get; }

        public List<CultureEntry> AvailableCultures { get; }

        public ListCollectionView FactorioVersionsView { get; }

        public ObservableCollection<FactorioVersion> FactorioVersions { get; }

        private void SelectedVersionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FactorioVersion.VersionString))
            {
                App.Instance.Settings.SelectedVersion = selectedVersion.VersionString;
                App.Instance.Settings.Save();
            }
        }

        private void SetSelectedVersionInternal(FactorioVersion value)
        {
            if (selectedVersion != null)
                selectedVersion.PropertyChanged -= SelectedVersionPropertyChangedHandler;

            selectedVersion = value;
            if (selectedVersion != null)
                selectedVersion.PropertyChanged += SelectedVersionPropertyChangedHandler;
        }

        public FactorioVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    SetSelectedVersionInternal(value);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));

                    App.Instance.Settings.SelectedVersion = value?.VersionString ?? string.Empty;
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

        public bool? AllModsActive
        {
            get { return allModsActive; }
            set
            {
                if (value != allModsActive)
                {
                    allModsActive = value;
                    allModsSelectedChanging = true;

                    if (allModsActive.HasValue)
                    {
                        ModManager.BeginUpdateTemplates();

                        foreach (var mod in Mods)
                        {
                            if (mod.Active != allModsActive.Value)
                                mod.Active = allModsActive.Value;
                        }

                        ModManager.EndUpdateTemplates();
                        ModManager.SaveTemplates();
                    }

                    allModsSelectedChanging = false;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllModsActive)));
                }
            }
        }

        public bool? AllModpacksActive
        {
            get { return allModpacksActive; }
            set
            {
                if (value != allModpacksActive)
                {
                    allModpacksActive = value;
                    allModpacksSelectedChanging = true;

                    if (allModpacksActive.HasValue)
                    {
                        ModManager.BeginUpdateTemplates();

                        foreach (var modpack in Modpacks)
                        {
                            if (modpack.Active != allModpacksActive.Value)
                                modpack.Active = allModpacksActive.Value;
                        }

                        ModManager.EndUpdateTemplates();
                        ModManager.SaveTemplates();
                    }

                    allModpacksSelectedChanging = false;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllModpacksActive)));
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

        public RelayCommand BrowseWikiCommand { get; }

        public RelayCommand ActivateSelectedModsCommand { get; }

        public RelayCommand DeactivateSelectedModsCommand { get; }

        public RelayCommand DeleteSelectedModsCommand { get; }

        public RelayCommand SelectActiveModsCommand { get; }

        public RelayCommand ActivateSelectedModpacksCommand { get; }

        public RelayCommand DeactivateSelectedModpacksCommand { get; }

        public RelayCommand DeleteSelectedModpacksCommand { get; }

        public RelayCommand SelectActiveModpacksCommand { get; }

        public RelayCommand DeleteSelectedModsAndModpacksCommand { get; }

        private bool ModFilter(object item)
        {
            Mod mod = item as Mod;
            if (mod == null) return false;

            if (string.IsNullOrWhiteSpace(modsFilter)) return true;

            return StringHelper.FilterIsContained(modsFilter, $"{mod.Title} {mod.Author}");
        }

        private bool ModpackFilter(object item)
        {
            Modpack modpack = item as Modpack;
            if (modpack == null) return false;

            if (string.IsNullOrWhiteSpace(modpacksFilter)) return true;

            return StringHelper.FilterIsContained(modpacksFilter, modpack.Name);
        }

        private void SetAllModsActive()
        {
            if (Mods.Count == 0 || allModsSelectedChanging)
                return;

            bool? newValue = Mods[0].Active;
            for (int i = 1; i < Mods.Count; i++)
            {
                if (Mods[i].Active != newValue)
                {
                    newValue = null;
                    break;
                }
            }

            if (newValue != allModsActive)
            {
                allModsActive = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllModsActive)));
            }
        }

        private void ModPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Mod.Active))
            {
                SetAllModsActive();
            }
        }

        private void ModsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Mod mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    SetAllModsActive();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Mod mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    SetAllModsActive();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (Mod mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    foreach (Mod mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    SetAllModsActive();
                    break;
            }
        }

        private void SetAllModpacksActive()
        {
            if (Modpacks.Count == 0 || allModpacksSelectedChanging)
                return;

            bool? newValue = Modpacks[0].Active;
            for (int i = 1; i < Modpacks.Count; i++)
            {
                if (Modpacks[i].Active != newValue)
                {
                    newValue = null;
                    break;
                }
            }

            if (newValue != allModpacksActive)
            {
                allModpacksActive = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllModpacksActive)));
            }
        }

        private void ModpackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Modpack.Active))
            {
                SetAllModpacksActive();
            }
        }

        private void ModpacksChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Modpack modpack in e.NewItems)
                        modpack.PropertyChanged += ModpackPropertyChanged;
                    SetAllModpacksActive();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Modpack modpack in e.OldItems)
                        modpack.PropertyChanged -= ModpackPropertyChanged;
                    SetAllModpacksActive();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (Modpack modpack in e.NewItems)
                        modpack.PropertyChanged += ModpackPropertyChanged;
                    foreach (Modpack modpack in e.OldItems)
                        modpack.PropertyChanged -= ModpackPropertyChanged;
                    SetAllModpacksActive();
                    break;
            }
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

                if (!Environment.Is64BitOperatingSystem && !App.Instance.Settings.WarningShown)
                {
                    MessageBox.Show(
                        App.Instance.GetLocalizedMessage("32Bit", MessageType.Information),
                        App.Instance.GetLocalizedMessageTitle("32Bit", MessageType.Information),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                App.Instance.Settings.WarningShown = true;


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
                        SetSelectedVersionInternal(factorioVersion);
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
                Mods.CollectionChanged += ModsChangedHandler;
                SetAllModsActive();

                Modpacks = new ObservableCollection<Modpack>();
                ModpacksView = (ListCollectionView)(new CollectionViewSource() { Source = Modpacks }).View;
                ModpacksView.CustomSort = new ModpackSorter();
                ModpacksView.Filter = ModpackFilter;
                Modpacks.CollectionChanged += ModpacksChangedHandler;
                SetAllModpacksActive();

                Mod.LoadMods(Mods, Modpacks);
                ModpackTemplateList.Instance.PopulateModpackList(Mods, Modpacks, ModpacksView);
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
                BrowseWikiCommand = new RelayCommand(() => Process.Start("https://github.com/Artentus/ModMyFactory/wiki"));

                // context menu
                ActivateSelectedModsCommand = new RelayCommand(ActivateSelectedMods, () => Mods.Any(mod => mod.IsSelected));
                DeactivateSelectedModsCommand = new RelayCommand(DeactivateSelectedMods, () => Mods.Any(mod => mod.IsSelected));
                DeleteSelectedModsCommand = new RelayCommand(DeleteSelectedMods, () => Mods.Any(mod => mod.IsSelected));
                SelectActiveModsCommand = new RelayCommand(SelectActiveMods);

                ActivateSelectedModpacksCommand = new RelayCommand(ActivateSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                DeactivateSelectedModpacksCommand = new RelayCommand(DeactivateSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                DeleteSelectedModpacksCommand = new RelayCommand(DeleteSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                SelectActiveModpacksCommand = new RelayCommand(SelectActiveModpacks);

                DeleteSelectedModsAndModpacksCommand = new RelayCommand(DeleteSelectedModsAndModpacks, () => Mods.Any(mod => mod.IsSelected) || Modpacks.Any(modpack => modpack.IsSelected));


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
                var modsViewModel = (OnlineModsViewModel)modsWindow.ViewModel;
                if (modsViewModel.SelectedMod != null) modsViewModel.UpdateSelectedReleases();
                modsWindow.ShowDialog();
            }
            else
            {
                List<ModInfo> modInfos;
                try
                {
                    modInfos = await ModHelper.FetchMods(Window);
                }
                catch (WebException)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }

                if (modInfos != null)
                {
                    var modsWindow = new OnlineModsWindow() { Owner = Window };
                    var modsViewModel = (OnlineModsViewModel)modsWindow.ViewModel;
                    modsViewModel.Mods = modInfos;

                    modsWindow.ShowDialog();
                }
            }
        }

        private async Task AddModFromFile(FileInfo archiveFile, Window messageOwner)
        {
            Version factorioVersion;
            string name;
            Version version;
            if (Mod.ArchiveFileValid(archiveFile, out factorioVersion, out name, out version))
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

                    var mod = new ZippedMod(name, version, factorioVersion, archiveFile, Mods, Modpacks);
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
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("ProcessingModsAction");

                var cancellationSource = new CancellationTokenSource();
                progressViewModel.CanCancel = true;
                progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                var progress = new Progress<Tuple<double, string>>(info =>
                {
                    progressViewModel.Progress = info.Item1;
                    progressViewModel.ProgressDescription = info.Item2;
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
                Version version;
                if (Mod.DirectoryValid(directory, out factorioVersion, out name, out version))
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
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("ProcessingModAction");
                progressViewModel.ProgressDescription = directory.Name;
                progressViewModel.IsIndeterminate = true;

                moveDirectoryTask = moveDirectoryTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();
                await moveDirectoryTask;

                Mods.Add(new ExtractedMod(name, version, factorioVersion, directory, Mods, Modpacks));
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

            Modpack modpack = new Modpack(newName, Modpacks);
            modpack.ParentView = ModpacksView;
            Modpacks.Add(modpack);

            modpack.Editing = true;
            Window.ModpacksListBox.ScrollIntoView(modpack);
        }

        private void CreateLink()
        {
            var propertiesWindow = new LinkPropertiesWindow() { Owner = Window };
            var propertiesViewModel = (LinkPropertiesViewModel)propertiesWindow.ViewModel;
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
                    string versionString = propertiesViewModel.SelectedVersion.VersionString;
                    string modpackName = propertiesViewModel.SelectedModpack?.Name;

                    string arguments = $"--factorio-version=\"{versionString}\"";
                    if (!string.IsNullOrEmpty(modpackName)) arguments += $" --modpack=\"{modpackName}\"";
                    ShellHelper.CreateShortcut(dialog.FileName, applicationPath, arguments, iconPath);
                }
            }
        }

        private void ExportModpacks()
        {
            var exportWindow = new ModpackExportWindow() { Owner = Window };
            var exportViewModel = (ModpackExportViewModel)exportWindow.ViewModel;
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
                        exportViewModel.IncludeVersionInfo);
                    ModpackExport.ExportTemplate(template, dialog.FileName);
                }
            }
        }

        #region ModpackImport

        private ModRelease GetNewestRelease(ExtendedModInfo info)
        {
            return info.Releases.MaxBy(release => release.Version, new VersionComparer());
        }

        private async Task<Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>> GetModsToDownload(ExportTemplate template, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var toDownload = new List<ModRelease>();
            var conflicting = new List<Tuple<Mod, ModExportTemplate>>();

            int modCount = template.Mods.Length;
            int counter = 0;
            foreach (var modTemplate in template.Mods)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                progress.Report(new Tuple<double, string>((double)counter / modCount, modTemplate.Name));
                counter++;

                ExtendedModInfo modInfo = null;
                try
                {
                    modInfo = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.ProtocolError) throw;
                }

                if (modInfo != null)
                {
                    if (template.IncludesVersionInfo)
                    {
                        if (!Mods.Contains(modTemplate.Name, modTemplate.Version))
                        {
                            Mod[] mods = Mods.Find(modTemplate.Name);

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
                    else
                    {
                        Mod[] mods = Mods.Find(modTemplate.Name);

                        if (mods.Length == 0)
                        {
                            ModRelease newestRelease = GetNewestRelease(modInfo);
                            toDownload.Add(newestRelease);
                        }
                        else
                        {
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
            }

            progress.Report(new Tuple<double, string>(1, string.Empty));

            return new Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>(toDownload, conflicting);
        }

        private async Task DownloadModAsyncInner(ModRelease modRelease, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            Mod mod = await ModWebsite.DownloadReleaseAsync(modRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken, Mods, Modpacks);
            if (!cancellationToken.IsCancellationRequested && (mod != null)) Mods.Add(mod);
        }

        private async Task DownloadModsAsyncInner(List<ModRelease> modReleases, string token, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
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

                try
                {
                    await DownloadModAsyncInner(release, token, modProgress, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                baseProgressValue += modProgressValue;
            }
        }

        private async Task DownloadModsAsync(List<ModRelease> modReleases)
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

                var progress = new Progress<Tuple<double, string>>(info =>
                {
                    progressViewModel.Progress = info.Item1;
                    progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), info.Item2);
                });

                var cancellationSource = new CancellationTokenSource();
                progressViewModel.CanCancel = true;
                progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                Task updateTask = DownloadModsAsyncInner(modReleases, token, progress, cancellationSource.Token);
                Task closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await updateTask;
                await closeWindowTask;
            }
        }

        private async Task ImportModpackFile(FileInfo modpackFile)
        {
            ExportTemplate template = ModpackExport.ImportTemplate(modpackFile);

            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

            var progress = new Progress<Tuple<double, string>>(info =>
            {
                progressViewModel.Progress = info.Item1;
                progressViewModel.ProgressDescription = info.Item2;
            });

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>> toDownloadResult;
            try
            {
                Task closeWindowTask = null;
                try
                {
                    var getModsTask = GetModsToDownload(template, progress, cancellationSource.Token);

                    closeWindowTask = getModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    toDownloadResult = await getModsTask;
                }
                finally
                {
                    if (closeWindowTask != null) await closeWindowTask;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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

            try
            {
                if (toDownload.Count > 0)
                    await DownloadModsAsync(toDownload);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var modpackTemplate in template.Modpacks)
            {
                var existingModpack = Modpacks.FirstOrDefault(item => item.Name == modpackTemplate.Name);

                if (existingModpack == null)
                {
                    Modpack modpack = new Modpack(modpackTemplate.Name, Modpacks);
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

                ExtendedModInfo extendedInfo = null;
                try
                {
                    extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod);
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.ProtocolError) throw;
                }

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

        private async Task UpdateModAsyncInner(ModUpdateInfo modUpdate, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            FileInfo modFile = await ModWebsite.UpdateReleaseAsync(modUpdate.NewestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken);
            var zippedMod = modUpdate.Mod as ZippedMod;
            var extractedMod = modUpdate.Mod as ExtractedMod;
            if (zippedMod != null)
            {
                if (zippedMod.FactorioVersion == modUpdate.NewestRelease.FactorioVersion)
                {
                    zippedMod.Update(modFile, modUpdate.NewestRelease.Version);
                }
                else
                {
                    var newMod = new ZippedMod(zippedMod.Name, modUpdate.NewestRelease.Version, modUpdate.NewestRelease.FactorioVersion, modFile, Mods, Modpacks);
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
                DirectoryInfo modDirectory = await Task.Run(() =>
                {
                    DirectoryInfo modsDirectory = App.Instance.Settings.GetModDirectory(modUpdate.NewestRelease.FactorioVersion);
                    ZipFile.ExtractToDirectory(modFile.FullName, modsDirectory.FullName);
                    modFile.Delete();

                    return new DirectoryInfo(Path.Combine(modsDirectory.FullName, modFile.NameWithoutExtension()));
                });

                if (extractedMod.FactorioVersion == modUpdate.NewestRelease.FactorioVersion)
                {
                    extractedMod.Update(modDirectory, modUpdate.NewestRelease.Version);
                }
                else
                {
                    var newMod = new ExtractedMod(extractedMod.Name, modUpdate.NewestRelease.Version, modUpdate.NewestRelease.FactorioVersion, modDirectory, Mods, Modpacks);
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

        private async Task UpdateModsAsyncInner(List<ModUpdateInfo> modUpdates, string token, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
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

                    try
                    {
                        await UpdateModAsyncInner(modUpdate, token, modProgress, cancellationToken);
                    }
                    catch (HttpRequestException)
                    {
                        MessageBox.Show(Window,
                            App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                            App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    baseProgressValue += modProgressValue;
                }
            }
        }

        private async Task UpdateMods()
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("SearchingForUpdatesAction");

            var progress = new Progress<Tuple<double, string>>(info =>
            {
                progressViewModel.Progress = info.Item1;
                progressViewModel.ProgressDescription = info.Item2;
            });

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            List<ModUpdateInfo> modUpdates;
            try
            {
                Task closeWindowTask = null;
                try
                {
                    Task<List<ModUpdateInfo>> searchForUpdatesTask = GetModUpdatesAsync(progress, cancellationSource.Token);

                    closeWindowTask = searchForUpdatesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    modUpdates = await searchForUpdatesTask;
                }
                finally
                {
                    if (closeWindowTask != null) await closeWindowTask;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!cancellationSource.IsCancellationRequested)
            {
                if (modUpdates.Count > 0)
                {
                    var updateWindow = new ModUpdateWindow() { Owner = Window };
                    var updateViewModel = (ModUpdateViewModel)updateWindow.ViewModel;
                    updateViewModel.ModsToUpdate = modUpdates;
                    bool? result = updateWindow.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        string token;
                        if (GlobalCredentials.Instance.LogIn(Window, out token))
                        {
                            progressWindow = new ProgressWindow() { Owner = Window };
                            progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("UpdatingModsAction");

                            progress = new Progress<Tuple<double, string>>(info =>
                            {
                                progressViewModel.Progress = info.Item1;
                                progressViewModel.ProgressDescription = info.Item2;
                            });

                            cancellationSource = new CancellationTokenSource();
                            progressViewModel.CanCancel = true;
                            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                            Task updateTask = UpdateModsAsyncInner(modUpdates, token, progress, cancellationSource.Token);

                            Task closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
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

        private async Task MoveDirectoriesInternal(DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo oldSavegameDirectory, DirectoryInfo oldScenarioDirectory,
            DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory, DirectoryInfo newSavegameDirectory, DirectoryInfo newScenarioDirectory)
        {
            // Factorio directory
            bool moveFactorioDirectory = !newFactorioDirectory.DirectoryEquals(oldFactorioDirectory);
            if (oldFactorioDirectory.Exists && moveFactorioDirectory)
            {
                if (!newFactorioDirectory.Exists) newFactorioDirectory.Create();

                DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                foreach (var version in FactorioVersions)
                {
                    version.DeleteLinks();

                    if (version.IsFileSystemEditable)
                    {
                        var versionDirectory = new DirectoryInfo(Path.Combine(factorioDirectory.FullName, version.VersionString));
                        await version.Directory.MoveToAsync(versionDirectory.FullName);
                        version.UpdateDirectory(versionDirectory);
                    }
                }
                
                oldFactorioDirectory.DeleteIfEmpty();
            }


            // Mod directory
            bool moveModDirectory = !newModDirectory.DirectoryEquals(oldModDirectory);
            if (oldModDirectory.Exists && moveModDirectory)
            {
                if (!newModDirectory.Exists) newModDirectory.Create();

                foreach (var mod in Mods)
                {
                    var dir = new DirectoryInfo(Path.Combine(newModDirectory.FullName, mod.FactorioVersion.ToString(2)));
                    if (!dir.Exists) dir.Create();
                    await mod.MoveTo(dir);
                }
                ModManager.SaveTemplates();

                foreach (var version in FactorioVersions)
                {
                    if (!version.IsSpecialVersion)
                    {
                        var dir = new DirectoryInfo(Path.Combine(oldModDirectory.FullName, version.Version.ToString(2)));
                        if (dir.Exists) dir.Delete(true);
                    }
                }
                
                oldModDirectory.DeleteIfEmpty();
            }


            // Savegame directory
            if (oldSavegameDirectory.Exists && !newSavegameDirectory.DirectoryEquals(oldSavegameDirectory))
                await oldSavegameDirectory.MoveToAsync(newSavegameDirectory.FullName);


            // Scenario directory
            if (oldScenarioDirectory.Exists && !newScenarioDirectory.DirectoryEquals(oldScenarioDirectory))
                await oldScenarioDirectory.MoveToAsync(newScenarioDirectory.FullName);


            foreach (var version in FactorioVersions)
                version.CreateLinks();
        }

        private async Task MoveDirectories(DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo oldSavegameDirectory, DirectoryInfo oldScenarioDirectory,
            DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory, DirectoryInfo newSavegameDirectory, DirectoryInfo newScenarioDirectory)
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("MovingDirectoriesAction");
            progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("MovingFilesDescription");
            progressViewModel.IsIndeterminate = true;

            Task moveDirectoriesTask = MoveDirectoriesInternal(oldFactorioDirectory, oldModDirectory, oldSavegameDirectory, oldScenarioDirectory, newFactorioDirectory, newModDirectory, newSavegameDirectory, newScenarioDirectory);

            Task closeWindowTask = moveDirectoriesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
            progressWindow.ShowDialog();

            await moveDirectoriesTask;
            await closeWindowTask;
        }

        private async Task ApplySettings(Settings settings, SettingsViewModel settingsViewModel, SettingsWindow settingsWindow)
        {
            DirectoryInfo oldFactorioDirectory = settings.GetFactorioDirectory();
            DirectoryInfo oldModDirectory = settings.GetModDirectory();
            DirectoryInfo oldSavegameDirectory = settings.GetSavegameDirectory();
            DirectoryInfo oldScenarioDirectory = settings.GetScenarioDirectory();

            // Manager mode
            settings.ManagerMode = settingsViewModel.ManagerMode;

            // Update search
            settings.UpdateSearchOnStartup = settingsViewModel.UpdateSearchOnStartup;

            // Factorio location
            settings.FactorioDirectoryOption = settingsViewModel.FactorioDirectoryOption;
            settings.FactorioDirectory = (settings.FactorioDirectoryOption == DirectoryOption.Custom)
                ? settingsViewModel.FactorioDirectory : string.Empty;

            // Mod location
            settings.ModDirectoryOption = settingsViewModel.ModDirectoryOption;
            settings.ModDirectory = (settings.ModDirectoryOption == DirectoryOption.Custom)
                ? settingsViewModel.ModDirectory : string.Empty;

            // Savegame location
            settings.SavegameDirectoryOption = settingsViewModel.SavegameDirectoryOption;
            settings.SavegameDirectory = (settings.SavegameDirectoryOption == DirectoryOption.Custom)
                ? settingsViewModel.SavegameDirectory : string.Empty;

            // Scenario location
            settings.ScenarioDirectoryOption = settingsViewModel.ScenarioDirectoryOption;
            settings.ScenarioDirectory = (settings.ScenarioDirectoryOption == DirectoryOption.Custom)
                ? settingsViewModel.ScenarioDirectory : string.Empty;

            // Login credentials
            settings.SaveCredentials = settingsWindow.SaveCredentialsBox.IsChecked ?? false;
            if (settings.SaveCredentials)
            {
                GlobalCredentials.Instance.Username = settingsWindow.UsernameBox.Text;
                if (!string.IsNullOrEmpty(settingsWindow.PasswordBox.Password))
                    GlobalCredentials.Instance.Password = settingsWindow.PasswordBox.SecurePassword;
                GlobalCredentials.Instance.Save();
            }
            else
            {
                GlobalCredentials.Instance.DeleteSave();
            }

            settings.Save();

            DirectoryInfo newFactorioDirectory = settings.GetFactorioDirectory();
            DirectoryInfo newModDirectory = settings.GetModDirectory();
            DirectoryInfo newSavegameDirectory = settings.GetSavegameDirectory();
            DirectoryInfo newScenarioDirectory = settings.GetScenarioDirectory();

            await MoveDirectories(oldFactorioDirectory, oldModDirectory, oldSavegameDirectory, oldScenarioDirectory, newFactorioDirectory, newModDirectory, newSavegameDirectory, newScenarioDirectory);
        }

        private async Task OpenSettings()
        {
            Settings settings = App.Instance.Settings;

            var settingsWindow = new SettingsWindow() { Owner = Window };
            var settingsViewModel = (SettingsViewModel)settingsWindow.ViewModel;
            settingsViewModel.Reset();
            settingsWindow.SaveCredentialsBox.IsChecked = settings.SaveCredentials;

            bool? result = settingsWindow.ShowDialog();
            if (result != null && result.Value)
            {
                await ApplySettings(settings, settingsViewModel, settingsWindow);
            }
        }

        #endregion

        private async Task Update(bool silent)
        {
            updating = true;

            try
            {
                UpdateSearchResult result = null;

                try
                {
                    result = await App.Instance.SearchForUpdateAsync();
                }
                catch (HttpRequestException)
                {
                    if (!silent)
                    {
                        MessageBox.Show(Window,
                            App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                            App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }

                if (result != null)
                {
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
                }
            }
            finally
            {
                updating = false;
            }
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }

        #region ContextMenus

        private void SetSelectedModsActiveState(bool state)
        {
            ModManager.BeginUpdateTemplates();

            foreach (Mod mod in Mods)
            {
                if (mod.IsSelected)
                    mod.Active = state;
            }

            ModManager.EndUpdateTemplates(true);
            ModManager.SaveTemplates();
        }

        private void ActivateSelectedMods()
        {
            SetSelectedModsActiveState(true);
        }

        private void DeactivateSelectedMods()
        {
            SetSelectedModsActiveState(false);
        }

        private void DeleteSelectedMods()
        {
            var deletionList = new List<Mod>();
            foreach (Mod mod in Mods)
            {
                if (mod.IsSelected)
                    deletionList.Add(mod);
            }

            if (deletionList.Count > 0 && MessageBox.Show(Window,
                App.Instance.GetLocalizedMessage("DeleteMods", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("DeleteMods", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (Mod mod in deletionList)
                    mod.Delete(false);
            }
        }

        private void SelectActiveMods()
        {
            foreach (Mod mod in Mods)
            {
                if (mod.Active)
                    mod.IsSelected = true;
            }
        }

        private void SetSelectedModpacksActiveState(bool state)
        {
            ModManager.BeginUpdateTemplates();

            foreach (Modpack modpack in Modpacks)
            {
                if (modpack.IsSelected)
                    modpack.Active = state;
            }

            ModManager.EndUpdateTemplates(true);
            ModManager.SaveTemplates();
        }

        private void ActivateSelectedModpacks()
        {
            SetSelectedModpacksActiveState(true);
        }

        private void DeactivateSelectedModpacks()
        {
            SetSelectedModpacksActiveState(false);
        }

        private void DeleteSelectedModpacks()
        {
            var deletionList = new List<Modpack>();
            foreach (Modpack modpack in Modpacks)
            {
                if (modpack.IsSelected)
                    deletionList.Add(modpack);
            }

            if (deletionList.Count > 0 && MessageBox.Show(Window,
                App.Instance.GetLocalizedMessage("DeleteModpacks", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("DeleteModpacks", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (Modpack modpack in deletionList)
                    modpack.Delete(false);
            }
        }

        private void SelectActiveModpacks()
        {
            foreach (Modpack modpack in Modpacks)
            {
                if (modpack.Active ?? false)
                    modpack.IsSelected = true;
            }
        }

        private void DeleteSelectedModsAndModpacks()
        {
            DeleteSelectedMods();
            DeleteSelectedModpacks();
        }

        #endregion

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

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if ((e.PropertyName == nameof(Window)) && (Window != null))
            {
                Window.Loaded += async (sender, ea) =>
                {
                    if (Program.ImportFileList.Count > 0)
                        await ImportModpacksInner(Program.ImportFileList);
                    else if (Program.UpdateCheckOnStartup && App.Instance.Settings.UpdateSearchOnStartup) // Just skip update check if import list is non-zero
                        await Update(true);
                };
            }
        }
    }
}
