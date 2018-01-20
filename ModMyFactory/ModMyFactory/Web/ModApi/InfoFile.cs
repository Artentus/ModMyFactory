using System;
using Newtonsoft.Json;

namespace ModMyFactory.Web.ModApi
{
    sealed class InfoFile
    {
        [JsonProperty("factorio_version")]
        [JsonConverter(typeof(TwoPartVersionConverter))]
        public Version FactorioVersion { get; set; }
    }
}
