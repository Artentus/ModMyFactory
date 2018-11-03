namespace ModMyFactory.Models.ModSettings
{
    interface ILimitedModSetting<T> : IModSetting<T>
    {
        T MinValue { get; }

        T MaxValue { get; }
    }
}
