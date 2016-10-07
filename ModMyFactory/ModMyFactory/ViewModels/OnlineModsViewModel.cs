using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.MVVM;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.ViewModels
{
    sealed class OnlineModsViewModel : ViewModelBase<OnlineModsWindow>
    {
        string token;

        bool LoggedInWithToken => GlobalCredentials.LoggedIn && !string.IsNullOrEmpty(token);

        ListCollectionView modsView;
        List<ModInfo> mods;
        string filter;
        ModRelease selectedRelease;

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
                }
            }
        }

        public ModCollection InstalledMods { get; set; }

        public string Filter
        {
            get { return filter; }
            set
            {
                if (value != filter)
                {
                    filter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Filter)));

                    modsView.Refresh();
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

                    new Action(async () => await LoadExtendedModInfoAsync(selectedMod)).Invoke();
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

        private async Task LoadExtendedModInfoAsync(ModInfo mod)
        {
            ExtendedModInfo extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod);
            ExtendedInfo = extendedInfo;
        }

        private bool ModFilter(object item)
        {
            ModInfo mod = item as ModInfo;
            if (mod == null) return false;

            if (string.IsNullOrWhiteSpace(filter)) return true;
            return Thread.CurrentThread.CurrentUICulture.CompareInfo.IndexOf(mod.Title, filter, CompareOptions.IgnoreCase) >= 0;
        }

        public OnlineModsViewModel()
        {
            SelectedReleases = new ObservableCollection<ModRelease>();

            DownloadCommand = new RelayCommand(async () => await DownloadSelectedModRelease(), () => SelectedRelease != null && !SelectedRelease.IsInstalled);
            UpdateCommand = new RelayCommand(async () => await UpdateSelectedModRelease(), () =>
                    SelectedRelease != null && SelectedRelease.IsInstalled &&
                    SelectedRelease != GetNewestRelease(ExtendedInfo, SelectedRelease));
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

        private async Task DownloadSelectedModRelease()
        {
            if (LogIn())
            {
                var cancellationSource = new CancellationTokenSource();
                var progressWindow = new ProgressWindow { Owner = Window };
                progressWindow.ViewModel.ActionName = "Downloading";
                progressWindow.ViewModel.ProgressDescription = "Downloading " + selectedRelease.FileName;
                progressWindow.ViewModel.CanCancel = true;
                progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                Task<Mod> downloadTask = ModWebsite.DownloadReleaseAsync(selectedRelease, GlobalCredentials.Username, token, new Progress<double>(p =>
                {
                    progressWindow.ViewModel.Progress = p;
                }), cancellationSource.Token, InstalledMods, MainViewModel.Instance.Modpacks, MainViewModel.Instance.Window);

                Task closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                Mod newMod = await downloadTask;
                if (newMod != null) InstalledMods.Add(newMod);
                await closeWindowTask;

                foreach (var release in SelectedReleases)
                {
                    release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                    release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.FactorioVersion);
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
            if (LogIn())
            {
                ModRelease newestRelease = GetNewestRelease(ExtendedInfo, SelectedRelease);
                Mod mod = InstalledMods.FindByFactorioVersion(SelectedMod.Name, newestRelease.FactorioVersion);
                var zippedMod = mod as ZippedMod;
                var extractedMod = mod as ExtractedMod;

                var cancellationSource = new CancellationTokenSource();
                var progressWindow = new ProgressWindow { Owner = Window };
                progressWindow.ViewModel.ActionName = "Updating";
                progressWindow.ViewModel.ProgressDescription = "Downloading " + newestRelease.FileName;
                progressWindow.ViewModel.CanCancel = true;
                progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                IProgress<double> progress = new Progress<double>(p =>
                {
                    if (p > 1)
                    {
                        progressWindow.ViewModel.ProgressDescription = "Extracting...";
                        progressWindow.ViewModel.IsIndeterminate = true;
                        progressWindow.ViewModel.CanCancel = false;
                    }
                    else
                    {
                        progressWindow.ViewModel.Progress = p;
                    }
                });

                Task downloadTask = ModWebsite.UpdateReleaseAsync(newestRelease, GlobalCredentials.Username, token, progress, cancellationSource.Token);

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
                    });
                }

                Task closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                if (zippedMod != null)
                {
                    FileInfo newModFile = await (Task<FileInfo>)downloadTask;
                    if (zippedMod.FactorioVersion == newestRelease.FactorioVersion)
                    {
                        zippedMod.Update(newModFile);
                    }
                    else
                    {
                        var newMod = new ZippedMod(zippedMod.Name, newestRelease.FactorioVersion, newModFile,
                            InstalledMods, MainViewModel.Instance.Modpacks, MainViewModel.Instance.Window);
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
                        extractedMod.Update(newModDirectory);
                    }
                    else
                    {
                        var newMod = new ExtractedMod(extractedMod.Name, newestRelease.FactorioVersion, newModDirectory,
                            InstalledMods, MainViewModel.Instance.Modpacks, MainViewModel.Instance.Window);
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
                    }
                }

                await closeWindowTask;

                SelectedRelease = null;
                foreach (var release in SelectedReleases)
                {
                    release.IsInstalled = InstalledMods.Contains(selectedMod.Name, release.Version);
                    release.IsVersionInstalled = !release.IsInstalled && InstalledMods.ContainsByFactorioVersion(selectedMod.Name, release.FactorioVersion);
                }
            }
        }
    }
}
