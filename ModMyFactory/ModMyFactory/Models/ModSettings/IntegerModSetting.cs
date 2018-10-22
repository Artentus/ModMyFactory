using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class IntegerModSetting : LimitedModSetting<long>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["IntegerModSettingTemplate"];

        public IntegerModSetting(string name, LoadTime loadTime, string ordering, long defaultValue, long minValue, long maxValue)
            : base(name, loadTime, ordering, defaultValue, minValue, maxValue)
        { }

        public override IModSetting Clone() => new IntegerModSetting(Name, LoadTime, Ordering, DefaultValue, MinValue, MaxValue) { Value = this.Value };
    }
}
