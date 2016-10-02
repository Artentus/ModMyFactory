using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    struct ExtendedModInfo
    {
        [JsonProperty("license_name")]
        public string LicenseName;

        [JsonProperty("license_url")]
        public string LicenseUrl;

        [JsonProperty("github_path")]
        public string GitHubUrl;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("releases")]
        public ModRelease[] Releases;
    }
}
