using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    interface IModSetting
    {
        string Name { get; }

        string Ordering { get; }

        DataTemplate Template { get; }
    }
}
