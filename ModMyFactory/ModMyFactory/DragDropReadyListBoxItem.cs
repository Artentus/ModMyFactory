using System.Windows.Controls;
using System.Windows.Input;

namespace ModMyFactory
{
    public class DragDropReadyListBoxItem : ListBoxItem
    {
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (IsSelected)
                e.Handled = true;
            else
                base.OnMouseDown(e);
        }
    }
}
