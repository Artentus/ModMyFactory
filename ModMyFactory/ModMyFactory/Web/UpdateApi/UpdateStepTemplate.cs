using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Web.UpdateApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class UpdateStepTemplate
    {
        [JsonProperty("from"), JsonConverter(typeof(VersionConverter))]
        public Version From { get; set; }

        [JsonProperty("to"), JsonConverter(typeof(VersionConverter))]
        public Version To { get; set; }

        [JsonProperty("stable"), JsonConverter(typeof(VersionConverter))]
        public Version Stable { get; set; }
    }
}
