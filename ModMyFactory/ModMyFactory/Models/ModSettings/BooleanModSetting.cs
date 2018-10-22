using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class BooleanModSetting : ModSetting<bool>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["BooleanModSettingTemplate"];

        public BooleanModSetting(string name, LoadTime loadTime, string ordering, bool defaultValue)
            : base(name, loadTime, ordering, defaultValue)
        { }

        public override IModSetting Clone() => new BooleanModSetting(Name, LoadTime, Ordering, DefaultValue) { Value = this.Value };
    }
}
