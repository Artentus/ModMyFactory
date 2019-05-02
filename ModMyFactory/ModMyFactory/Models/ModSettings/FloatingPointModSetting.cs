using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointModSetting : LimitedModSetting<decimal>
    {
        sealed class FloatingPointModSettingProxy : LimitedModSettingProxy<decimal>
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

        public FloatingPointModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, decimal defaultValue, decimal minValue, decimal maxValue)
            : base(owner, name, loadTime, ordering, defaultValue, minValue, maxValue)
        { }

        public override IModSettingProxy CreateProxy()
        {
            return new FloatingPointModSettingProxy(this);
        }
    }
}
