using System.Windows;
using System.Windows.Controls;

namespace ModMyFactory.MVVM
{
    public class DragDropReadyListBox : ListBox
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DragDropReadyListBoxItem();
        }
    }
}
