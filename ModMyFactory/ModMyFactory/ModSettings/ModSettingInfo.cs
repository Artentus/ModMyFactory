using ModMyFactory.Models;
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

        public IModSetting ToSetting(IHasModSettings owner)
        {
            switch (Type)
            {
                case SettingType.Boolean:
                    return new BooleanModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetBoolean());
                case SettingType.Integer:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new IntegerModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetInteger(), MinValue?.GetInteger() ?? long.MinValue, MaxValue?.GetInteger() ?? long.MaxValue);
                    else
                        return new IntegerListModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetInteger(), AllowedValues.Select(value => value.GetInteger()));
                case SettingType.FloatingPoint:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new FloatingPointModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetFloatingPoint(), MinValue?.GetFloatingPoint() ?? decimal.MinValue, MaxValue?.GetFloatingPoint() ?? decimal.MaxValue);
                    else
                        return new FloatingPointListModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetFloatingPoint(), AllowedValues.Select(value => value.GetFloatingPoint()));
                case SettingType.String:
                    if ((AllowedValues == null) || (AllowedValues.Length == 0))
                        return new StringModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetString(), AllowEmptyValue);
                    else
                        return new StringListModSetting(owner, Name, LoadTime, Ordering, DefaultValue.GetString(), AllowedValues.Select(value => value.GetString()));
            }

            return null;
        }
    }
}
