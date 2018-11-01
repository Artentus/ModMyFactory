using Newtonsoft.Json;
using System;

namespace ModMyFactory.ModSettings.Serialization
{
    class ModSettingValueTemplate
    {
        [JsonProperty("value")]
        public object Value { get; }

        [JsonConstructor]
        public ModSettingValueTemplate(object value)
        {
            Value = value;
        }

        public T GetValue<T>()
        {
            if (ReferenceEquals(Value, null)) return default(T);
            if (Value.GetType() == typeof(T)) return (T)Value;
            return (T)Convert.ChangeType(Value, typeof(T));
        }
    }
}
