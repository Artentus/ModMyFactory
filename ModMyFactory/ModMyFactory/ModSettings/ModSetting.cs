using Newtonsoft.Json;
using System;

namespace ModMyFactory.ModSettings
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ModSetting
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(SettingTypeJsonConverter))]
        public SettingType Type { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("setting_type")]
        [JsonConverter(typeof(LoadTimeJsonConverter))]
        public LoadTime LoadTime { get; }

        [JsonProperty("default_value")]
        [JsonConverter(typeof(SettingValueJsonConverter))]
        public SettingValue DefaultValue { get; }

        [JsonProperty("minimum_value")]
        [JsonConverter(typeof(SettingValueJsonConverter))]
        public SettingValue MinValue { get; }

        [JsonProperty("maximum_value")]
        [JsonConverter(typeof(SettingValueJsonConverter))]
        public SettingValue MaxValue { get; }

        [JsonProperty("allowed_values")]
        [JsonConverter(typeof(SettingValueArrayJsonConverter))]
        public SettingValue[] AllowedValues { get; }

        [JsonProperty("order")]
        public string Ordering { get; }

        [JsonConstructor]
        public ModSetting(SettingType type, string name, LoadTime loadTime, SettingValue defaultValue, SettingValue minValue, SettingValue maxValue, SettingValue[] allowedValues, string ordering)
        {
            Type = type;
            Name = name;
            LoadTime = loadTime;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            AllowedValues = allowedValues;
            Ordering = ordering;

            if (defaultValue.Type != type)
                throw new ArgumentException("Value types do not match.", nameof(defaultValue));
            if ((minValue != null) && (minValue.Type != type))
                throw new ArgumentException("Value types do not match.", nameof(minValue));
            if ((maxValue != null) && (maxValue.Type != type))
                throw new ArgumentException("Value types do not match.", nameof(maxValue));
            if (allowedValues != null)
            {
                foreach (var value in allowedValues)
                {
                    if (value.Type != type)
                        throw new ArgumentException("Value types do not match.", nameof(allowedValues));
                }
            }
        }
    }
}
