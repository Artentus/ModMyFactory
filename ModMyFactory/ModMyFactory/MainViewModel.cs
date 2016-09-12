using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
            taskbarInfo = (Application.Current.MainWindow.TaskbarItemInfo = new TaskbarItemInfo());
            AvailableCultures = new List<CultureEntry>()
            {
                new CultureEntry(new CultureInfo("en")),
                new CultureEntry(new CultureInfo("de")),
            };
            AvailableCultures[0].Select();

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

            //var fi = new FileInfo(@"C:\Users\Mathis\AppData\Roaming\Factorio\mods\mod-list.json");
            //using (var fs = fi.OpenRead())
            //{
            //    var serializer = new DataContractJsonSerializer(typeof(ModTemplateList));
            //    var templates = (ModTemplateList)serializer.ReadObject(fs);
            //    MessageBox.Show(string.Join("\n", templates.Mods.Select(template => template.Name + ";" + template.Enabled)));
            //}
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow() { Owner = Application.Current.MainWindow };

            settingsWindow.ShowDialog();
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
                    Owner = Application.Current.MainWindow,
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

            var versionListWindow = new VersionListWindow { Owner = Application.Current.MainWindow };
            var versionViewModel = new VersionListViewModell(versions);
            versionListWindow.DataContext = versionViewModel;

            bool? versionResult = versionListWindow.ShowDialog();
            if (versionResult == true)
            {
                FactorioOnlineVersion selectedVersion = versionViewModel.SelectedVersion;

                var cancellationSource = new CancellationTokenSource();
                var downloadWindow = new DownloadWindow { Owner = Application.Current.MainWindow };
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
            var aboutWindow = new AboutWindow() { Owner = Application.Current.MainWindow };
            aboutWindow.ShowDialog();
        }
    }
}
