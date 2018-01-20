using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
        ListCollectionView selectedReleasesView;
        ModRelease[] selectedReleases; 

        volatile int asyncFetchExtendedInfoIndex;
        ModInfo selectedMod;
        ExtendedModInfo extendedInfo;

        string selectedModName;
        string selectedModDescription;
        string selectedModLicense;
        string selectedModHomepage;
        string selectedModGitHubUrl;

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

        public ListCollectionView SelectedReleasesView
        {
            get { return selectedReleasesView; }
            private set
            {
                if (value != selectedReleasesView)
                {
                    selectedReleasesView = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedReleasesView)));
                }
            }
            
        }

        public ModRelease[] SelectedReleases
        {
            get { return selectedReleases; }
            private set
            {
                if (value != selectedReleases)
                {
                    selectedReleases = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedReleases)));

                    if (selectedReleases != null)
                    {
                        SelectedReleasesView = (ListCollectionView)(new CollectionViewSource() { Source = selectedReleases }).View;
                        SelectedReleasesView.CustomSort = new ModReleaseSorter();
                    }
                    else
                    {
                        SelectedReleasesView = null;
                    }
                }
            }
        }

        public ModCollection InstalledMods { get; }

        public ModpackCollection InstalledModpacks { get; }

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

                    if (selectedMod != null)
                    {
                        SelectedModName = selectedMod.Title;
                        //SelectedModLicense = selectedMod.License;
                        //SelectedModHomepage = selectedMod.Homepage;
                        //SelectedModGitHubUrl = selectedMod.GitHubUrl;

                        ExtendedInfo = null;
                        asyncFetchExtendedInfoIndex++;
                        new Action(async () => await LoadExtendedModInfoAsync(selectedMod, asyncFetchExtendedInfoIndex)).Invoke();
                    }
                    else
                    {
                        SelectedModName = string.Empty;
                        SelectedModLicense = string.Empty;
                        SelectedModHomepage = string.Empty;
                        SelectedModGitHubUrl = string.Empty;

                        ExtendedInfo = null;
                    }
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

                if (extendedInfo != null)
                {
                    //SelectedModDescription = extendedInfo.Description;
                    foreach (var release in extendedInfo.Releases)
                    {
                        release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                        release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.InfoFile.FactorioVersion);
                    }

                    SelectedReleases = extendedInfo.Releases;
                    SelectedRelease = SelectedReleases.MinBy(item => item, new ModReleaseSorter());
                }
                else
                {
                    selectedModDescription = string.Empty;
                    SelectedReleases = null;
                    SelectedRelease = null;
                }
                
                CommandManager.InvalidateRequerySuggested();
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

        public string SelectedModLicense
        {
            get { return selectedModLicense; }
            set
            {
                if (value != selectedModLicense)
                {
                    selectedModLicense = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModLicense)));
                }
            }
        }

        public string SelectedModHomepage
        {
            get { return selectedModHomepage; }
            set
            {
                if (value != selectedModHomepage)
                {
                    selectedModHomepage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModHomepage)));
                }
            }
        }

        public string SelectedModGitHubUrl
        {
            get { return selectedModGitHubUrl; }
            set
            {
                if (value != selectedModGitHubUrl)
                {
                    selectedModGitHubUrl = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModGitHubUrl)));
                }
            }
        }

        public RelayCommand DownloadCommand { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand DeleteCommand { get; }

        public RelayCommand RefreshCommand { get; }

        public RelayCommand OpenLicenseLinkCommand { get; }

        public RelayCommand OpenHomepageCommand { get; }

        public RelayCommand OpenGitHubLinkCommand { get; }

        public RelayCommand ClearFilterCommand { get; }

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
            InstalledModpacks = MainViewModel.Instance.Modpacks;

            asyncFetchExtendedInfoIndex = -1;

            DownloadCommand = new RelayCommand(async () => await DownloadSelectedModRelease(), () => SelectedRelease != null && !SelectedRelease.IsInstalled);
            UpdateCommand = new RelayCommand(async () => await UpdateSelectedModRelease(), () =>
            {
                ModRelease newestRelease = GetNewestRelease(ExtendedInfo);
                return (SelectedRelease != null) && SelectedRelease.IsInstalled && !newestRelease.IsInstalled && (SelectedRelease != newestRelease);
            });
            DeleteCommand = new RelayCommand(DeleteSelectedModRelease, () => SelectedRelease != null && SelectedRelease.IsInstalled);
            RefreshCommand = new RelayCommand(async () => await RefreshModList());

            OpenLicenseLinkCommand = new RelayCommand(() =>
            {
                //string url = SelectedMod.LicenseUrl;
                //if (!string.IsNullOrWhiteSpace(url))
                //{
                //    try
                //    {
                //        Process.Start(url);
                //    }
                //    catch { }
                //}
            });
            OpenHomepageCommand = new RelayCommand(() =>
            {
                //string url = SelectedMod.Homepage;
                //if (!string.IsNullOrWhiteSpace(url))
                //{
                //    try
                //    {
                //        Process.Start(url);
                //    }
                //    catch { }
                //}
            });
            OpenGitHubLinkCommand = new RelayCommand(() =>
            {
                const string prefix = "https://www.github.com/";

                //string url = SelectedMod.GitHubUrl;
                //if (!url.StartsWith(prefix)) url = prefix + url;

                //if (!string.IsNullOrWhiteSpace(url))
                //{
                //    try
                //    {
                //        Process.Start(url);
                //    }
                //    catch { }
                //}
            });

            ClearFilterCommand = new RelayCommand(() => Filter = string.Empty);
        }

        public void UpdateSelectedReleases()
        {
            foreach (var release in SelectedReleases)
            {
                release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.InfoFile.FactorioVersion);
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

        private ModRelease GetNewestRelease(ExtendedModInfo info)
        {
            return info?.Releases.MaxBy(release => release.Version, new VersionComparer());
        }

        private async Task UpdateModAsyncInner(Mod oldMod, ModRelease newestRelease, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            FileInfo modFile = await ModWebsite.UpdateReleaseAsync(newestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken);
            Mod newMod;

            if (App.Instance.Settings.AlwaysUpdateZipped || (oldMod is ZippedMod))
            {
                newMod = new ZippedMod(oldMod.Name, newestRelease.Version, newestRelease.InfoFile.FactorioVersion, modFile, InstalledMods, InstalledModpacks);
            }
            else
            {
                DirectoryInfo modDirectory = await Task.Run(() =>
                {
                    progress.Report(2);
                    DirectoryInfo modsDirectory = App.Instance.Settings.GetModDirectory(newestRelease.InfoFile.FactorioVersion);
                    ZipFile.ExtractToDirectory(modFile.FullName, modsDirectory.FullName);
                    modFile.Delete();

                    return new DirectoryInfo(Path.Combine(modsDirectory.FullName, modFile.NameWithoutExtension()));
                });

                newMod = new ExtractedMod(oldMod.Name, newestRelease.Version, newestRelease.InfoFile.FactorioVersion, modDirectory, InstalledMods, InstalledModpacks);
            }

            InstalledMods.Add(newMod);
            if (oldMod.Update(newMod)) InstalledModpacks.ExchangeMods(oldMod, newMod);

            ModpackTemplateList.Instance.Update(InstalledModpacks);
            ModpackTemplateList.Instance.Save();
        }

        private async Task UpdateSelectedModRelease()
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                ModRelease newestRelease = GetNewestRelease(ExtendedInfo);
                Mod oldMod = InstalledMods.Find(SelectedMod.Name).MaxBy(mod => mod.Version);

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
                        Task updateTask = UpdateModAsyncInner(oldMod, newestRelease, token, progress, cancellationSource.Token);

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
                    return;
                }

                SelectedRelease = null;
                foreach (var release in SelectedReleases)
                {
                    release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                    release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.InfoFile.FactorioVersion);
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
