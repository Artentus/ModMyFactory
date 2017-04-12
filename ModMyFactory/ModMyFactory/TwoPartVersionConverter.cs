using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModMyFactory
{
    class TwoPartVersionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(((Version)value).ToString(2).ToLower());
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            var v = Version.Parse(value);
            return new Version(v.Major, v.Minor);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Version);
        }
    }
}
