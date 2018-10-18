using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class FloatingPointListModSetting : ListModSetting<double>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["FloatingPointListModSettingTemplate"];

        public FloatingPointListModSetting(string name, string ordering, double defaultValue, IEnumerable<double> allowedValues)
            : base(name, ordering, defaultValue, allowedValues)
        { }

        public override IModSetting Clone() => new FloatingPointListModSetting(Name, Ordering, DefaultValue, AllowedValues) { Value = this.Value };
    }
}
