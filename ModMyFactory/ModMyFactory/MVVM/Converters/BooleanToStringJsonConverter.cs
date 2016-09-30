using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModMyFactory.MVVM.Converters
{
    sealed class BooleanToStringJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(((bool)value).ToString().ToLower());
            token.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return bool.Parse(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
}
