using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModMyFactory.Lang;
using Octokit;
using Application = System.Windows.Application;
using ModMyFactory.Models;
using FileMode = System.IO.FileMode;

namespace ModMyFactory
{
    public partial class App : Application
    {
        /// <summary>
        /// The current application instance.
        /// </summary>
        internal static App Instance => (App)Application.Current;

        /// <summary>
        /// Indicates whether the application is currently in design mode.
        /// </summary>
        internal static bool IsInDesignMode => !(Application.Current is App);

        ResourceDictionary enDictionary;
        UpdateSearchResult searchResult;

        /// <summary>
        /// The applications version.
        /// </summary>
        internal Version AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// The applications settings.
        /// </summary>
        internal Settings Settings { get; }

        /// <summary>
        /// The applications AppData path.
        /// </summary>
        internal string AppDataPath { get; }

        /// <summary>
        /// The global location for savegames.
        /// </summary>
        internal string GlobalSavePath => Path.Combine(AppDataPath, "saves");

        /// <summary>
        /// The global location for scenarios.
        /// </summary>
        internal string GlobalScenarioPath => Path.Combine(AppDataPath, "scenarios");

        public App(bool createCrashLog, string appDataPath)
        {
            AppDataPath = appDataPath;

            var appDataDirectory = new DirectoryInfo(AppDataPath);
            if (!appDataDirectory.Exists) appDataDirectory.Create();

            string settingsFile = Path.Combine(appDataDirectory.FullName, "settings.json");
            Settings = Settings.Load(settingsFile, true);

            // Generate log when crashed.
            if (createCrashLog)
            {
                this.DispatcherUnhandledException += (sender, e) =>
                {
                    var logFile = new FileInfo(Path.Combine(AppDataPath, "crash-log.txt"));
                    using (Stream stream = logFile.Open(FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(e.Exception.ToString());
                        }
                    }

                    MessageBox.Show(this.MainWindow, "A crash log has been created in %AppData%\\ModMyFactory.",
                        "ModMyFactory crashed!", MessageBoxButton.OK, MessageBoxImage.Error);

                    e.Handled = true;
                    this.Shutdown(e.Exception.HResult);
                };
            }
        }

        public App(bool createCrashLog = true)
            : this(createCrashLog, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModMyFactory"))
        { }

        /// <summary>
        /// Lists all available cultures.
        /// </summary>
        /// <returns>Returns a list containing all available cultures.</returns>
        internal List<CultureEntry> GetAvailableCultures()
        {
            var availableCultures = new List<CultureEntry>();
            foreach (var key in Resources.Keys)
            {
                string stringKey = key as string;
                if (stringKey != null && stringKey.StartsWith("Strings."))
                {
                    var dictionary = Resources[key] as ResourceDictionary;
                    if (dictionary != null)
                    {
                        string cultureName = stringKey.Split('.')[1];
                        availableCultures.Add(new CultureEntry(new CultureInfo(cultureName)));
                    }
                }
            }
            return availableCultures;
        }

        /// <summary>
        /// Sets a specified culture as UI culture.
        /// </summary>
        /// <param name="culture">The culture to set as UI culture.</param>
        internal void SelectCulture(CultureInfo culture)
        {
            if (enDictionary == null)
                enDictionary = (ResourceDictionary)Resources["Strings.en"];

            var mergedDictionaries = Resources.MergedDictionaries;
            mergedDictionaries.Clear();

            mergedDictionaries.Add(enDictionary);
            string resourceName = "Strings." + culture.TwoLetterISOLanguageName;
            if (culture.TwoLetterISOLanguageName != "en" && Resources.Contains(resourceName))
                mergedDictionaries.Add((ResourceDictionary)Resources[resourceName]);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        /// <summary>
        /// Searches for available updates on GitHub.
        /// </summary>
        /// <returns>Returns an update-search result.</returns>
        internal async Task<UpdateSearchResult> SearchForUpdateAsync()
        {
            if (searchResult == null || !searchResult.UpdateAvailable)
            {
                var client = new GitHubClient(new ProductHeaderValue("ModMyFactory"));
                var latestRelease = await client.Repository.Release.GetLatest("Artentus", "ModMyFactory");

                var version = Version.Parse(latestRelease.TagName.Substring(2));
                bool updateAvailable = version > AssemblyVersion;
                string updateUrl = latestRelease.HtmlUrl;
                searchResult = new UpdateSearchResult(updateAvailable, updateUrl, version);
            }

            return searchResult;
        }

        private void ExpanderPreviewMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var element = (UIElement)sender;
            var newArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            newArgs.RoutedEvent = UIElement.MouseWheelEvent;
            element.RaiseEvent(newArgs);
        }
    }
}
