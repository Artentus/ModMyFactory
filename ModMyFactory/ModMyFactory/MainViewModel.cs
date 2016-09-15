using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using ModMyFactory.Lang;
using ModMyFactory.Web;

namespace ModMyFactory
{
    sealed class MainViewModel : NotifyPropertyChangedBase
    {
        static MainViewModel instance;
        bool loggedIn;
        string username;
        string password;
        CookieContainer container;
        readonly TaskbarItemInfo taskbarInfo;
        bool canCancelDownload;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        public List<CultureEntry> AvailableCultures { get; } 

        public ObservableCollection<Mod> Mods { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        public RelayCommand OpenSettingsCommand { get; }

        public RelayCommand OpenVersionListCommand { get; }

        public RelayCommand OpenAboutWindowCommand { get; }

        private MainViewModel()
        {
            loggedIn = false;
            taskbarInfo = (App.Instance.MainWindow.TaskbarItemInfo = new TaskbarItemInfo());

            AvailableCultures = GetAvailableCultures();
            AvailableCultures.First(entry => string.Equals(entry.LanguageCode, App.Instance.Settings.SelectedLanguage, StringComparison.InvariantCultureIgnoreCase)).Select();

            Mods = new ObservableCollection<Mod>();
            Modpacks = new ObservableCollection<Modpack>();

            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenVersionListCommand = new RelayCommand(async () => await OpenVersionList());
            OpenAboutWindowCommand = new RelayCommand(OpenAboutWindow);

            Mod mod1 = new Mod("aaa", new FileInfo("a"));
            Mod mod2 = new Mod("bbb", new FileInfo("b"));
            Mod mod3 = new Mod("ccc", new FileInfo("c"));
            Mod mod4 = new Mod("ddd", new FileInfo("d"));
            Mod mod5 = new Mod("eee", new FileInfo("e"));
            Mods.Add(mod1);
            Mods.Add(mod2);
            Mods.Add(mod3);
            Mods.Add(mod4);
            Mods.Add(mod5);
            Modpack modpack1 = new Modpack("aaa");
            Modpack modpack2 = new Modpack("bbb");
            Modpack modpack3 = new Modpack("ccc");
            //modpack.Mods.Add(mod1);
            //modpack.Mods.Add(mod2);
            Modpacks.Add(modpack1);
            Modpacks.Add(modpack2);
            Modpacks.Add(modpack3);
        }

        private List<CultureEntry> GetAvailableCultures()
        {
            var availableCultures = new List<CultureEntry>();
            foreach (var key in App.Instance.Resources.Keys)
            {
                string stringKey = key as string;
                if (stringKey != null && stringKey.StartsWith("Strings."))
                {
                    var dictionary = App.Instance.Resources[key] as ResourceDictionary;
                    if (dictionary != null)
                    {
                        string cultureName = stringKey.Split('.')[1];
                        availableCultures.Add(new CultureEntry(new CultureInfo(cultureName)));
                    }
                }
            }
            availableCultures.Sort((entry1, entry2) => string.Compare(entry1.EnglishName, entry2.EnglishName, StringComparison.InvariantCultureIgnoreCase));
            return availableCultures;
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow() { Owner = App.Instance.MainWindow };

            bool? result = settingsWindow.ShowDialog();
            if (result != null && result.Value)
            {
                var viewModel = (SettingsViewModel)settingsWindow.DataContext;

                if (viewModel.FactorioDirectoryIsAppData)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.AppData;
                    App.Instance.Settings.FactorioDirectory = string.Empty;
                }
                else if (viewModel.FactorioDirectoryIsAppDirectory)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.ApplicationDirectory;
                    App.Instance.Settings.FactorioDirectory = string.Empty;
                }
                else if (viewModel.FactorioDirectoryIsCustom)
                {
                    App.Instance.Settings.FactorioDirectoryOption = DirectoryOption.Custom;
                    App.Instance.Settings.FactorioDirectory = viewModel.FactorioDirectory;
                }

                if (viewModel.ModDirectoryIsAppData)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.AppData;
                    App.Instance.Settings.ModDirectory = string.Empty;
                }
                else if (viewModel.ModDirectoryIsAppDirectory)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.ApplicationDirectory;
                    App.Instance.Settings.ModDirectory = string.Empty;
                }
                else if (viewModel.ModDirectoryIsCustom)
                {
                    App.Instance.Settings.ModDirectoryOption = DirectoryOption.Custom;
                    App.Instance.Settings.ModDirectory = viewModel.ModDirectory;
                }

                App.Instance.Settings.Save();
            }
        }

        private async Task OpenVersionList()
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
                    Owner = App.Instance.MainWindow,
                    FailedText = { Visibility = failed ? Visibility.Visible : Visibility.Collapsed }
                };
                bool? loginResult = loginWindow.ShowDialog();
                if (loginResult == null || loginResult == false) return;
                username = loginWindow.UsernameBox.Text;
                password = loginWindow.PasswordBox.Password;

                container = new CookieContainer();
                loggedIn = FactorioWebsite.LogIn(container, username, password);
                failed = !loggedIn;
            }

            List<FactorioOnlineVersion> versions;
            if (!FactorioWebsite.GetVersions(container, out versions))
            {
                MessageBox.Show(App.Instance.MainWindow, "Error retrieving available versions!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var versionListWindow = new VersionListWindow { Owner = App.Instance.MainWindow };
            var versionViewModel = new VersionListViewModell(versions);
            versionListWindow.DataContext = versionViewModel;

            bool? versionResult = versionListWindow.ShowDialog();
            if (versionResult == true)
            {
                FactorioOnlineVersion selectedVersion = versionViewModel.SelectedVersion;

                var cancellationSource = new CancellationTokenSource();
                var downloadWindow = new DownloadWindow { Owner = App.Instance.MainWindow };
                canCancelDownload = true;
                var cancelCommand = new RelayCommand(() => cancellationSource.Cancel(), () => canCancelDownload);
                var downloadViewModel = new DownloadViewModel(cancelCommand)
                {
                    DownloadState = "Downloading " + selectedVersion.DownloadUrl
                };
                downloadWindow.DataContext = downloadViewModel;

                string directoryPath = Path.Combine(Environment.CurrentDirectory, "Factorio");
                var directory = new DirectoryInfo(directoryPath);

                taskbarInfo.ProgressState = TaskbarItemProgressState.Normal;
                taskbarInfo.ProgressValue = 0;
                Task t = FactorioWebsite.DownloadFactorioPackageAsync(selectedVersion, directory, container, new Progress<double>(p =>
                {
                    if (p > 1)
                    {
                        downloadViewModel.DownloadState = "Extracting...";
                        downloadViewModel.IsIndeterminate = true;
                        taskbarInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                        canCancelDownload = false;
                    }
                    else
                    {
                        downloadViewModel.Progress = p;
                        taskbarInfo.ProgressValue = p;
                    }
                }), cancellationSource.Token)
                    .ContinueWith((t1) => Task.Run(() => downloadWindow.Dispatcher.Invoke(downloadWindow.Close)));
                downloadWindow.ShowDialog();
                await t;
                taskbarInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = App.Instance.MainWindow };
            aboutWindow.ShowDialog();
        }
    }
}
