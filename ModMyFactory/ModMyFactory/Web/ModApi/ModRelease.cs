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
        public Version Version { get; set; }

        [JsonProperty("game_version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version FactorioVersion { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("downloads_count")]
        public int DownloadCount { get; set; }
    }
}
