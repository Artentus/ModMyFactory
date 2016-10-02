using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct ModRelease
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version;

        [JsonProperty("game_version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version FactorioVersion;

        [JsonProperty("download_url")]
        public string DownloadUrl;

        [JsonProperty("downloads_count")]
        public int DownloadCount;
    }
}
