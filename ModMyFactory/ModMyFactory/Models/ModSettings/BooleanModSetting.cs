using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class BooleanModSetting : ModSetting<bool>
    {
        sealed class BooleanModSettingProxy : ModSettingProxy<bool>
        {
            public BooleanModSettingProxy(ModSetting<bool> baseSetting)
                : base(baseSetting)
            { }

            public override IModSettingProxy CreateProxy()
            {
                return new BooleanModSettingProxy(this);
            }
        }

        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["BooleanModSettingTemplate"];

        public BooleanModSetting(string name, LoadTime loadTime, string ordering, bool defaultValue)
            : base(name, loadTime, ordering, defaultValue)
        { }
        
        public override IModSettingProxy CreateProxy()
        {
            return new BooleanModSettingProxy(this);
        }
    }
}
