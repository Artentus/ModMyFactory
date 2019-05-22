using ModMyFactory.ModSettings;
using ModMyFactory.ModSettings.Serialization;
using System.Windows;
using System.Windows.Input;

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

        ICommand ResetCommand { get; }

        void Reset();
    }

    interface IModSetting<T> : IModSetting
    {
        T Value { get; set; }

        T DefaultValue { get; }
    }
}
