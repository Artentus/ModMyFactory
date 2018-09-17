using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Web;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using ModMyFactory.FactorioUpdate;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using ModMyFactory.Web.UpdateApi;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class VersionManagementViewModel : ViewModelBase
    {
        static VersionManagementViewModel instance;

        public static VersionManagementViewModel Instance = instance ?? (instance = new VersionManagementViewModel());

        public VersionManagementWindow Window => (VersionManagementWindow)View;

        FactorioVersion selectedVersion;

        public ListCollectionView FactorioVersionsView { get; }

        public ObservableCollection<FactorioVersion> FactorioVersions { get; }

        public ModCollection Mods { get; set; }

        public FactorioVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));
                }
            }
        }

        public RelayCommand DownloadCommand { get; }

        public RelayCommand AddFromZipCommand { get; }

        public RelayCommand AddFromFolderCommand { get; }

        public RelayCommand SelectSteamCommand { get; }

        public RelayCommand OpenFolderCommand { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand RemoveCommand { get; }

        private VersionManagementViewModel()
        {
            if (!App.IsInDesignMode)
            {
                FactorioVersions = MainViewModel.Instance.FactorioVersions;
                FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
                FactorioVersionsView.CustomSort = new FactorioVersionSorter();
                FactorioVersionsView.Filter = item => !((FactorioVersion)item).IsSpecialVersion;

                Mods = MainViewModel.Instance.Mods;

                DownloadCommand = new RelayCommand(async () => await DownloadOnlineVersion());
                AddFromZipCommand = new RelayCommand(async () => await AddZippedVersion());
                AddFromFolderCommand = new RelayCommand(async () => await AddLocalVersion());
                SelectSteamCommand = new RelayCommand(async () => await SelectSteamVersion(), () => string.IsNullOrEmpty(App.Instance.Settings.SteamVersionPath));
                OpenFolderCommand = new RelayCommand(OpenFolder, () => SelectedVersion != null);
                UpdateCommand = new RelayCommand(async () => await UpdateSelectedVersion(), () => SelectedVersion != null && SelectedVersion.IsFileSystemEditable);
                RemoveCommand = new RelayCommand(async () => await RemoveSelectedVersion(), () => SelectedVersion != null && SelectedVersion.IsFileSystemEditable);
            }
        }

        private bool VersionAlreadyInstalled(FactorioOnlineVersion version)
        {
            foreach (var localVersion in FactorioVersions)
            {
                if (version.Version == localVersion.Version)
                    return true;
            }

            return false;
        }

        private bool GetVersionDownloadable(FactorioOnlineVersion version)
        {
            foreach (var localVersion in FactorioVersions)
            {
                if ((version.Version == localVersion.Version) && localVersion.IsFileSystemEditable)
                    return false;
            }

            return true;
        }

        private bool ShowVersionList(CookieContainer container, out FactorioOnlineVersion selectedVersion)
        {
            selectedVersion = null;
            List<FactorioOnlineVersion> versions;
            try
            {
                if (!FactorioWebsite.GetVersions(container, out versions))
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("RetrievingVersions", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("RetrievingVersions", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("RetrievingVersions", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("RetrievingVersions", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            versions.ForEach(item => item.Downloadable = GetVersionDownloadable(item));

            var versionListWindow = new VersionListWindow { Owner = Window };
            var versionListViewModel = (VersionListViewModel)versionListWindow.ViewModel;
            versions.ForEach(item => versionListViewModel.FactorioVersions.Add(item));

            bool? versionResult = versionListWindow.ShowDialog();
            selectedVersion = versionListViewModel.SelectedVersion;
            return versionResult.HasValue && versionResult.Value;
        }

        private async Task DownloadOnlineVersion()
        {
            CookieContainer container;
            if (GlobalCredentials.Instance.LogIn(Window, out container))
            {
                FactorioOnlineVersion selectedVersion;
                if (ShowVersionList(container, out selectedVersion))
                {
                    var cancellationSource = new CancellationTokenSource();
                    var progressWindow = new ProgressWindow { Owner = Window };
                    var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                    progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");
                    progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), selectedVersion.DownloadUrl);
                    progressViewModel.CanCancel = true;
                    progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                    FactorioVersion newVersion;
                    try
                    {
                        Task closeWindowTask = null;
                        try
                        {
                            DirectoryInfo directory = App.Instance.Settings.GetFactorioDirectory();
                            Task<FactorioVersion> downloadTask = FactorioWebsite.DownloadFactorioPackageAsync(selectedVersion, directory, container, new Progress<double>(p =>
                            {
                                if (p > 1)
                                {
                                    progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("ExtractingDescription");
                                    progressViewModel.IsIndeterminate = true;
                                    progressViewModel.CanCancel = false;
                                }
                                else
                                {
                                    progressViewModel.Progress = p;
                                }
                            }), cancellationSource.Token);

                            closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                            progressWindow.ShowDialog();

                            newVersion = await downloadTask;
                        }
                        finally
                        {
                            if (closeWindowTask != null) await closeWindowTask;
                        }
                    }
                    catch (HttpRequestException)
                    {
                        MessageBox.Show(Window,
                            App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                            App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (newVersion != null) FactorioVersions.Add(newVersion);
                }
            }
        }

        private async Task AddZippedVersion()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = App.Instance.GetLocalizedResourceString("ZipDescription") + @" (*.zip)|*.zip";
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var archiveFile = new FileInfo(dialog.FileName);
                Version version = null;
                DirectoryInfo versionDirectory = null;

                var progressWindow = new ProgressWindow() { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("AddingFromZipAction");
                progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("CheckingValidityDescription");
                progressViewModel.IsIndeterminate = true;

                bool invalidArchiveFile = false;
                bool invalidPlatform = false;
                bool versionInstalled = false;
                IProgress<int> progress = new Progress<int>(stage =>
                {
                    switch (stage)
                    {
                        case 1:
                            progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("ExtractingDescription");
                            break;
                        case -1:
                            invalidArchiveFile = true;
                            break;
                        case -2:
                            invalidPlatform = true;
                            break;
                        case -3:
                            versionInstalled = true;
                            break;
                    }
                });

                Task extractTask = Task.Run(() =>
                {
                    bool is64Bit;
                    if (FactorioVersion.ArchiveFileValid(archiveFile, out version, out is64Bit))
                    {
                        if (is64Bit == Environment.Is64BitOperatingSystem)
                        {
                            if (FactorioVersions.All(factorioVersion => factorioVersion.Version != version))
                            {
                                progress.Report(1);

                                DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                                ZipFile.ExtractToDirectory(archiveFile.FullName, factorioDirectory.FullName);

                                string versionString = version.ToString(3);
                                versionDirectory = factorioDirectory.EnumerateDirectories($"Factorio_{versionString}*").First();
                                versionDirectory.MoveTo(Path.Combine(factorioDirectory.FullName, versionString));
                            }
                            else
                            {
                                progress.Report(-3);
                            }
                        }
                        else
                        {
                            progress.Report(-2);
                        }
                    }
                    else
                    {
                        progress.Report(-1);
                    }
                });

                Task closeWindowTask =
                    extractTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await extractTask;
                await closeWindowTask;

                if (invalidArchiveFile)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InvalidFactorioArchive", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InvalidFactorioArchive", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (invalidPlatform)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("IncompatiblePlatform", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("IncompatiblePlatform", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (versionInstalled)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("FactorioVersionInstalled", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("FactorioVersionInstalled", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    FactorioVersions.Add(new FactorioVersion(versionDirectory, version));

                    if (MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("DeleteFactorioArchive", MessageType.Question),
                        App.Instance.GetLocalizedMessageTitle("DeleteFactorioArchive", MessageType.Question),
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        archiveFile.Delete();
                    }
                }
            }
        }

        #region PreserveContents

        private void PreserveSavegames(DirectoryInfo localSaveDirectory, DirectoryInfo globalSaveDirectory, bool move)
        {
            foreach (var saveFile in localSaveDirectory.GetFiles())
            {
                if (!saveFile.Name.StartsWith("_autosave"))
                {
                    string newPath = Path.Combine(globalSaveDirectory.FullName, saveFile.Name);
                    if (File.Exists(newPath))
                    {
                        int count = 1;
                        do
                        {
                            string newName = $"{saveFile.NameWithoutExtension()}_{count}{saveFile.Extension}";
                            newPath = Path.Combine(globalSaveDirectory.FullName, newName);
                            count++;
                        } while (File.Exists(newPath));
                    }

                    if (move)
                        saveFile.MoveTo(newPath);
                    else
                        saveFile.CopyTo(newPath);
                }
            }

            if (move) localSaveDirectory.Delete(true);
        }

        private void PreserveScenarios(DirectoryInfo localScenarioDirectory, DirectoryInfo globalScenarioDirectory, bool move)
        {
            foreach (var scenarioFile in localScenarioDirectory.GetFiles())
            {
                string newPath = Path.Combine(globalScenarioDirectory.FullName, scenarioFile.Name);
                if (File.Exists(newPath))
                {
                    int count = 1;
                    do
                    {
                        string newName = $"{scenarioFile.NameWithoutExtension()}_{count}{scenarioFile.Extension}";
                        newPath = Path.Combine(globalScenarioDirectory.FullName, newName);
                        count++;
                    } while (File.Exists(newPath));
                }

                if (move)
                    scenarioFile.MoveTo(newPath);
                else
                    scenarioFile.CopyTo(newPath);
            }

            if (move) localScenarioDirectory.Delete(true);
        }

        private async Task PreserveMods(DirectoryInfo localModDirectory, bool move)
        {
            foreach (var file in localModDirectory.GetFiles("*.zip"))
            {
                if (ModFile.TryLoadFromFile(file, out var modFile))
                    await Mod.Add(modFile, Mods, MainViewModel.Instance.Modpacks, !move, true);
            }

            foreach (var directory in localModDirectory.GetDirectories())
            {
                if (ModFile.TryLoadFromDirectory(directory, out var modFile))
                    await Mod.Add(modFile, Mods, MainViewModel.Instance.Modpacks, !move, true);
            }

            if (move) localModDirectory.Delete(true);
        }

        private async Task PreserveContentsAsync(DirectoryInfo sourceDirectory, bool move)
        {
            await Task.Run(() =>
            {
                // Savegames
                var localSaveDirectory = new DirectoryInfo(Path.Combine(sourceDirectory.FullName, "saves"));
                if (localSaveDirectory.Exists)
                {
                    DirectoryInfo globalSaveDirectory = App.Instance.Settings.GetSavegameDirectory();
                    if (!globalSaveDirectory.Exists) globalSaveDirectory.Create();

                    PreserveSavegames(localSaveDirectory, globalSaveDirectory, move);
                }


                // Scenarios
                var localScenarioDirectory = new DirectoryInfo(Path.Combine(sourceDirectory.FullName, "scenarios"));
                if (localScenarioDirectory.Exists)
                {
                    DirectoryInfo globalScenarioDirectory = App.Instance.Settings.GetScenarioDirectory();
                    if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

                    PreserveScenarios(localScenarioDirectory, globalScenarioDirectory, move);
                }
            });

            // Mods
            var localModDirectory = new DirectoryInfo(Path.Combine(sourceDirectory.FullName, "mods"));
            if (localModDirectory.Exists)
            {
                await PreserveMods(localModDirectory, move);
            }
            
        }

        #endregion

        private async Task MoveFactorioInstallationAsync(DirectoryInfo installationDirectory, DirectoryInfo destinationDirectory)
        {
            await PreserveContentsAsync(installationDirectory, true);
            await installationDirectory.MoveToAsync(destinationDirectory.FullName);
        }

        private async Task CopyFactorioInstallationAsync(DirectoryInfo installationDirectory, DirectoryInfo destinationDirectory)
        {
            await PreserveContentsAsync(installationDirectory, false);
            await installationDirectory.CopyToAsync(destinationDirectory.FullName);
        }

        private async Task AddLocalVersion()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var installationDirectory = new DirectoryInfo(dialog.SelectedPath);
                Version version;

                bool is64Bit;
                if (!FactorioVersion.LocalInstallationValid(installationDirectory, out version, out is64Bit))
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InvalidFactorioFolder", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InvalidFactorioFolder", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (is64Bit != Environment.Is64BitOperatingSystem)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("IncompatiblePlatform", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("IncompatiblePlatform", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (FactorioVersions.Any(factorioVersion => factorioVersion.Version == version))
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("FactorioVersionInstalled", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("FactorioVersionInstalled", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                var copyOrMoveWindow = new CopyOrMoveMessageWindow() { Owner = Window };
                ((CopyOrMoveViewModel)copyOrMoveWindow.ViewModel).CopyOrMoveType = CopyOrMoveType.Factorio;
                result = copyOrMoveWindow.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    bool move = copyOrMoveWindow.Move;

                    DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                    if (!factorioDirectory.Exists) factorioDirectory.Create();
                    DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(factorioDirectory.FullName, version.ToString(3)));

                    var progressWindow = new ProgressWindow() { Owner = Window };
                    var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                    progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("AddingLocalInstallationAction");
                    progressViewModel.ProgressDescription = move ? App.Instance.GetLocalizedResourceString("MovingFilesDescription") : App.Instance.GetLocalizedResourceString("CopyingFilesDescription");
                    progressViewModel.IsIndeterminate = true;

                    Task addTask = move
                        ? MoveFactorioInstallationAsync(installationDirectory, destinationDirectory)
                        : CopyFactorioInstallationAsync(installationDirectory, destinationDirectory);

                    Task closeWindowTask = addTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    await addTask;
                    await closeWindowTask;

                    FactorioVersions.Add(new FactorioVersion(destinationDirectory, version));
                }
            }
        }

        private async Task SelectSteamVersion()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog(Window);

            if (result.HasValue && result.Value)
            {
                var selectedDirectory = new DirectoryInfo(dialog.SelectedPath);
                Version version;

                bool is64Bit;
                if (!FactorioVersion.LocalInstallationValid(selectedDirectory, out version, out is64Bit))
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InvalidFactorioFolder", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InvalidFactorioFolder", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (is64Bit != Environment.Is64BitOperatingSystem)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("IncompatiblePlatform", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("IncompatiblePlatform", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("MoveSteamFactorio", MessageType.Warning),
                    App.Instance.GetLocalizedMessageTitle("MoveSteamFactorio", MessageType.Warning),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    App.Instance.Settings.SteamVersionPath = selectedDirectory.FullName;
                    App.Instance.Settings.Save();

                    var progressWindow = new ProgressWindow() { Owner = Window };
                    var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                    progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("AddingSteamVersionAction");
                    progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("MovingFilesDescription");
                    progressViewModel.IsIndeterminate = true;

                    var steamAppDataDirectory = new DirectoryInfo(FactorioSteamVersion.SteamAppDataPath);
                    Task moveTask = PreserveContentsAsync(steamAppDataDirectory, true);

                    Task closeWindowTask = moveTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();
                    await moveTask;
                    await closeWindowTask;

                    FactorioVersions.Add(new FactorioSteamVersion(selectedDirectory, version));
                }
            }
        }

        private void OpenFolder()
        {
            Process.Start(SelectedVersion.Directory.FullName);
        }

        private async Task UpdateSelectedVersion()
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                UpdateInfo updateInfo = UpdateWebsite.GetUpdateInfo(GlobalCredentials.Instance.Username, token);
                List<UpdateStep> updateSteps = updateInfo.Package.Where(step => step.From >= SelectedVersion.Version).ToList();

                if (updateSteps.Count > 0)
                {
                    List<UpdateTarget> targets = FactorioUpdater.GetUpdateTargets(SelectedVersion, FactorioVersions, updateSteps);

                    var updateListWindow = new UpdateListWindow() { Owner = Window };
                    var updateListViewModel = (UpdateListViewModel)updateListWindow.ViewModel;
                    updateListViewModel.UpdateTargets = targets;
                    bool? result = updateListWindow.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        UpdateTarget target = updateListViewModel.SelectedTarget;

                        var progressWindow = new ProgressWindow { Owner = Window };
                        var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                        progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("UpdatingFactorioAction");

                        var cancellationSource = new CancellationTokenSource();
                        progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                        var progress = new Progress<double>(value => progressViewModel.Progress = value );
                        var stageProgress = new Progress<UpdaterStageInfo>(value =>
                        {
                            progressViewModel.CanCancel = value.CanCancel;
                            progressViewModel.ProgressDescription = value.Description;
                        });

                        try
                        {
                            Task closeWindowTask = null;
                            try
                            {
                                Task updateTask = FactorioUpdater.ApplyUpdateAsync(SelectedVersion,
                                    GlobalCredentials.Instance.Username, token, target,
                                    progress, stageProgress, cancellationSource.Token);

                                closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                                progressWindow.ShowDialog();

                                await updateTask;
                            }
                            finally
                            {
                                if (closeWindowTask != null) await closeWindowTask;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            MessageBox.Show(Window,
                                App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                                App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (CriticalUpdaterException)
                        {
                            MessageBox.Show(Window,
                                App.Instance.GetLocalizedMessage("FactorioUpdaterCritical", MessageType.Error),
                                App.Instance.GetLocalizedMessageTitle("FactorioUpdaterCritical", MessageType.Error),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("NoFactorioUpdate", MessageType.Information),
                        App.Instance.GetLocalizedMessageTitle("NoFactorioUpdate", MessageType.Information),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async Task RemoveSelectedVersion()
        {
            if (MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("RemoveFactorioVersion", MessageType.Question),
                    App.Instance.GetLocalizedMessageTitle("RemoveFactorioVersion", MessageType.Question),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("RemovingFactorioVersionAction");
                progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("DeletingFilesDescription");
                progressViewModel.IsIndeterminate = true;

                Task deleteTask = Task.Run(() =>
                {
                    SelectedVersion.DeleteLinks();
                    SelectedVersion.Directory.Delete(true);
                });

                Task closeWindowTask = deleteTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await deleteTask;
                await closeWindowTask;

                FactorioVersions.Remove(SelectedVersion);
            }
        }
    }
}
