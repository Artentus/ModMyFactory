using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    class ModInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("downloads_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("owner")]
        public string Author { get; set; }

        [JsonProperty("latest_release")]
        public virtual ModRelease LatestRelease { get; set; }
    }
}
