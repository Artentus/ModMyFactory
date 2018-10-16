using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    sealed class LoadTimeJsonConverter : JsonConverter
    {
        private static readonly Dictionary<LoadTime, string> to;
        private static readonly Dictionary<string, LoadTime> from;

        static LoadTimeJsonConverter()
        {
            to = new Dictionary<LoadTime, string>
            {
                { LoadTime.Startup, "startup" },
                { LoadTime.RuntimeGlobal, "runtime-global" },
                { LoadTime.RuntimeUser, "runtime-per-user" },
            };

            from = new Dictionary<string, LoadTime>
            {
                { "startup", LoadTime.Startup },
                { "runtime-global", LoadTime.RuntimeGlobal },
                { "runtime-per-user", LoadTime.RuntimeUser },
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(to[(LoadTime)value]);
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return from[value];
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LoadTime);
        }
    }
}
