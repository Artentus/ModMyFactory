using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct PageLinks
    {
        [JsonProperty("prev")]
        public string PreviousPage;

        [JsonProperty("next")]
        public string NextPage;

        [JsonProperty("first")]
        public string FirstPage;

        [JsonProperty("last")]
        public string LastPage;
    }
}
