using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Helpers;
using ModMyFactory.Lang;
using Ookii.Dialogs.Wpf;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using ModMyFactory.Web.ModApi;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed partial class MainViewModel : ViewModelBase
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        public MainWindow Window => (MainWindow)View;

        #region AvailableCultures

        public List<CultureEntry> AvailableCultures { get; }

        public ListCollectionView AvailableCulturesView { get; }

        #endregion

        #region FactorioVersions

        ObservableCollection<FactorioVersion> factorioVersions;
        CollectionViewSource factorioVersionsSource;
        ListCollectionView factorioVersionsView;
        FactorioVersion selectedFactorioVersion;

        private bool FactorioVersionFilter(object item)
        {
            FactorioVersion factorioVersion = item as FactorioVersion;
            return factorioVersion?.IsSpecialVersion == false;
        }

        public ObservableCollection<FactorioVersion> FactorioVersions
        {
            get { return factorioVersions; }
            private set
            {
                if (value != factorioVersions)
                {
                    factorioVersions = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioVersions)));

                    if (factorioVersionsSource == null) factorioVersionsSource = new CollectionViewSource();
                    factorioVersionsSource.Source = factorioVersions;
                    var factorioVersionsView = (ListCollectionView)factorioVersionsSource.View;
                    factorioVersionsView.CustomSort = new FactorioVersionSorter();
                    factorioVersionsView.Filter = FactorioVersionFilter;
                    FactorioVersionsView = factorioVersionsView;
                }
            }
        }

        public ListCollectionView FactorioVersionsView
        {
            get { return factorioVersionsView; }
            private set
            {
                if (value != factorioVersionsView)
                {
                    factorioVersionsView = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FactorioVersionsView)));
                }
            }
        }

        private void SelectedFactorioVersionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FactorioVersion.VersionString))
            {
                App.Instance.Settings.SelectedVersion = selectedFactorioVersion.VersionString;
                App.Instance.Settings.Save();
            }
        }

        public FactorioVersion SelectedFactorioVersion
        {
            get { return selectedFactorioVersion; }
            set
            {
                if (value != selectedFactorioVersion)
                {
                    if (selectedFactorioVersion != null)
                        selectedFactorioVersion.PropertyChanged -= SelectedFactorioVersionPropertyChangedHandler;
                    selectedFactorioVersion = value;
                    if (selectedFactorioVersion != null)
                        selectedFactorioVersion.PropertyChanged += SelectedFactorioVersionPropertyChangedHandler;

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedFactorioVersion)));

                    string newVersionString = selectedFactorioVersion?.VersionString ?? string.Empty;
                    if (newVersionString != App.Instance.Settings.SelectedVersion)
                    {
                        App.Instance.Settings.SelectedVersion = newVersionString;
                        App.Instance.Settings.Save();
                    }
                }
            }
        }

        #endregion

        #region Mods

        string modFilterPattern;
        bool? allModsActive;
        bool allModsSelectedChanging;
        ModCollection mods;
        CollectionViewSource modsSource;
        ListCollectionView modsView;

        public string ModFilterPattern
        {
            get { return modFilterPattern; }
            set
            {
                if (value != modFilterPattern)
                {
                    modFilterPattern = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModFilterPattern)));

                    ModsView.Refresh();
                }
            }
        }

        private bool ModFilter(object item)
        {
            Mod mod = item as Mod;
            if (mod == null) return false;

            if (string.IsNullOrWhiteSpace(ModFilterPattern)) return true;
            return StringHelper.FilterIsContained(ModFilterPattern, $"{mod.FriendlyName} {mod.Author}");
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
                    if (e.NewItems != null)
                    {
                        foreach (Mod mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    }
                    if (e.OldItems != null)
                    {
                        foreach (Mod mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    }
                    SetAllModsActive();
                    break;
            }
        }

        public ModCollection Mods
        {
            get { return mods; }
            private set
            {
                if (value != mods)
                {
                    mods = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));

                    if (modsSource == null) modsSource = new CollectionViewSource();
                    modsSource.Source = mods;
                    var modsView = (ListCollectionView)modsSource.View;
                    modsView.CustomSort = new ModSorter();
                    modsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Mod.FactorioVersion)));
                    modsView.Filter = ModFilter;
                    mods.CollectionChanged += ModsChangedHandler;
                    ModsView = modsView;

                    SetAllModsActive();
                }
            }
        }

        public ListCollectionView ModsView
        {
            get { return modsView; }
            private set
            {
                if (value != modsView)
                {
                    modsView = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsView)));
                }
            }
        }

        #endregion

        #region Modpacks

        string modpackFilterPattern;
        bool? allModpacksActive;
        bool allModpacksSelectedChanging;
        ModpackCollection modpacks;
        CollectionViewSource modpacksSource;
        ListCollectionView modpacksView;

        public string ModpackFilterPattern
        {
            get { return modpackFilterPattern; }
            set
            {
                if (value != modpackFilterPattern)
                {
                    modpackFilterPattern = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModpackFilterPattern)));

                    ModpacksView.Refresh();
                }
            }
        }

        private bool ModpackFilter(object item)
        {
            Modpack modpack = item as Modpack;
            if (modpack == null) return false;

            if (string.IsNullOrWhiteSpace(ModpackFilterPattern)) return true;
            return StringHelper.FilterIsContained(ModpackFilterPattern, modpack.Name);
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
                    if (e.NewItems != null)
                    {
                        foreach (Modpack modpack in e.NewItems)
                        modpack.PropertyChanged += ModpackPropertyChanged;
                    }
                    if (e.OldItems != null)
                    { foreach (Modpack modpack in e.OldItems)
                        modpack.PropertyChanged -= ModpackPropertyChanged;
                    }
                    SetAllModpacksActive();
                    break;
            }
        }

        public ModpackCollection Modpacks
        {
            get { return modpacks; }
            set
            {
                if (value != modpacks)
                {
                    modpacks = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Modpacks)));

                    if (modpacksSource == null) modpacksSource = new CollectionViewSource();
                    modpacksSource.Source = modpacks;
                    var modpacksView = (ListCollectionView)modpacksSource.View;
                    modpacksView.CustomSort = new ModpackSorter();
                    modpacksView.Filter = ModpackFilter;
                    modpacks.CollectionChanged += ModpacksChangedHandler;
                    ModpacksView = modpacksView;

                    SetAllModpacksActive();
                }
            }
        }

        public ListCollectionView ModpacksView
        {
            get { return modpacksView; }
            set
            {
                if (value != modpacksView)
                {
                    modpacksView = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModpacksView)));
                }
            }
        }

        #endregion

        #region GridLengths

        GridLength modGridLength;
        GridLength modpackGridLength;

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

        #endregion

        #region Commands

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

        public RelayCommand DownloadDependenciesCommand { get; }

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

        public RelayCommand SelectInactiveModsCommand { get; }

        public RelayCommand ActivateSelectedModpacksCommand { get; }

        public RelayCommand DeactivateSelectedModpacksCommand { get; }

        public RelayCommand DeleteSelectedModpacksCommand { get; }

        public RelayCommand SelectActiveModpacksCommand { get; }

        public RelayCommand SelectInactiveModpacksCommand { get; }

        public RelayCommand DeleteSelectedModsAndModpacksCommand { get; }

        public RelayCommand ClearModFilterCommand { get; }

        public RelayCommand ClearModpackFilterCommand { get; }

        public RelayCommand RefreshCommand { get; }

        #endregion

        volatile bool modpacksLoading;
        volatile bool updating;

        private void LoadFactorioVersions()
        {
            var installedVersions = FactorioVersion.GetInstalledVersions();
            var factorioVersions = new ObservableCollection<FactorioVersion>(installedVersions) { FactorioVersion.Latest };

            FactorioVersion steamVersion;
            if (FactorioSteamVersion.TryLoad(out steamVersion)) factorioVersions.Add(steamVersion);

            string versionString = App.Instance.Settings.SelectedVersion;
            FactorioVersions = factorioVersions;
            SelectedFactorioVersion = string.IsNullOrEmpty(versionString) ? null : FactorioVersions.FirstOrDefault(item => item.VersionString == versionString);
        }

        private void ModpacksCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!modpacksLoading)
            {
                ModpackTemplateList.Instance.Update(Modpacks);
                ModpackTemplateList.Instance.Save();
            }
        }

        private void LoadModsAndModpacks()
        {
            modpacksLoading = true;

            if (Mods == null)
            {
                Mods = new ModCollection();
            }
            else
            {
                Mods.Clear();
            }

            if (Modpacks == null)
            {
                Modpacks = new ModpackCollection();
                Modpacks.CollectionChanged += ModpacksCollectionChangedHandler;
            }
            else
            {
                Modpacks.Clear();
            }

            
            Mod.LoadMods(Mods, Modpacks);
            ModpackTemplateList.Instance.PopulateModpackList(Mods, Modpacks, ModpacksView);

            modpacksLoading = false;
        }

        private void Refresh()
        {
            ModManager.LoadTemplates();
            LoadFactorioVersions();
            LoadModsAndModpacks();
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

                Refresh();
                

                modGridLength = App.Instance.Settings.ModGridLength;
                modpackGridLength = App.Instance.Settings.ModpackGridLength;


                // 'File' menu
                DownloadModsCommand = new RelayCommand(async () => await DownloadMods());
                AddModsFromFilesCommand = new RelayCommand(async () => await AddModsFromFiles());
                AddModFromFolderCommand = new RelayCommand(async () => await AddModFromFolder());
                CreateModpackCommand = new RelayCommand(CreateNewModpack);
                CreateLinkCommand = new RelayCommand(CreateLink);

                ExportModpacksCommand = new RelayCommand(async () => await ExportModpacks());
                ImportModpacksCommand = new RelayCommand(async () => await ImportModpacks());

                StartGameCommand = new RelayCommand(StartGame, () => SelectedFactorioVersion != null);

                // 'Edit' menu
                UpdateModsCommand = new RelayCommand(async () => await UpdateMods());
                DownloadDependenciesCommand = new RelayCommand(async () => await DownloadDependencies());

                OpenVersionManagerCommand = new RelayCommand(OpenVersionManager);
                OpenSettingsCommand = new RelayCommand(async () => await OpenSettings());

                // 'View' menu
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
                    var savegameDirectory = App.Instance.Settings.GetSavegameDirectory();
                    if (!savegameDirectory.Exists) savegameDirectory.Create();
                    Process.Start(savegameDirectory.FullName);
                });
                OpenScenarioFolderCommand = new RelayCommand(() =>
                {
                    var scenariosDirectory = App.Instance.Settings.GetScenarioDirectory();
                    if (!scenariosDirectory.Exists) scenariosDirectory.Create();
                    Process.Start(scenariosDirectory.FullName);
                });

                RefreshCommand = new RelayCommand(Refresh);

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
                SelectInactiveModsCommand = new RelayCommand(SelectInactiveMods);

                ActivateSelectedModpacksCommand = new RelayCommand(ActivateSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                DeactivateSelectedModpacksCommand = new RelayCommand(DeactivateSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                DeleteSelectedModpacksCommand = new RelayCommand(DeleteSelectedModpacks, () => Modpacks.Any(modpack => modpack.IsSelected));
                SelectActiveModpacksCommand = new RelayCommand(SelectActiveModpacks);
                SelectInactiveModpacksCommand = new RelayCommand(SelectInactiveModpacks);

                DeleteSelectedModsAndModpacksCommand = new RelayCommand(DeleteSelectedModsAndModpacks, () => Mods.Any(mod => mod.IsSelected) || Modpacks.Any(modpack => modpack.IsSelected));

                ClearModFilterCommand = new RelayCommand(() => ModFilterPattern = string.Empty);
                ClearModpackFilterCommand = new RelayCommand(() => ModpackFilterPattern = string.Empty);


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
                    return;
                }

                if (modInfos != null)
                {
                    var modsWindow = new OnlineModsWindow() { Owner = Window };
                    var modsViewModel = (OnlineModsViewModel)modsWindow.ViewModel;
                    modsViewModel.Mods = modInfos;

                    modsWindow.ShowDialog();
                }
            }

            Mods.EvaluateDependencies();
        }
        
        private async Task AddModsFromFilesInner(string[] fileNames, bool move, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            int fileCount = fileNames.Length;
            int counter = 0;
            foreach (string fileName in fileNames)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                progress.Report(new Tuple<double, string>((double)counter / fileCount, Path.GetFileName(fileName)));

                var archiveFile = new FileInfo(fileName);
                await Mod.Add(archiveFile, Mods, Modpacks, !move);

                counter++;
            }

            progress.Report(new Tuple<double, string>(1, string.Empty));

            Mods.EvaluateDependencies();
        }

        private async Task AddModsFromFiles()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = App.Instance.GetLocalizedResourceString("ZipDescription") + @" (*.zip)|*.zip";
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var copyOrMoveWindow = new CopyOrMoveMessageWindow() { Owner = Window };
                ((CopyOrMoveViewModel)copyOrMoveWindow.ViewModel).CopyOrMoveType = CopyOrMoveType.Mods;
                result = copyOrMoveWindow.ShowDialog();
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

                    Task processModsTask = AddModsFromFilesInner(dialog.FileNames, copyOrMoveWindow.Move, progress, cancellationSource.Token);

                    Task closeWindowTask = processModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    await processModsTask;
                    await closeWindowTask;
                }
            }
        }

        private async Task AddModFromFolder()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var copyOrMoveWindow = new CopyOrMoveMessageWindow() { Owner = Window };
                ((CopyOrMoveViewModel)copyOrMoveWindow.ViewModel).CopyOrMoveType = CopyOrMoveType.Mod;
                result = copyOrMoveWindow.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var directory = new DirectoryInfo(dialog.SelectedPath);
                    var addModTask = Mod.Add(directory, Mods, Modpacks, !copyOrMoveWindow.Move);

                    var progressWindow = new ProgressWindow() { Owner = Window };
                    var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                    progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("ProcessingModAction");
                    progressViewModel.ProgressDescription = directory.Name;
                    progressViewModel.IsIndeterminate = true;

                    addModTask = addModTask.ContinueWith(t =>
                    {
                        progressWindow.Dispatcher.Invoke(progressWindow.Close);
                        return t.Result;
                    });
                    progressWindow.ShowDialog();

                    await addModTask;
                    Mods.EvaluateDependencies();
                }
            }
        }

        #endregion

        private bool ContainsModpack(string name)
        {
            return Modpacks.Any(item => item.Name == name);
        }

        public void CreateNewModpack(ICollection<Mod> mods)
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

            if (mods != null)
            {
                foreach (Mod mod in mods)
                {
                    var reference = new ModReference(mod, modpack);
                    modpack.Mods.Add(reference);
                }

                modpack.ContentsExpanded = true;
            }

            ((MainWindow)View).ModpacksToggledPopup.IsOpen = false;
            modpack.Editing = true;
            Window.ModpacksListBox.ScrollIntoView(modpack);
        }

        public void CreateNewModpack()
        {
            CreateNewModpack(null);
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
                    string versionString =
                        (propertiesViewModel.UseExactVersion || propertiesViewModel.SelectedVersion.IsSpecialVersion)
                        ? propertiesViewModel.SelectedVersion.VersionString
                        : propertiesViewModel.SelectedVersion.Version.ToString(2);
                    string modpackName = propertiesViewModel.SelectedModpack?.Name;
                    string savegameName = propertiesViewModel.SelectedSavegame?.Name;
                    string customArgs = propertiesViewModel.Arguments?.Replace('"', '\'');

                    if ((versionString == FactorioSteamVersion.Key) && (propertiesViewModel.LoadGame || propertiesViewModel.UseArguments))
                    {
                        if (MessageBox.Show(Window,
                            App.Instance.GetLocalizedMessage("SteamCustomArgs", MessageType.Question),
                            App.Instance.GetLocalizedMessageTitle("SteamCustomArgs", MessageType.Question),
                            MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    string arguments = $"--factorio-version=\"{versionString}\"";
                    if (!string.IsNullOrEmpty(modpackName)) arguments += $" --modpack=\"{modpackName}\"";
                    if (propertiesViewModel.LoadGame) arguments += $" --savegame=\"{savegameName}\"";
                    if (propertiesViewModel.UseArguments) arguments += $" {customArgs}";
                    ShellHelper.CreateShortcut(dialog.FileName, applicationPath, arguments, iconPath);
                }
            }
        }

        private void StartGame()
        {
            foreach (var mod in Mods)
            {
                if (mod.Active && !mod.DependenciesActive)
                {
                    if (MessageBox.Show(Window,
                            App.Instance.GetLocalizedMessage("Dependencies", MessageType.Question),
                            App.Instance.GetLocalizedMessageTitle("Dependencies", MessageType.Question),
                            MessageBoxButton.YesNo, MessageBoxImage.Question)
                            == MessageBoxResult.Yes)
                    {
                        break;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            Process.Start(SelectedFactorioVersion.ExecutablePath);
        }

        private void OpenVersionManager()
        {
            var versionManagementWindow = new VersionManagementWindow() { Owner = Window };
            versionManagementWindow.ShowDialog();
        }

        #region Settings

        private async Task MoveFactorioDirectory(DirectoryInfo oldFactorioDirectory, DirectoryInfo newFactorioDirectory)
        {
            if (oldFactorioDirectory.Exists)
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
        }

        private async Task MoveModDirectory(DirectoryInfo oldModDirectory, DirectoryInfo newModDirectory)
        {
            if (oldModDirectory.Exists)
            {
                if (!newModDirectory.Exists) newModDirectory.Create();

                foreach (var mod in Mods)
                {
                    var dir = new DirectoryInfo(Path.Combine(newModDirectory.FullName, mod.FactorioVersion.ToString(2)));
                    if (!dir.Exists) dir.Create();
                    await mod.MoveToAsync(dir.FullName);
                }
                foreach (var version in FactorioVersions)
                {
                    if (!version.IsSpecialVersion)
                    {
                        var dir = new DirectoryInfo(Path.Combine(oldModDirectory.FullName, version.Version.ToString(2)));
                        if (dir.Exists)
                        {
                            var modListFile = new FileInfo(Path.Combine(dir.FullName, "mod-list.json"));
                            var newDir = new DirectoryInfo(Path.Combine(newModDirectory.FullName, version.Version.ToString(2)));
                            if (!newDir.Exists) newDir.Create();
                            if (modListFile.Exists) await modListFile.MoveToAsync(Path.Combine(newDir.FullName, "mod-list.json"));

                            dir.DeleteIfEmpty();
                        }
                    }
                }

                oldModDirectory.DeleteIfEmpty();
            }
        }

        private async Task MoveSavegameDirectory(DirectoryInfo oldSavegameDirectory, DirectoryInfo newSavegameDirectory)
        {
            if (oldSavegameDirectory.Exists)
                await oldSavegameDirectory.MoveToAsync(newSavegameDirectory.FullName);
        }

        private async Task MoveScenarioDirectory(DirectoryInfo oldScenarioDirectory, DirectoryInfo newScenarioDirectory)
        {
            if (oldScenarioDirectory.Exists && !newScenarioDirectory.DirectoryEquals(oldScenarioDirectory))
                await oldScenarioDirectory.MoveToAsync(newScenarioDirectory.FullName);
        }

        private async Task RecreateLinks()
        {
            await Task.Run(() =>
            {
                foreach (var version in FactorioVersions)
                    version.CreateLinks();
            });
        }

        private async Task MoveDirectoriesInternal(
            DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo oldSavegameDirectory, DirectoryInfo oldScenarioDirectory,
            DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory, DirectoryInfo newSavegameDirectory, DirectoryInfo newScenarioDirectory,
            bool moveFactorioDirectory, bool moveModDirectory, bool moveSavegameDirectory, bool moveScenarioDirectory)
        {
            if (moveFactorioDirectory)
                await MoveFactorioDirectory(oldFactorioDirectory, newFactorioDirectory);

            if (moveModDirectory)
                await MoveModDirectory(oldModDirectory, newModDirectory);

            if (moveSavegameDirectory)
                await MoveSavegameDirectory(oldSavegameDirectory, newSavegameDirectory);

            if (moveScenarioDirectory)
                await MoveScenarioDirectory(oldScenarioDirectory, newScenarioDirectory);

            await RecreateLinks();
        }

        private async Task MoveDirectories(
            DirectoryInfo oldFactorioDirectory, DirectoryInfo oldModDirectory, DirectoryInfo oldSavegameDirectory, DirectoryInfo oldScenarioDirectory,
            DirectoryInfo newFactorioDirectory, DirectoryInfo newModDirectory, DirectoryInfo newSavegameDirectory, DirectoryInfo newScenarioDirectory,
            bool moveFactorioDirectory, bool moveModDirectory, bool moveSavegameDirectory, bool moveScenarioDirectory)
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("MovingDirectoriesAction");
            progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("MovingFilesDescription");
            progressViewModel.IsIndeterminate = true;

            Task moveDirectoriesTask = MoveDirectoriesInternal(
                oldFactorioDirectory, oldModDirectory, oldSavegameDirectory, oldScenarioDirectory,
                newFactorioDirectory, newModDirectory, newSavegameDirectory, newScenarioDirectory,
                moveFactorioDirectory, moveModDirectory, moveSavegameDirectory, moveScenarioDirectory);

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
            bool managerModeChanged = (settingsViewModel.ManagerMode != settings.ManagerMode);
            settings.ManagerMode = settingsViewModel.ManagerMode;

            // Update search
            settings.UpdateSearchOnStartup = settingsViewModel.UpdateSearchOnStartup;
            settings.IncludePreReleasesForUpdate = settingsViewModel.IncludePreReleasesForUpdate;

            // Mod update
            settings.AlwaysUpdateZipped = settingsViewModel.AlwaysUpdateZipped;
            settings.KeepOldModVersions = settingsViewModel.KeepOldModVersions;
            settings.KeepOldExtractedModVersions = settingsViewModel.KeepExtracted;
            settings.KeepOldZippedModVersions = settingsViewModel.KeepZipped;
            settings.KeepOldModVersionsWhenNewFactorioVersion = settingsViewModel.KeepWhenNewFactorioVersion;
            settings.DownloadIntermediateUpdates = settingsViewModel.UpdateIntermediate;

            // Mod dependencies
            settings.ActivateDependencies = settingsViewModel.ActivateDependencies;
            settings.ActivateOptionalDependencies = settingsViewModel.ActivateOptionalDependencies;

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
                if (settingsWindow.PasswordBox.SecurePassword.Length > 0)
                {
                    GlobalCredentials.Instance.Username = settingsWindow.UsernameBox.Text;
                    GlobalCredentials.Instance.Password = settingsWindow.PasswordBox.SecurePassword;
                    GlobalCredentials.Instance.Save();
                }
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


            // Move directories
            bool moveFactorioDirectory = !newFactorioDirectory.DirectoryEquals(oldFactorioDirectory);
            bool moveModDirectory = !newModDirectory.DirectoryEquals(oldModDirectory);
            bool moveSavegameDirectory = !newSavegameDirectory.DirectoryEquals(oldSavegameDirectory);
            bool moveScenarioDirectory = !newScenarioDirectory.DirectoryEquals(oldScenarioDirectory);

            if (moveFactorioDirectory || moveModDirectory || moveSavegameDirectory || moveScenarioDirectory)
            {
                if (MessageBox.Show(Window,
                App.Instance.GetLocalizedMessage("MoveDirectories", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("MoveDirectories", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await MoveDirectories(
                        oldFactorioDirectory, oldModDirectory, oldSavegameDirectory, oldScenarioDirectory,
                        newFactorioDirectory, newModDirectory, newSavegameDirectory, newScenarioDirectory,
                        moveFactorioDirectory, moveModDirectory, moveSavegameDirectory, moveScenarioDirectory);
                }
            }


            // Reload everything if required
            if (managerModeChanged || moveFactorioDirectory || moveModDirectory)
            {
                Refresh();
            }
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
                    result = await App.Instance.SearchForUpdateAsync(App.Instance.Settings.IncludePreReleasesForUpdate);
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
                        string currentVersionString = App.Version.ToString();
                        string newVersionString = result.Version.ToString();
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

                Mods.EvaluateDependencies();
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

        private void SelectInactiveMods()
        {
            foreach (Mod mod in Mods)
            {
                if (!mod.Active)
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

        private void SelectInactiveModpacks()
        {
            foreach (Modpack modpack in Modpacks)
            {
                if (!(modpack.Active ?? false))
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
            await Window.Dispatcher.InvokeAsync(async () => await OnNewInstanceStarted(e.CommandLine, e.GameStarted));
        }

        private async Task OnNewInstanceStarted(CommandLine commandLine, bool gameStarted)
        {
            if (gameStarted)
            {
                Refresh();
            }
            else
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
