using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory
{
    static class MenuItemProperties
    {
        public static readonly DependencyProperty ShowCheckedProperty =
            DependencyProperty.RegisterAttached("ShowChecked", typeof(bool), typeof(MenuItemProperties), new PropertyMetadata(false));

        public static bool GetShowChecked(MenuItem item)
        {
            return (bool)item.GetValue(ShowCheckedProperty);
        }

        public static void SetShowChecked(MenuItem item, bool value)
        {
            item.SetValue(ShowCheckedProperty, value);
        }
    }
}
