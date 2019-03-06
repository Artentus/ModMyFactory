using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModMyFactory.MVVM.Converters
{
    sealed class AdvancedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bVal = (bool)value;

            if ((parameter != null) && (parameter is bool))
            {
                bool bParam = (bool)parameter;
                if (bParam) bVal = !bVal;
            }

            return bVal ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
