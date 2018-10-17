using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointModSetting : LimitedModSetting<double>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["FloatingPointModSettingTemplate"];

        public FloatingPointModSetting(string name, string ordering, double defaultValue, double minValue, double maxValue)
            : base(name, ordering, defaultValue, minValue, maxValue)
        { }
    }
}
