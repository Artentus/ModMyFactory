using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory.Views
{
    partial class VersionManagementWindow
    {
        const int DefaultWidth = 500, DefaultHeight = 400;

        public VersionManagementWindow()
            : base(App.Instance.Settings.VersionManagerWindowInfo, DefaultWidth, DefaultHeight)
        {
            InitializeComponent();

            Closing += ClosingHandler;
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            App.Instance.Settings.VersionManagerWindowInfo = CreateInfo();
            App.Instance.Settings.Save();
        }

        private void VersionListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
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
