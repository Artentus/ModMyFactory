using Newtonsoft.Json;

namespace ModMyFactory.Web.AuthenticationApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class AuthenticationInfo
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
