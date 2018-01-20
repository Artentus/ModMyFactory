using System.IO;
using Newtonsoft.Json;

namespace ModMyFactory.Helpers
{
    static class JsonHelper
    {
        private static readonly JsonSerializerSettings DefaultSettings;

        static JsonHelper()
        {
            DefaultSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

        public static void Serialize<T>(T value, FileInfo file)
        {
            using (var writer = file.CreateText())
            {
                string json = JsonConvert.SerializeObject(value, Formatting.Indented, DefaultSettings);
                writer.Write(json);
            }
        }

        public static T Deserialize<T>(FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
            }
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
        }
    }
}
