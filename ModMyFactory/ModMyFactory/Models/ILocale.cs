namespace ModMyFactory.Models
{
    interface ILocale
    {
        string GetValue(string key, LocaleType type);
    }
}
