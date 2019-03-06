using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ModMyFactory
{
    class GameVersionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(((GameCompatibleVersion)value).ToString().ToLower());
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return GameCompatibleVersion.Parse(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GameCompatibleVersion);
        }
    }
}
