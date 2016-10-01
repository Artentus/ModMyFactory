using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.MVVM;
using ModMyFactory.Web;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;

namespace ModMyFactory.ViewModels
{
    sealed class VersionManagementViewModel : ViewModelBase<VersionManagementWindow>
    {
        static VersionManagementViewModel instance;

        public static VersionManagementViewModel Instance = instance ?? (instance = new VersionManagementViewModel());

        bool loggedIn;
        string username;
        string password;
        CookieContainer container;

        FactorioVersion selectedVersion;

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
                }
            }
        }

        public RelayCommand DownloadCommand { get; }

        public RelayCommand AddFromZipCommand { get; }

        public RelayCommand AddFromFolderCommand { get; }

        public RelayCommand OpenFolderCommand { get; }

        public RelayCommand RemoveCommand { get; }

        private VersionManagementViewModel()
        {
            FactorioVersions = MainViewModel.Instance.FactorioVersions;
            FactorioVersionsView = (ListCollectionView)CollectionViewSource.GetDefaultView(FactorioVersions);
            FactorioVersionsView.CustomSort = new FactorioVersionSorter();

            
            DownloadCommand = new RelayCommand(async () => await DownloadOnlineVersion());
            AddFromZipCommand = new RelayCommand(async () => await AddZippedVersion());
            AddFromFolderCommand = new RelayCommand(async () => await AddLocalVersion());
            OpenFolderCommand = new RelayCommand(OpenFolder, () => SelectedVersion != null);
            RemoveCommand = new RelayCommand(RemoveSelectedVersion, () => SelectedVersion != null);
        }

        private bool LogIn()
        {
            bool failed = false;
            if (loggedIn)
            {
                loggedIn = FactorioWebsite.EnsureLoggedIn(container);
                failed = !loggedIn;
            }

            while (!loggedIn)
            {
                var loginWindow = new LoginWindow
                {
                    Owner = Window,
                    FailedText = { Visibility = failed ? Visibility.Visible : Visibility.Collapsed }
                };
                bool? loginResult = loginWindow.ShowDialog();
                if (loginResult == null || loginResult == false) return false;
                username = loginWindow.UsernameBox.Text;
                password = loginWindow.PasswordBox.Password;

                container = new CookieContainer();
                loggedIn = FactorioWebsite.LogIn(container, username, password);
                failed = !loggedIn;
            }

            return true;
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

        private bool ShowVersionList(out FactorioOnlineVersion selectedVersion)
        {
            selectedVersion = null;
            List<FactorioOnlineVersion> versions;
            if (!FactorioWebsite.GetVersions(container, out versions))
            {
                MessageBox.Show(Window, "Error retrieving available versions!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            versions.ForEach(item => item.Downloadable = !VersionAlreadyInstalled(item));

            var versionListWindow = new VersionListWindow { Owner = Window };
            versions.ForEach(item => versionListWindow.ViewModel.FactorioVersions.Add(item));

            bool? versionResult = versionListWindow.ShowDialog();
            selectedVersion = versionListWindow.ViewModel.SelectedVersion;
            return versionResult.HasValue && versionResult.Value;
        }

        private async Task DownloadOnlineVersion()
        {
            if (LogIn())
            {
                FactorioOnlineVersion selectedVersion;
                if (ShowVersionList(out selectedVersion))
                {
                    var cancellationSource = new CancellationTokenSource();
                    var progressWindow = new ProgressWindow { Owner = Window };
                    progressWindow.ViewModel.ActionName = "Downloading";
                    progressWindow.ViewModel.ProgressDescription = "Downloading " + selectedVersion.DownloadUrl;
                    progressWindow.ViewModel.CanCancel = true;
                    progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                    DirectoryInfo directory = App.Instance.Settings.GetFactorioDirectory();
                    Task<FactorioVersion> downloadTask = FactorioWebsite.DownloadFactorioPackageAsync(selectedVersion, directory, container, new Progress<double>(p =>
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
                    }), cancellationSource.Token);

                    Task closeWindowTask = downloadTask.ContinueWith(t => Task.Run(() => progressWindow.Dispatcher.Invoke(progressWindow.Close)));
                    progressWindow.ShowDialog();

                    FactorioVersion newVersion = await downloadTask;
                    if (newVersion != null) FactorioVersions.Add(newVersion);
                    await closeWindowTask;
                }
            }
        }

        private bool ArchiveFileValid(FileInfo archiveFile, out Version validVersion)
        {
            validVersion = default(Version);

            using (ZipArchive archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("data/base/info.json"))
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string content = reader.ReadToEnd();
                                MatchCollection matches = Regex.Matches(content, @"[0-9]+\.[0-9]+\.[0-9]+",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count == 0) return false;

                                string versionString = matches[0].Value;
                                validVersion = Version.Parse(versionString);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private async Task AddZippedVersion()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = "ZIP-Archives (*.zip)|*.zip";
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var archiveFile = new FileInfo(dialog.FileName);
                Version version = null;
                DirectoryInfo versionDirectory = null;

                var progressWindow = new ProgressWindow() { Owner = Window };
                progressWindow.ViewModel.ActionName = "Adding from ZIP";
                progressWindow.ViewModel.ProgressDescription = "Checking validity...";
                progressWindow.ViewModel.IsIndeterminate = true;

                bool invalidArchiveFile = false;
                IProgress<int> progress = new Progress<int>(stage =>
                {
                    switch (stage)
                    {
                        case 1:
                            progressWindow.ViewModel.ProgressDescription = "Extracting...";
                            break;
                        case -1:
                            invalidArchiveFile = true;
                            break;
                    }
                });

                Task extractTask = Task.Run(() =>
                {
                    if (ArchiveFileValid(archiveFile, out version))
                    {
                        progress.Report(1);

                        DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                        ZipFile.ExtractToDirectory(archiveFile.FullName, factorioDirectory.FullName);

                        versionDirectory = new DirectoryInfo(Path.Combine(factorioDirectory.FullName, "Factorio_" + version.ToString(3)));
                        versionDirectory.MoveTo(Path.Combine(factorioDirectory.FullName, version.ToString(3)));
                    }
                    else
                    {
                        progress.Report(-1);
                    }
                });

                Task closeWindowTask =
                    extractTask.ContinueWith(t => Task.Run(() => progressWindow.Dispatcher.Invoke(progressWindow.Close)));
                progressWindow.ShowDialog();

                await extractTask;
                await closeWindowTask;

                if (invalidArchiveFile)
                {
                    MessageBox.Show(Window,
                        "This ZIP archive does not contain a valid Factorio installation.",
                        "Invalid archive", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    FactorioVersions.Add(new FactorioVersion(versionDirectory, version));

                    if (MessageBox.Show(Window, "Do you want to delete the source file?", "Delete file?",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        archiveFile.Delete();
                    }
                }
            }
        }

        private bool LocalInstallationValid(DirectoryInfo directory, out Version validVersion)
        {
            validVersion = default(Version);

            FileInfo infoFile = new FileInfo(Path.Combine(directory.FullName, @"data\base\info.json"));
            if (infoFile.Exists)
            {
                using (var reader = infoFile.OpenText())
                {
                    string content = reader.ReadToEnd();
                    MatchCollection matches = Regex.Matches(content, @"[0-9]+\.[0-9]+\.[0-9]+",
                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    if (matches.Count == 0) return false;

                    string versionString = matches[0].Value;
                    validVersion = Version.Parse(versionString);
                    return true;
                }
                    
            }

            return false;
        }

        private async Task AddLocalVersion()
        {
            var dialog = new VistaFolderBrowserDialog();
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var installationDirectory = new DirectoryInfo(dialog.SelectedPath);
                Version version;

                if (!LocalInstallationValid(installationDirectory, out version))
                {
                    MessageBox.Show(Window, "This folder does not contain a valid Factorio installation.",
                        "Invalid folder", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (MessageBox.Show(Window,
                        "The selected installation of Factorio will be moved to the location specified in the settings!\nDo you wish to continue?",
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
                    DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(factorioDirectory.FullName, version.ToString(3)));

                    var progressWindow = new ProgressWindow() { Owner = Window };
                    progressWindow.ViewModel.ActionName = "Adding local installation";
                    progressWindow.ViewModel.ProgressDescription = "Moving files...";
                    progressWindow.ViewModel.IsIndeterminate = true;

                    Task moveTask = installationDirectory.MoveToAsync(destinationDirectory.FullName)
                        .ContinueWith(t => Task.Run(() => progressWindow.Dispatcher.Invoke(progressWindow.Close)));
                    progressWindow.ShowDialog();
                    await moveTask;

                    FactorioVersions.Add(new FactorioVersion(destinationDirectory, version));
                }
            }
        }

        private void OpenFolder()
        {
            Process.Start(SelectedVersion.Directory.FullName);
        }

        private void RemoveSelectedVersion()
        {
            if (MessageBox.Show(Window,
                    "Do you really want to remove this version of Factorio?\nThis will delete all corresponding files on your hard drive.",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SelectedVersion.DeleteLinks();
                SelectedVersion.Directory.Delete(true);
                FactorioVersions.Remove(SelectedVersion);
            }
        }
    }
}
