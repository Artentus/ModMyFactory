using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Lang;
using ModMyFactory.MVVM;
using ModMyFactory.Web;

namespace ModMyFactory
{
    sealed class MainViewModel : ViewModelBase<MainWindow>
    {
        static MainViewModel instance;

        public static MainViewModel Instance => instance ?? (instance = new MainViewModel());

        bool loggedIn;
        string username;
        string password;
        CookieContainer container;
        bool canCancelDownload;

        public List<CultureEntry> AvailableCultures { get; } 

        public ObservableCollection<Mod> Mods { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        public RelayCommand OpenSettingsCommand { get; }

        public RelayCommand OpenVersionListCommand { get; }

        public RelayCommand OpenAboutWindowCommand { get; }

        private MainViewModel()
        {
            loggedIn = false;

            AvailableCultures = App.Instance.GetAvailableCultures();
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

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow() { Owner = Window };
            settingsWindow.ViewModel.Reset();

            bool? result = settingsWindow.ShowDialog();
            if (result != null && result.Value)
            {
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
                    Owner = Window,
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
                MessageBox.Show(Application.Current.MainWindow, "Error retrieving available versions!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var versionListWindow = new VersionListWindow { Owner = Window };
            versions.ForEach(item => versionListWindow.ViewModel.FactorioVersions.Add(item));

            bool? versionResult = versionListWindow.ShowDialog();
            if (versionResult == true)
            {
                FactorioOnlineVersion selectedVersion = versionListWindow.ViewModel.SelectedVersion;

                var cancellationSource = new CancellationTokenSource();
                var progressWindow = new ProgressWindow { Owner = Window };
                progressWindow.ViewModel.ProgressDescription = "Downloading " + selectedVersion.DownloadUrl;
                progressWindow.ViewModel.CanCancel = true;
                progressWindow.ViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                string directoryPath = Path.Combine(Environment.CurrentDirectory, "Factorio");
                var directory = new DirectoryInfo(directoryPath);

                Task t = FactorioWebsite.DownloadFactorioPackageAsync(selectedVersion, directory, container, new Progress<double>(p =>
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
                }), cancellationSource.Token)
                    .ContinueWith((t1) => Task.Run(() => progressWindow.Dispatcher.Invoke(progressWindow.Close)));
                progressWindow.ShowDialog();
                await t;
            }
        }

        private void OpenAboutWindow()
        {
            var aboutWindow = new AboutWindow() { Owner = Window };
            aboutWindow.ShowDialog();
        }
    }
}
