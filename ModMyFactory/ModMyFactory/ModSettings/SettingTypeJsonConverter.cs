using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings
{
    sealed class SettingTypeJsonConverter : JsonConverter
    {
        private static readonly Dictionary<SettingType, string> to;
        private static readonly Dictionary<string, SettingType> from;

        static SettingTypeJsonConverter()
        {
            to = new Dictionary<SettingType, string>
            {
                { SettingType.Boolean, "bool-setting" },
                { SettingType.Integer, "int-setting" },
                { SettingType.FloatingPoint, "double-setting" },
                { SettingType.String, "string-setting" },
            };

            from = new Dictionary<string, SettingType>
            {
                { "bool-setting", SettingType.Boolean },
                { "int-setting", SettingType.Integer },
                { "double-setting", SettingType.FloatingPoint },
                { "string-setting", SettingType.String },
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(to[(SettingType)value]);
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
            return objectType == typeof(SettingType);
        }
    }
}
