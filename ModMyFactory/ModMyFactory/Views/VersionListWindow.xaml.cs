using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory.Views
{
    partial class VersionListWindow
    {
        public VersionListWindow()
        {
            InitializeComponent();
        }

        private void DownloadButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void VersionListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (e.ChangedButton != MouseButton.Left)
                return;
            
            ListBoxItem item = null;
            int itemCount = ((ICollection)((ListCollectionView)listBox.ItemsSource).SourceCollection).Count;
            for (int i = 0; i < itemCount; i++)
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
