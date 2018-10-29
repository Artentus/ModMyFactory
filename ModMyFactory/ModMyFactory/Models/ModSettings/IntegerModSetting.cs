using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class IntegerModSetting : LimitedModSetting<long>
    {
        sealed class IntegerModSettingProxy : LimitedModSettingProxy<long>
        {
            public IntegerModSettingProxy(IntegerModSetting baseSetting)
                : base(baseSetting)
            { }

            private IntegerModSettingProxy(IntegerModSettingProxy baseSetting)
                : base(baseSetting)
            { }

            public override IModSettingProxy CreateProxy()
            {
                return new IntegerModSettingProxy(this);
            }
        }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["IntegerModSettingTemplate"];

        public IntegerModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, long defaultValue, long minValue, long maxValue)
            : base(owner, name, loadTime, ordering, defaultValue, minValue, maxValue)
        { }

        public override IModSettingProxy CreateProxy()
        {
            return new IntegerModSettingProxy(this);
        }
    }
}
