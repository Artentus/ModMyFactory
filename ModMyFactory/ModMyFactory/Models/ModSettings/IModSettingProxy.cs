namespace ModMyFactory.Models.ModSettings
{
    interface IModSettingProxy : IModSetting
    {
        bool Override { get; set; }
    }

    interface IModSettingProxy<T> : IModSettingProxy, IModSetting<T>
    { }
}
