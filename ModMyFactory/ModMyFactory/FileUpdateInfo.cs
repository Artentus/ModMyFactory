using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class FileUpdateInfo
    {
        [JsonProperty("file")]
        public string Path { get; }

        [JsonProperty("action"), JsonConverter(typeof(StringEnumConverter), true)]
        public FileUpdateAction Action { get; }

        [JsonProperty("old_crc")]
        public int OldCrc { get; }

        [JsonProperty("crc")]
        public int NewCrc { get; }

        [JsonConstructor]
        public FileUpdateInfo(string path, FileUpdateAction action, int oldCrc, int newCrc)
        {
            Path = path;
            Action = action;
            OldCrc = oldCrc;
            NewCrc = newCrc;
        }
    }
}
