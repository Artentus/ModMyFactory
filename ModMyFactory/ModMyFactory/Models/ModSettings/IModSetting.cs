using ModMyFactory.ModSettings;
using ModMyFactory.ModSettings.Serialization;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    interface IModSetting
    {
        IHasModSettings Owner { get; }

        string Name { get; }

        LoadTime LoadTime { get; }

        string Ordering { get; }

        DataTemplate Template { get; }

        IModSettingProxy CreateProxy();

        ModSettingValueTemplate CreateValueTemplate();
    }

    interface IModSetting<T> : IModSetting
    {
        T Value { get; set; }

        T DefaultValue { get; }
    }
}
