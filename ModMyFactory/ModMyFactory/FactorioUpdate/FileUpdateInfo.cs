using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.FactorioUpdate
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class FileUpdateInfo
    {
        [JsonProperty("file")]
        public string Path { get; }

        [JsonProperty("action"), JsonConverter(typeof(StringEnumConverter), true)]
        public FileUpdateAction Action { get; }

        [JsonProperty("old_crc")]
        public uint OldCrc { get; }

        [JsonProperty("crc")]
        public uint NewCrc { get; }

        [JsonConstructor]
        public FileUpdateInfo(string path, FileUpdateAction action, uint oldCrc, uint newCrc)
        {
            Path = path;
            Action = action;
            OldCrc = oldCrc;
            NewCrc = newCrc;
        }
    }
}
