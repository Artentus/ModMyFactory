using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class PageLinks
    {
        [JsonProperty("prev")]
        public string PreviousPage { get; set; }

        [JsonProperty("next")]
        public string NextPage { get; set; }

        [JsonProperty("first")]
        public string FirstPage { get; set; }

        [JsonProperty("last")]
        public string LastPage { get; set; }
    }
}
