using ModMyFactory.MVVM.Sorters;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ModMyFactory.MVVM.Converters
{
    [ValueConversion(typeof(ModInfoSorterMode), typeof(string))]
    class ModInfoSorterModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ModInfoSorterMode)) throw new ArgumentException("Value has to be of type ModInfoSorterMode.", nameof(value));
            var sortingMode = (ModInfoSorterMode)value;
            
            return App.Instance.GetLocalizedResourceString(sortingMode.ToString() + "SortingMode");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
