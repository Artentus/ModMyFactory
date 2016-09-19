using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using ModMyFactory.Lang;

namespace ModMyFactory
{
    public partial class App : Application
    {
        internal static App Instance => (App)Application.Current;

        ResourceDictionary enDictionary;

        internal Settings Settings { get; }

        internal string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModMyFactory");

        private App()
        {
            var appDataDirectory = new DirectoryInfo(AppDataPath);
            if (!appDataDirectory.Exists) appDataDirectory.Create();

            string settingsFile = Path.Combine(appDataDirectory.FullName, "settings.json");
            Settings = Settings.Load(settingsFile, true);
        }

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
