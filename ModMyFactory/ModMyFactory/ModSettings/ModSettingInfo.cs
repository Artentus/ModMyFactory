using ModMyFactory.Models.ModSettings;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ModMyFactory.ModSettings
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ModSettingInfo
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

        [JsonProperty("allow_blank")]
        public bool AllowEmptyValue { get; }

        [JsonProperty("order")]
        public string Ordering { get; }

        [JsonConstructor]
        public ModSettingInfo(SettingType type, string name, LoadTime loadTime, SettingValue defaultValue, SettingValue minValue, SettingValue maxValue, SettingValue[] allowedValues, bool allowEmptyValue, string ordering)
        {
            Type = type;
            Name = name;
            LoadTime = loadTime;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            AllowedValues = allowedValues;
            AllowEmptyValue = allowEmptyValue;
            Ordering = ordering;
            
            if (!defaultValue.Type.CompatibleTo(type))
                throw new ArgumentException("Value types do not match.", nameof(defaultValue));
            if ((minValue != null) && !minValue.Type.CompatibleTo(type))
                throw new ArgumentException("Value types do not match.", nameof(minValue));
            if ((maxValue != null) && !maxValue.Type.CompatibleTo(type))
                throw new ArgumentException("Value types do not match.", nameof(maxValue));
            if (allowedValues != null)
            {
                foreach (var value in allowedValues)
                {
                    if (!value.Type.CompatibleTo(type))
                        throw new ArgumentException("Value types do not match.", nameof(allowedValues));
                }
            }
        }

        public IModSetting ToSetting()
        {
            switch (Type)
            {
                case SettingType.Boolean:
                    return new BooleanModSetting(Name, Ordering, DefaultValue.GetBoolean());
                case SettingType.Integer:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new IntegerModSetting(Name, Ordering, DefaultValue.GetInteger(), MinValue?.GetInteger() ?? long.MinValue, MaxValue?.GetInteger() ?? long.MaxValue);
                    else
                        return new IntegerListModSetting(Name, Ordering, DefaultValue.GetInteger(), AllowedValues.Select(value => value.GetInteger()));
                case SettingType.FloatingPoint:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new FloatingPointModSetting(Name, Ordering, DefaultValue.GetFloatingPoint(), MinValue?.GetFloatingPoint() ?? double.NegativeInfinity, MaxValue?.GetFloatingPoint() ?? double.PositiveInfinity);
                    else
                        return new FloatingPointListModSetting(Name, Ordering, DefaultValue.GetFloatingPoint(), AllowedValues.Select(value => value.GetFloatingPoint()));
                case SettingType.String:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new StringModSetting(Name, Ordering, DefaultValue.GetString(), AllowEmptyValue);
                    else
                        return new StringListModSetting(Name, Ordering, DefaultValue.GetString(), AllowedValues.Select(value => value.GetString()));
            }

            return null;
        }
    }
}
