using System.Collections.Generic;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    sealed class IntegerListModSetting : ListModSetting<long>
    {
        public override DataTemplate Template => (DataTemplate)App.Instance.Resources["IntegerListModSettingTemplate"];

        public IntegerListModSetting(string name, string ordering, long defaultValue, IEnumerable<long> allowedValues)
            : base(name, ordering, defaultValue, allowedValues)
        { }
    }
}
