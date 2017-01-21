using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class OnlineModsViewModel : ViewModelBase
    {
        static OnlineModsViewModel instance;

        public static OnlineModsViewModel Instance => instance ?? (instance = new OnlineModsViewModel());

        public OnlineModsWindow Window => (OnlineModsWindow)View;

        ListCollectionView modsView;
        List<ModInfo> mods;
        string filter;
        ModRelease selectedRelease;

        volatile int asyncFetchExtendedInfoIndex;
        ModInfo selectedMod;
        ExtendedModInfo extendedInfo;
        string selectedModName;
        string selectedModDescription;

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

        public List<ModInfo> Mods
        {
            get { return mods; }
            set
            {
                if (value != mods)
                {
                    mods = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));

                    ModsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Mods);
                    ModsView.Filter = ModFilter;
                    ModsView.CustomSort = new ModInfoSorter();
                }
            }
        }

        public ModCollection InstalledMods { get; }

        public string Filter
        {
            get { return filter; }
            set
            {
                if (value != filter)
                {
                    filter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Filter)));

                    ModsView.Refresh();
                }
            }
        }

        public ModRelease SelectedRelease
        {
            get { return selectedRelease; }
            set
            {
                if (value != selectedRelease)
                {
                    selectedRelease = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedRelease)));
                }
            }
        }

        public ModInfo SelectedMod
        {
            get { return selectedMod; }
            set
            {
                if (value != selectedMod)
                {
                    selectedMod = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMod)));

                    SelectedModName = selectedMod.Title;
                    SelectedRelease = null;

                    asyncFetchExtendedInfoIndex++;
                    new Action(async () => await LoadExtendedModInfoAsync(selectedMod, asyncFetchExtendedInfoIndex)).Invoke();
                }
            }
        }

        public ExtendedModInfo ExtendedInfo
        {
            get { return extendedInfo; }
            private set
            {
                extendedInfo = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ExtendedInfo)));

                SelectedModDescription = extendedInfo.Description;
                SelectedReleases.Clear();
                foreach (var release in extendedInfo.Releases)
                {
                    release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                    release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.FactorioVersion);
                    SelectedReleases.Add(release);
                }
            }
        }

        public string SelectedModName
        {
            get { return selectedModName; }
            private set
            {
                if (value != selectedModName)
                {
                    selectedModName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModName)));
                }
            }
        }

        public string SelectedModDescription
        {
            get { return string.IsNullOrWhiteSpace(selectedModDescription) ? selectedMod?.Summary ?? string.Empty : selectedModDescription; }
            set
            {
                if (value != selectedModDescription)
                {
                    selectedModDescription = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModDescription)));
                }
            }
        }

        public ObservableCollection<ModRelease> SelectedReleases { get; }

        public RelayCommand DownloadCommand { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand DeleteCommand { get; }

        public RelayCommand RefreshCommand { get; }

        private async Task LoadExtendedModInfoAsync(ModInfo mod, int operationIndex)
        {
            ExtendedModInfo extendedInfo;
            try
            {
                extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod);
            }
            catch (WebException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (operationIndex == asyncFetchExtendedInfoIndex) ExtendedInfo = extendedInfo;
        }

        private bool ModFilter(object item)
        {
            ModInfo mod = item as ModInfo;
            if (mod == null) return false;

            if (string.IsNullOrWhiteSpace(filter)) return true;

            return StringHelper.FilterIsContained(filter, $"{mod.Title} {mod.Author}");
        }

        private OnlineModsViewModel()
        {
            InstalledMods = MainViewModel.Instance.Mods;

            SelectedReleases = new ObservableCollection<ModRelease>();
            asyncFetchExtendedInfoIndex = -1;

            DownloadCommand = new RelayCommand(async () => await DownloadSelectedModRelease(), () => SelectedRelease != null && !SelectedRelease.IsInstalled);
            UpdateCommand = new RelayCommand(async () => await UpdateSelectedModRelease(), () => SelectedRelease != null && SelectedRelease.IsInstalled &&
                    SelectedRelease != GetNewestRelease(ExtendedInfo, SelectedRelease));
            DeleteCommand = new RelayCommand(DeleteSelectedModRelease, () => SelectedRelease != null && SelectedRelease.IsInstalled);
            RefreshCommand = new RelayCommand(async () => await RefreshModList());
        }

        public void UpdateSelectedReleases()
        {
            foreach (var release in SelectedReleases)
            {
                release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.FactorioVersion);
            }
        }

        private async Task DownloadSelectedModRelease()
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                var progressWindow = new ProgressWindow { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");
                progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), selectedRelease.FileName);

                progressViewModel.CanCancel = true;
                var cancellationSource = new CancellationTokenSource();
                progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                var progress = new Progress<double>(p => progressViewModel.Progress = p);

                Mod newMod;
                try
                {
                    Task closeWindowTask = null;
                    try
                    {
                        Task<Mod>  downloadTask = ModWebsite.DownloadReleaseAsync(selectedRelease,
                            GlobalCredentials.Instance.Username, token,
                            progress, cancellationSource.Token, InstalledMods, MainViewModel.Instance.Modpacks);

                        closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                        progressWindow.ShowDialog();

                        newMod = await downloadTask;
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

                if (!cancellationSource.IsCancellationRequested)
                {
                    if (newMod != null) InstalledMods.Add(newMod);
                    UpdateSelectedReleases();
                }
            }
        }

        private ModRelease GetNewestRelease(ExtendedModInfo info, ModRelease currentRelease)
        {
            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                return info.Releases.Where(release => release.FactorioVersion == currentRelease.FactorioVersion)
                    .MaxBy(release => release.Version, new VersionComparer());
            }
            else
            {
                return info.Releases.MaxBy(release => release.Version, new VersionComparer());
            }
        }

        private async Task UpdateSelectedModRelease()
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                ModRelease newestRelease = GetNewestRelease(ExtendedInfo, SelectedRelease);
                Mod mod = InstalledMods.FindByFactorioVersion(SelectedMod.Name, newestRelease.FactorioVersion);
                var zippedMod = mod as ZippedMod;
                var extractedMod = mod as ExtractedMod;

                var cancellationSource = new CancellationTokenSource();
                var progressWindow = new ProgressWindow { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("UpdatingAction");
                progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), newestRelease.FileName);
                progressViewModel.CanCancel = true;
                progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                IProgress<double> progress = new Progress<double>(p =>
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
                });

                try
                {
                    Task closeWindowTask = null;
                    try
                    {
                        Task downloadTask = ModWebsite.UpdateReleaseAsync(newestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationSource.Token);

                        if (extractedMod != null)
                        {
                            downloadTask = downloadTask.ContinueWith(t =>
                            {
                                progress.Report(2);

                                FileInfo modFile = ((Task<FileInfo>)t).Result;
                                DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(newestRelease.FactorioVersion);
                                ZipFile.ExtractToDirectory(modFile.FullName, modDirectory.FullName);
                                modFile.Delete();

                                return new DirectoryInfo(Path.Combine(modDirectory.FullName, modFile.NameWithoutExtension()));
                            }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);
                        }

                        closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                        progressWindow.ShowDialog();

                        if (zippedMod != null)
                        {
                            FileInfo newModFile = await (Task<FileInfo>)downloadTask;
                            if (zippedMod.FactorioVersion == newestRelease.FactorioVersion)
                            {
                                zippedMod.Update(newModFile, newestRelease.Version);
                            }
                            else
                            {
                                var newMod = new ZippedMod(zippedMod.Name, newestRelease.Version, newestRelease.FactorioVersion, newModFile,
                                    InstalledMods, MainViewModel.Instance.Modpacks);
                                InstalledMods.Add(newMod);
                                foreach (var modpack in MainViewModel.Instance.Modpacks)
                                {
                                    ModReference reference;
                                    if (modpack.Contains(zippedMod, out reference))
                                    {
                                        modpack.Mods.Remove(reference);
                                        modpack.Mods.Add(new ModReference(newMod, modpack));
                                    }
                                }
                                zippedMod.File.Delete();
                                InstalledMods.Remove(extractedMod);
                            }
                        }
                        if (extractedMod != null)
                        {
                            DirectoryInfo newModDirectory = await (Task<DirectoryInfo>)downloadTask;
                            if (extractedMod.FactorioVersion == newestRelease.FactorioVersion)
                            {
                                extractedMod.Update(newModDirectory, newestRelease.Version);
                            }
                            else
                            {
                                var newMod = new ExtractedMod(extractedMod.Name, newestRelease.Version, newestRelease.FactorioVersion, newModDirectory,
                                    InstalledMods, MainViewModel.Instance.Modpacks);
                                InstalledMods.Add(newMod);
                                foreach (var modpack in MainViewModel.Instance.Modpacks)
                                {
                                    ModReference reference;
                                    if (modpack.Contains(extractedMod, out reference))
                                    {
                                        modpack.Mods.Remove(reference);
                                        modpack.Mods.Add(new ModReference(newMod, modpack));
                                    }
                                }
                                extractedMod.Directory.Delete(true);
                                InstalledMods.Remove(extractedMod);

                                ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                                ModpackTemplateList.Instance.Save();
                            }
                        }
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

                SelectedRelease = null;
                foreach (var release in SelectedReleases)
                {
                    release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                    release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.FactorioVersion);
                }
            }
        }

        private void DeleteSelectedModRelease()
        {
            Mod mod = InstalledMods.Find(SelectedMod.Name, SelectedRelease.Version);
            mod?.Delete(true);
            UpdateSelectedReleases();
        }

        private async Task RefreshModList()
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
                Mods = modInfos;
            }
        }
    }
}
