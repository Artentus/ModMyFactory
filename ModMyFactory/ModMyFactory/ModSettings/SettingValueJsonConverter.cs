using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ModMyFactory.ModSettings
{
    sealed class SettingValueJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(((SettingValue)value).Value);
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Boolean:
                    return new SettingValue(token.Value<bool>());
                case JTokenType.Integer:
                    return new SettingValue(token.Value<long>());
                case JTokenType.Float:
                    return new SettingValue(token.Value<double>());
                case JTokenType.String:
                    return new SettingValue(token.Value<string>());
                default:
                    throw new JsonSerializationException("Incompatible data type.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SettingValue);
        }
    }
}
