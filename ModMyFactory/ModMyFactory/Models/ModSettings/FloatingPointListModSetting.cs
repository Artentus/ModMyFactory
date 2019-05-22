using ModMyFactory.ModSettings;
using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointListModSetting : ListModSetting<decimal>
    {
        sealed class FloatingPointListModSettingProxy : ListModSettingProxy<decimal>
        {
            public FloatingPointListModSettingProxy(FloatingPointListModSetting baseSetting)
                : base(baseSetting)
            { }

            private FloatingPointListModSettingProxy(FloatingPointListModSettingProxy baseSetting)
                : base(baseSetting)
            { }

            public override IModSettingProxy CreateProxy()
            {
                return new FloatingPointListModSettingProxy(this);
            }
        }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["FloatingPointListModSettingTemplate"];

        public FloatingPointListModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, decimal defaultValue, IEnumerable<decimal> allowedValues)
            : base(owner, name, loadTime, ordering, defaultValue, allowedValues)
        { }

        public override IModSettingProxy CreateProxy()
        {
            return new FloatingPointListModSettingProxy(this);
        }
    }
}
