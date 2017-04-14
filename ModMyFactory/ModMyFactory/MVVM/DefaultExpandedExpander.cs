using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory.MVVM
{
    class DefaultExpandedExpander : Expander
    {
        static DefaultExpandedExpander()
        {
            IsExpandedProperty.OverrideMetadata(typeof(DefaultExpandedExpander), new FrameworkPropertyMetadata(true));
        }

        public DefaultExpandedExpander()
        {
            IsExpanded = true;
        }
    }
}
