using Newtonsoft.Json;

namespace ModMyFactory.FactorioUpdate
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class UpdatePackageInfo
    {
        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("apiVersion")]
        public int ApiVersion { get; }

        [JsonProperty("files")]
        public FileUpdateInfo[] UpdatedFiles { get; }

        public string PackageDirectory { get; set; }

        [JsonConstructor]
        public UpdatePackageInfo(string type, int apiVersion, FileUpdateInfo[] updatedFiles)
        {
            Type = type;
            ApiVersion = apiVersion;
            UpdatedFiles = updatedFiles;
        }
    }
}
