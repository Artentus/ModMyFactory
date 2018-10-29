using ModMyFactory.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ModMyFactory.MVVM.Converters
{
    class LocalizedNameConverter : IMultiValueConverter
    {
        public LocaleType LocaleType { get; set; }
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var key = (string)values[0];
            var localeSource = (IHasLocale)values[1];
            return localeSource.GetLocale(CultureInfo.CurrentUICulture).GetValue(key, LocaleType);
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
