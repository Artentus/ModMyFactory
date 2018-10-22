using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointModSetting : LimitedModSetting<double>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["FloatingPointModSettingTemplate"];

        public FloatingPointModSetting(string name, LoadTime loadTime, string ordering, double defaultValue, double minValue, double maxValue)
            : base(name, loadTime, ordering, defaultValue, minValue, maxValue)
        { }

        public override IModSetting Clone() => new FloatingPointModSetting(Name, LoadTime, Ordering, DefaultValue, MinValue, MaxValue) { Value = this.Value };
    }
}
