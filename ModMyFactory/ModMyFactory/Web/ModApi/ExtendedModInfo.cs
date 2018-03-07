using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ExtendedModInfo
    {
        [JsonProperty("releases")]
        public ModRelease[] Releases { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        // Removed from API

        //[JsonProperty("license_name")]
        //public string LicenseName { get; set; }

        //[JsonProperty("license_url")]
        //public string LicenseUrl { get; set; }

        //[JsonProperty("github_path")]
        //public string GitHubUrl { get; set; }
    }
}
