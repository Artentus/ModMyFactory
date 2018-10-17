using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class IntegerModSetting : LimitedModSetting<long>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["IntegerModSettingTemplate"];

        public IntegerModSetting(string name, string ordering, long defaultValue, long minValue, long maxValue)
            : base(name, ordering, defaultValue, minValue, maxValue)
        { }
    }
}
