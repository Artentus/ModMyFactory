﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModMyFactory.Helpers;
using ModMyFactory.Lang;
using Octokit;
using Application = System.Windows.Application;
using ModMyFactory.Models;
using FileMode = System.IO.FileMode;
using System.Net;
using System.Linq;
using System.Text;

namespace ModMyFactory
{
    public partial class App : Application
    {
        private const int PreReleaseVersion = 3;

        /// <summary>
        /// The current application instance.
        /// </summary>
        internal static App Instance => (App)Application.Current;

        /// <summary>
        /// Indicates whether the application is currently in design mode.
        /// </summary>
        internal static bool IsInDesignMode => !(Application.Current is App);

        /// <summary>
        /// The applications version.
        /// </summary>
        internal static ExtendedVersion Version
        {
            get
            {
                Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return PreReleaseVersion >= 0
                    ? new ExtendedVersion(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build, PreReleaseVersion)
                    : new ExtendedVersion(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);
            }
        }

        UpdateSearchResult searchResult;
        bool includePreReleases;

        /// <summary>
        /// The applications settings.
        /// </summary>
        internal Settings Settings { get; }

        /// <summary>
        /// The applications AppData path.
        /// </summary>
        internal string AppDataPath { get; }

        /// <summary>
        /// The applications temp path.
        /// </summary>
        internal string TempPath => Path.Combine(Path.GetTempPath(), "ModMyFactory");

        /// <summary>
        /// The application directory.
        /// </summary>
        internal string ApplicationDirectoryPath { get; }

        public App(bool createCrashLog, bool registerFileTypes, string appDataPath)
        {
            AppDataPath = appDataPath;
            ApplicationDirectoryPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            var appDataDirectory = new DirectoryInfo(AppDataPath);
            if (!appDataDirectory.Exists) appDataDirectory.Create();

            CleanUpTempDir();

            string settingsFile = Path.Combine(appDataDirectory.FullName, "settings.json");
            Settings = Settings.Load(settingsFile, true);

            // Create file type association
            if (registerFileTypes)
            {
                string iconPath = Path.Combine(ApplicationDirectoryPath, "Factorio_Modpack_Icon.ico");
                string handlerName = RegistryHelper.RegisterHandler("FactorioModpack", 1, "Factorio modpack", $"\"{iconPath}\"");
                RegistryHelper.RegisterFileType(".fmp", handlerName, "application/json", PercievedFileType.Text);
                RegistryHelper.RegisterFileType(".fmpa", handlerName, "application/x-zip-compressed", PercievedFileType.Text);
            }

            // Reset log
            ResetExceptionLog();

            // Generate log when crashed.
            if (createCrashLog)
            {
                this.DispatcherUnhandledException += (sender, e) =>
                {
                    WriteExceptionLog(e.Exception);

                    MessageBox.Show("A crash log has been created in %AppData%\\ModMyFactory.",
                        "ModMyFactory crashed!", MessageBoxButton.OK, MessageBoxImage.Error);

                    e.Handled = true;
                    this.Shutdown(e.Exception.HResult);
                };
            }

            // Http request speed optimization
            ServicePointManager.DefaultConnectionLimit = 20;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public App(bool createCrashLog, bool registerFileTypes)
            : this(createCrashLog, registerFileTypes, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModMyFactory"))
        { }

        public App()
            : this(false, false)
        { }

        /// <summary>
        /// Cleans up the apps temporary directory.
        /// </summary>
        internal void CleanUpTempDir()
        {
            var tempDir = new DirectoryInfo(TempPath);
            if (tempDir.Exists) tempDir.Delete(true);
        }

        /// <summary>
        /// Deletes the default log file if it exists.
        /// </summary>
        private void ResetExceptionLog()
        {
            var logFile = new FileInfo(Path.Combine(AppDataPath, "error-log.txt"));
            if (logFile.Exists) logFile.Delete();
        }

        /// <summary>
        /// Writes an error message to the default log file.
        /// </summary>
        internal void WriteExceptionLog(Exception exception)
        {
            var logFile = new FileInfo(Path.Combine(AppDataPath, "error-log.txt"));
            if (logFile.Exists)
            {
                using (Stream stream = logFile.Open(FileMode.Append, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine();
                        writer.WriteLine();
                        writer.Write(exception.ToString());
                    }
                }
            }
            else
            {
                using (Stream stream = logFile.Open(FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(exception.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Lists all available cultures.
        /// </summary>
        /// <returns>Returns a list containing all available cultures.</returns>
        internal List<CultureEntry> GetAvailableCultures()
        {
            var availableCultures = new List<CultureEntry>()
            {
                new CultureEntry(new CultureInfo("en"))
            };

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
            var mergedDictionaries = Resources.MergedDictionaries;
            if (mergedDictionaries.Count == 3) mergedDictionaries.RemoveAt(2);
            
            string resourceName = "Strings." + culture.TwoLetterISOLanguageName;
            if (culture.TwoLetterISOLanguageName != "en" && Resources.Contains(resourceName))
                mergedDictionaries.Add((ResourceDictionary)Resources[resourceName]);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        
        /// <summary>
        /// Applies the specified theme to the UI.
        /// </summary>
        /// <param name="name">The themes name.</param>
        internal void SetTheme(string name)
        {
            var dict = new ResourceDictionary() { Source = new Uri($"Colors_{name}.xaml", UriKind.Relative) };
            Resources.MergedDictionaries[0] = dict;

            if (name != Settings.Theme)
            {
                Settings.Theme = name;
                Settings.Save();
            }
        }

        /// <summary>
        /// Gets a localized string from the applications resources.
        /// </summary>
        /// <param name="key">The key of the string to get.</param>
        /// <returns>Returns the requested string resource or null if the specified resource key does not exist.</returns>
        internal string GetLocalizedResourceString(string key)
        {
            if (!Resources.Contains(key)) return null;

            return Resources[key] as string;
        }

        /// <summary>
        /// Gets a localized message from the applications resources.
        /// </summary>
        /// <param name="key">The key of the message to get.</param>
        /// <param name="messageType">The type of the message to get.</param>
        /// <returns>Returns the requested message or null if the specified resource key does not exist.</returns>
        internal string GetLocalizedMessage(string key, MessageType messageType)
        {
            return GetLocalizedResourceString(string.Join(".", messageType.ToString("g"), key, "Message"));
        }

        /// <summary>
        /// Gets a localized message title from the applications resources.
        /// </summary>
        /// <param name="key">The key of the message title to get.</param>
        /// <param name="messageType">The type of the message title to get.</param>
        /// <returns>Returns the requested message title or null if the specified resource key does not exist.</returns>
        internal string GetLocalizedMessageTitle(string key, MessageType messageType)
        {
            return GetLocalizedResourceString(string.Join(".", messageType.ToString("g"), key, "Title"));
        }

        private string GetAssetName(ExtendedVersion version)
        {
            var sb = new StringBuilder();
            sb.Append("ModMyFactory_");
            sb.Append(version);

            #if PORTABLE
            sb.Append("_portable");
            #endif

            sb.Append(".zip");

            return sb.ToString();
        }

        /// <summary>
        /// Searches for available updates on GitHub.
        /// </summary>
        /// <returns>Returns an update-search result.</returns>
        internal async Task<UpdateSearchResult> SearchForUpdateAsync(bool includePreReleases)
        {
            if (searchResult == null || !searchResult.UpdateAvailable || (includePreReleases != this.includePreReleases))
            {
                this.includePreReleases = includePreReleases;

                var client = new GitHubClient(new ProductHeaderValue("ModMyFactory"));
                var latestRelease = await client.GetLatestReleaseAsync("Artentus", "ModMyFactory", includePreReleases);

                var version = new ExtendedVersion(latestRelease.TagName);
                bool updateAvailable = version > App.Version;
                string updateUrl = latestRelease.HtmlUrl;

                string assetName = GetAssetName(version);
                string assetUrl = latestRelease.Assets.FirstOrDefault(asset => asset.Name == assetName)?.BrowserDownloadUrl;

                searchResult = new UpdateSearchResult(updateAvailable, updateUrl, assetUrl, version);
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
