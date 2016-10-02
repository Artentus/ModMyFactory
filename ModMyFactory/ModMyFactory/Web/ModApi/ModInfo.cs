using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct ModInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("downloads_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("visits_count")]
        public int ViewCount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("owner")]
        public string Author { get; set; }
    }
}
