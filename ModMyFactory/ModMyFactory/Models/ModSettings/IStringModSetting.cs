namespace ModMyFactory.Models.ModSettings
{
    interface IStringModSetting : IModSetting<string>
    {
        bool AllowEmptyValue { get; }
    }
}
