using ModMyFactory.ModSettings;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    interface IModSetting
    {
        string Name { get; }

        LoadTime LoadTime { get; }

        string Ordering { get; }

        DataTemplate Template { get; }

        IModSettingProxy CreateProxy();
    }

    interface IModSetting<T> : IModSetting
    {
        T Value { get; set; }

        T DefaultValue { get; }
    }
}
