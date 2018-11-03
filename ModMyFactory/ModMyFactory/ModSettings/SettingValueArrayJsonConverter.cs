using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactory.ModSettings
{
    sealed class SettingValueArrayJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(((SettingValue[])value).Select(v => v.Value));
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;

                var result = new List<SettingValue>();
                foreach (var child in array.Children())
                {
                    switch (child.Type)
                    {
                        case JTokenType.Boolean:
                            return new SettingValue(child.Value<bool>());
                        case JTokenType.Integer:
                            return new SettingValue(child.Value<long>());
                        case JTokenType.Float:
                            return new SettingValue(child.Value<double>());
                        case JTokenType.String:
                            return new SettingValue(child.Value<string>());
                        default:
                            throw new JsonSerializationException("Incompatible data type.");
                    }
                }
                return result.ToArray();
            }
            else
            {
                throw new JsonSerializationException("Incompatible data type.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SettingValue[]);
        }
    }
}
