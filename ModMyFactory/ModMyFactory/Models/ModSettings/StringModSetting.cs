using ModMyFactory.ModSettings;
using System;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class StringModSetting : ModSetting<string>, IStringModSetting
    {
        sealed class StringModSettingProxy : ModSettingProxy<string>, IStringModSetting
        {
            public override string Value
            {
                get => base.Value;
                set
                {
                    if (!AllowEmptyValue && string.IsNullOrEmpty(value))
                        throw new ArgumentException("Value not allowed.", nameof(value));

                    base.Value = value;
                }
            }

            public bool AllowEmptyValue { get; }

            public StringModSettingProxy(StringModSetting baseSetting)
                : base(baseSetting)
            {
                AllowEmptyValue = baseSetting.AllowEmptyValue;
            }

            private StringModSettingProxy(StringModSettingProxy baseSetting)
                : base(baseSetting)
            {
                AllowEmptyValue = baseSetting.AllowEmptyValue;
            }

            public override IModSettingProxy CreateProxy()
            {
                return new StringModSettingProxy(this);
            }
        }

        public override string Value
        {
            get => base.Value;
            set
            {
                if (!AllowEmptyValue && string.IsNullOrEmpty(value))
                    throw new ArgumentException("Value not allowed.", nameof(value));

                base.Value = value;
            }
        }

        public bool AllowEmptyValue { get; }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["StringModSettingTemplate"];

        public StringModSetting(string name, LoadTime loadTime, string ordering, string defaultValue, bool allowEmptyValue)
            : base(name, loadTime, ordering, defaultValue)
        {
            if (!allowEmptyValue && string.IsNullOrEmpty(defaultValue))
                throw new ArgumentException("Value not allowed.", nameof(defaultValue));

            AllowEmptyValue = allowEmptyValue;
        }

        public override IModSettingProxy CreateProxy()
        {
            return new StringModSettingProxy(this);
        }
    }
}
