using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct ModInfo
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("downloads_count")]
        public int DownloadCount;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("summary")]
        public string Summary;

        [JsonProperty("owner")]
        public string Author;
    }
}
