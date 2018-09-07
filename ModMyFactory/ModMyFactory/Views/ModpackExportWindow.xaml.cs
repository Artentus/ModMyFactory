using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory.Views
{
    partial class ModpackExportWindow
    {
        const int DefaultWidth = 700, DefaultHeight = 800;

        public ModpackExportWindow()
            : base(App.Instance.Settings.ModpackExportWindowInfo, DefaultWidth, DefaultHeight)
        {
            InitializeComponent();

            Closing += ClosingHandler;
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            App.Instance.Settings.ModpackExportWindowInfo = CreateInfo();
            App.Instance.Settings.Save();
        }

        private void ExportButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ModpackListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (e.ChangedButton != MouseButton.Left)
                return;

            int itemCount = ((ICollectionView)listBox.ItemsSource).Cast<object>().Count();
            ListBoxItem item = null;
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
