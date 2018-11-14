using ModMyFactory.Models;
using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory.MVVM.Selectors
{
    class ModUpdateTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptyItemTemplate { get; set; }
        public DataTemplate FilledItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var updateTemplate = item as ModUpdateInfo;
            if ((updateTemplate != null) && (updateTemplate.ModVersions.Count > 0)) return FilledItemTemplate;
            return EmptyItemTemplate;
        }
    }
}
