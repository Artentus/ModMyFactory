using System.Collections.Generic;

namespace ModMyFactory.Models.ModSettings
{
    interface IListModSetting<T> : IModSetting<T>
    {
        IReadOnlyCollection<T> AllowedValues { get; }
    }
}
