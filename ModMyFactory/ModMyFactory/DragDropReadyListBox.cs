using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory
{
    public class DragDropReadyListBox : ListBox
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DragDropReadyListBoxItem();
        }
    }
}
