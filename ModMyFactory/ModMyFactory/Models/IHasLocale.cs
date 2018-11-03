using System.Globalization;

namespace ModMyFactory.Models
{
    interface IHasLocale
    {
        ILocale GetLocale(CultureInfo culture);
    }
}
