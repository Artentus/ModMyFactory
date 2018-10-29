using ModMyFactory.ModSettings;
using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class StringListModSetting : ListModSetting<string>
    {
        sealed class StringListModSettingProxy : ListModSettingProxy<string>
        {
            public StringListModSettingProxy(StringListModSetting baseSetting)
                : base(baseSetting)
            { }

            private StringListModSettingProxy(StringListModSettingProxy baseSetting)
                : base(baseSetting)
            { }

            public override IModSettingProxy CreateProxy()
            {
                return new StringListModSettingProxy(this);
            }
        }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["StringListModSettingTemplate"];

        public StringListModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, string defaultValue, IEnumerable<string> allowedValues)
            : base(owner, name, loadTime, ordering, defaultValue, allowedValues)
        { }

        public override IModSettingProxy CreateProxy()
        {
            return new StringListModSettingProxy(this);
        }
    }
}
