using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModMyFactory.MVVM.Selectors
{
    class ComboBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplate ListItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parent = container;
            while (parent != null && !(parent is ComboBoxItem) && !(parent is ComboBox))
                parent = VisualTreeHelper.GetParent(parent);
            
            var inDropDown = parent is ComboBoxItem;
            return inDropDown ? ListItemTemplate : SelectedItemTemplate;
        }
    }
}
