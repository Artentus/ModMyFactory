using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class StringListModSetting : ListModSetting<string>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["StringListModSettingTemplate"];

        public StringListModSetting(string name, string ordering, string defaultValue, IEnumerable<string> allowedValues)
            : base(name, ordering, defaultValue, allowedValues)
        { }

        public override IModSetting Clone() => new StringListModSetting(Name, Ordering, DefaultValue, AllowedValues) { Value = this.Value };
    }
}
