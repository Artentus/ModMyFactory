using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace ModMyFactory
{
    public partial class App : Application
    {
        public static App Instance => (App)Application.Current;

        ResourceDictionary enDictionary;

        internal Settings Settings { get; }

        private App()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModMyFactory");
            var appDataDirectory = new DirectoryInfo(appDataPath);
            if (!appDataDirectory.Exists) appDataDirectory.Create();

            string settingsFile = Path.Combine(appDataDirectory.FullName, "settings.json");
            Settings = Settings.Load(settingsFile, true);
        }

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
    }
}
