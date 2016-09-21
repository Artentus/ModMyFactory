using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory
{
    [JsonObject(MemberSerialization.OptOut)]
    class Settings
    {
        public static Settings CreateDefault(string fileName)
        {
            var defaultSettings = new Settings(fileName)
            {
                FactorioDirectoryOption = DirectoryOption.AppData,
                ModDirectoryOption = DirectoryOption.AppData,
                FactorioDirectory = string.Empty,
                ModDirectory = string.Empty,
                SelectedLanguage = "en",
                State = WindowState.Normal,
                PosX = 0, PosY = 0, Width = -1, Height = -1,
                ModGridLength = new GridLength(1, GridUnitType.Star),
                ModpackGridLength = new GridLength(1, GridUnitType.Star),
                SelectedVersion = default(Version),
            };
            return defaultSettings;
        }

        public static Settings Load(string fileName, bool createDefault = false)
        {
            var file = new FileInfo(fileName);
            if (!file.Exists && createDefault)
            {
                Settings defaultSettings = CreateDefault(fileName);
                defaultSettings.Save();
                return defaultSettings;
            }

            Settings settings = JsonHelper.Deserialize<Settings>(file);
            settings.file = file;
            return settings;
        }

        FileInfo file;

        public DirectoryOption FactorioDirectoryOption;

        public DirectoryOption ModDirectoryOption;

        public string FactorioDirectory;

        public string ModDirectory;

        public string SelectedLanguage;

        public WindowState State;

        public int PosX, PosY, Width, Height;

        public GridLength ModGridLength, ModpackGridLength;

        [JsonConverter(typeof(VersionConverter))]
        public Version SelectedVersion;

        [JsonConstructor]
        private Settings()
        { }

        private Settings(string fileName)
        {
            file = new FileInfo(fileName);
        }

        public void Save()
        {
            JsonHelper.Serialize(this, file);
        }

        public DirectoryInfo GetFactorioDirectory()
        {
            const string directoryName = "Factorio";

            switch (FactorioDirectoryOption)
            {
                case DirectoryOption.AppData:
                    return new DirectoryInfo(Path.Combine(App.Instance.AppDataPath, directoryName));
                case DirectoryOption.ApplicationDirectory:
                    return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, directoryName));
                case DirectoryOption.Custom:
                    return new DirectoryInfo(FactorioDirectory);
            }

            throw new InvalidOperationException();
        }

        public DirectoryInfo GetModDirectory()
        {
            const string directoryName = "mods";

            switch (ModDirectoryOption)
            {
                case DirectoryOption.AppData:
                    return new DirectoryInfo(Path.Combine(App.Instance.AppDataPath, directoryName));
                case DirectoryOption.ApplicationDirectory:
                    return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, directoryName));
                case DirectoryOption.Custom:
                    return new DirectoryInfo(ModDirectory);
            }

            throw new InvalidOperationException();
        }
    }
}
