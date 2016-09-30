using System;
using System.Globalization;
using System.Windows.Data;

namespace ModMyFactory.MVVM.Converters
{
    /// <summary>
    /// Multiplies two or more numeric values in a multi binding.
    /// </summary>
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            foreach (var value in values)
                result *= (double)value;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
