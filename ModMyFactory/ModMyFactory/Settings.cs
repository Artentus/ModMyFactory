using System.IO;
using Newtonsoft.Json;

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
    }
}
