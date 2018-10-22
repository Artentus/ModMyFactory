using ModMyFactory.ModSettings;
using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class IntegerListModSetting : ListModSetting<long>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["IntegerListModSettingTemplate"];

        public IntegerListModSetting(string name, LoadTime loadTime, string ordering, long defaultValue, IEnumerable<long> allowedValues)
            : base(name, loadTime, ordering, defaultValue, allowedValues)
        { }

        public override IModSetting Clone() => new IntegerListModSetting(Name, LoadTime, Ordering, DefaultValue, AllowedValues) { Value = this.Value };
    }
}
