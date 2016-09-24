using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory
{
    internal partial class LinkPropertiesWindow
    {
        public LinkPropertiesWindow()
        {
            InitializeComponent();
        }

        private void CreateButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ModpackListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (e.ChangedButton != MouseButton.Left)
                return;

            ListBoxItem item = null;
            for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
            {
                item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (VisualTreeHelper.GetDescendantBounds(item).Contains(e.GetPosition(item)))
                    break;
                item = null;
            }

            if (item == null) listBox.SelectedItem = null;
        }
    }
}
