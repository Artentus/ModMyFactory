using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ApiPage
    {
        [JsonProperty("pagination")]
        public PageInfo Info { get; set; }

        [JsonProperty("results")]
        public ModInfo[] Mods { get; set; }
    }
}
