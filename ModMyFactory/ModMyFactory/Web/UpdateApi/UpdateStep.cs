using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Web.UpdateApi
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class UpdateStep
    {
        public static UpdateStep StableFromStep(UpdateStep step)
        {
            return new UpdateStep((Version)step.from.Clone(), (Version)step.to.Clone(), (Version)step.to.Clone());
        }

        [JsonProperty("from"), JsonConverter(typeof(VersionConverter))]
        readonly Version from;

        [JsonProperty("to"), JsonConverter(typeof(VersionConverter))]
        readonly Version to;

        [JsonProperty("stable"), JsonConverter(typeof(VersionConverter))]
        readonly Version stable;

        public bool IsStable => stable != null;

        public Version Version => IsStable ? stable : to;

        [JsonConstructor]
        private UpdateStep(Version from, Version to, Version stable)
        {
            this.from = from;
            this.to = to;
            this.stable = stable;
        }
    }
}
