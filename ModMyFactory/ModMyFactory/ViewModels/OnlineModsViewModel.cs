using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        const int ModTitleMinLength = 2;
        static readonly string[] ModBlacklist = { "[Abandoned]", "[Deprecated]", "[Discontinued]", "[Outdated]" };

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
        string selectedModChangelog;
        string selectedModFaq;
        string selectedModLicenseName;
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

                        ExtendedInfo = null;
                        asyncFetchExtendedInfoIndex++;
                        new Action(async () => await LoadExtendedModInfoAsync(selectedMod, asyncFetchExtendedInfoIndex)).Invoke();
                    }
                    else
                    {
                        SelectedModName = string.Empty;
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
                    SelectedModDescription = extendedInfo.Description;
                    SelectedModChangelog = extendedInfo.Changelog;
                    SelectedModFaq = extendedInfo.Faq;
                    SelectedModLicenseName = extendedInfo.License?.Name;
                    SelectedModHomepage = extendedInfo.Homepage;
                    SelectedModGitHubUrl = extendedInfo.GitHubUrl;
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
                    SelectedModDescription = string.Empty;
                    SelectedModChangelog = string.Empty;
                    SelectedModFaq = string.Empty;
                    SelectedModLicenseName = string.Empty;
                    SelectedModHomepage = string.Empty;
                    SelectedModGitHubUrl = string.Empty;

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
            get { return (string.IsNullOrWhiteSpace(selectedModDescription) || (selectedModDescription == "."))
                    ? selectedMod?.Summary ?? string.Empty : selectedModDescription; }
            set
            {
                if (value != selectedModDescription)
                {
                    selectedModDescription = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModDescription)));
                }
            }
        }

        public string SelectedModChangelog
        {
            get { return selectedModChangelog; }
            set
            {
                if (value != selectedModChangelog)
                {
                    selectedModChangelog = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModChangelog)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShowChangelog)));
                }
            }
        }

        public bool ShowChangelog => !string.IsNullOrWhiteSpace(SelectedModChangelog);

        public string SelectedModFaq
        {
            get { return selectedModFaq; }
            set
            {
                if (value != selectedModFaq)
                {
                    selectedModFaq = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModFaq)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShowFaq)));
                }
            }
        }

        public bool ShowFaq => !string.IsNullOrWhiteSpace(SelectedModFaq);

        public string SelectedModLicenseName
        {
            get { return (ExtendedInfo != null && string.IsNullOrEmpty(selectedModLicenseName)) ? "N/A" : selectedModLicenseName; }
            set
            {
                if (value != selectedModLicenseName)
                {
                    selectedModLicenseName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModLicenseName)));
                }
            }
        }

        public string SelectedModHomepage
        {
            get { return (ExtendedInfo != null && string.IsNullOrEmpty(selectedModHomepage)) ? "N/A" : selectedModHomepage; }
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
            get { return (ExtendedInfo != null && string.IsNullOrEmpty(selectedModGitHubUrl)) ? "N/A" : selectedModGitHubUrl; }
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

        public RelayCommand DeleteCommand { get; }

        public RelayCommand RefreshCommand { get; }

        public RelayCommand OpenLicenseLinkCommand { get; }

        public RelayCommand OpenHomepageCommand { get; }

        public RelayCommand OpenGitHubLinkCommand { get; }

        public RelayCommand OpenOnModPortalCommand { get; }

        public RelayCommand OpenAuthorCommand { get; }

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

        private bool ModIsBlacklisted(ModInfo mod)
        {
            return ModBlacklist.Any(keyword => mod.Title.StartsWith(keyword, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool ModFilter(object item)
        {
            ModInfo mod = item as ModInfo;
            if ((mod == null) || (string.IsNullOrWhiteSpace(mod.Title) || (mod.Title.Length < ModTitleMinLength) || ModIsBlacklisted(mod))) return false;

            if (string.IsNullOrWhiteSpace(filter)) return true;

            return StringHelper.FilterIsContained(filter, $"{mod.Title} {mod.Author}");
        }

        private OnlineModsViewModel()
        {
            InstalledMods = MainViewModel.Instance.Mods;
            InstalledModpacks = MainViewModel.Instance.Modpacks;

            asyncFetchExtendedInfoIndex = -1;

            DownloadCommand = new RelayCommand(async () => await DownloadSelectedModRelease(), () => SelectedRelease != null && !SelectedRelease.IsInstalled);
            DeleteCommand = new RelayCommand(DeleteSelectedModRelease, () => SelectedRelease != null && SelectedRelease.IsInstalled);
            RefreshCommand = new RelayCommand(async () => await RefreshModList());

            OpenLicenseLinkCommand = new RelayCommand(() =>
            {
                string url = ExtendedInfo?.License?.Url;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        Process.Start(url);
                    }
                    catch { }
                }
            });
            OpenHomepageCommand = new RelayCommand(() =>
            {
                string url = ExtendedInfo?.Homepage;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        Process.Start(url);
                    }
                    catch { }
                }
            });
            OpenGitHubLinkCommand = new RelayCommand(() =>
            {
                const string prefix = "https://www.github.com/";

                string url = ExtendedInfo?.GitHubUrl;
                
                if (!string.IsNullOrWhiteSpace(url))
                {
                    try
                    {
                        if (!url.StartsWith(prefix)) url = prefix + url;
                        Process.Start(url);
                    }
                    catch { }
                }
            });
            OpenOnModPortalCommand = new RelayCommand(() =>
            {
                if (SelectedMod != null)
                {
                    try
                    {
                        string name = SelectedMod.Name;
                        string url = $"https://mods.factorio.com/mod/{name}";
                        Process.Start(url);
                    }
                    catch { }
                }
            }, () => SelectedMod != null);
            OpenAuthorCommand = new RelayCommand(() =>
            {
                if (SelectedMod != null)
                {
                    try
                    {
                        string name = SelectedMod.Author;
                        string url = $"https://mods.factorio.com/user/{name}";
                        Process.Start(url);
                    }
                    catch { }
                }
            }, () => SelectedMod != null);

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
                        var downloadTask = ModWebsite.DownloadReleaseAsync(selectedRelease,
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
                
                UpdateSelectedReleases();
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
