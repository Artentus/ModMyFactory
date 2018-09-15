using System;
using ModMyFactory.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModMyFactory
{
    sealed class ModDependencyJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string value = token.Value<string>();
            return new ModDependency(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ModDependency);
        }
    }
}
