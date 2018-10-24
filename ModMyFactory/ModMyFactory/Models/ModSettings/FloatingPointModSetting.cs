using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointModSetting : LimitedModSetting<double>
    {
        sealed class FloatingPointModSettingProxy : LimitedModSettingProxy<double>
        {
            public FloatingPointModSettingProxy(FloatingPointModSetting baseSetting)
                : base(baseSetting)
            { }

            private FloatingPointModSettingProxy(FloatingPointModSettingProxy baseSetting)
                : base(baseSetting)
            { }

            public override IModSettingProxy CreateProxy()
            {
                return new FloatingPointModSettingProxy(this);
            }
        }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["FloatingPointModSettingTemplate"];

        public FloatingPointModSetting(string name, LoadTime loadTime, string ordering, double defaultValue, double minValue, double maxValue)
            : base(name, loadTime, ordering, defaultValue, minValue, maxValue)
        { }

        public override IModSettingProxy CreateProxy()
        {
            return new FloatingPointModSettingProxy(this);
        }
    }
}
