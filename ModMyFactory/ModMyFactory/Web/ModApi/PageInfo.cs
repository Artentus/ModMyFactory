using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class PageInfo
    {
        [JsonProperty("page_count")]
        public int PageCount { get; set; }

        [JsonProperty("page")]
        public int PageNumber { get; set; }

        [JsonProperty("count")]
        public int ModCount { get; set; }

        [JsonProperty("page_size")]
        public int ModsOnPage { get; set; }

        [JsonProperty("links")]
        public PageLinks Links { get; set; }
    }
}
