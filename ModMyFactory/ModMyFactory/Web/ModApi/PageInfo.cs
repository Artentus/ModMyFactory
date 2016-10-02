using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct PageInfo
    {
        [JsonProperty("page_count")]
        public int PageCount;

        [JsonProperty("page")]
        public int PageNumber;

        [JsonProperty("count")]
        public int ModCount;

        [JsonProperty("page_size")]
        public int ModsOnPage;

        [JsonProperty("links")]
        public PageLinks Links;
    }
}
