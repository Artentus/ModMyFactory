using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct ApiPage
    {
        [JsonProperty("pagination")]
        public PageInfo Info;

        [JsonProperty("results")]
        public ModInfo[] Mods;
    }
}
