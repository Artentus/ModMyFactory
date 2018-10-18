using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class BooleanModSetting : ModSetting<bool>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["BooleanModSettingTemplate"];

        public BooleanModSetting(string name, string ordering, bool defaultValue)
            : base(name, ordering, defaultValue)
        { }

        public override IModSetting Clone() => new BooleanModSetting(Name, Ordering, DefaultValue) { Value = this.Value };
    }
}
