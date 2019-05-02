using ModMyFactory.ModSettings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ModMyFactory.MVVM.Converters
{
    [ValueConversion(typeof(LoadTime), typeof(string))]
    class LoadTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LoadTime)) throw new ArgumentException("Value has to be of type LoadTime.", nameof(value));
            var sortingMode = (LoadTime)value;

            return App.Instance.GetLocalizedResourceString(sortingMode.ToString() + "LoadTime");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
