using System.IO;
using Newtonsoft.Json;

namespace ModMyFactory.Helpers
{
    static class JsonHelper
    {
        public static void Serialize<T>(T value, FileInfo file)
        {
            using (var writer = file.CreateText())
            {
                string json = JsonConvert.SerializeObject(value, Formatting.Indented);
                writer.Write(json);
            }
        }

        public static T Deserialize<T>(FileInfo file)
        {
            using (var reader = file.OpenText())
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}
