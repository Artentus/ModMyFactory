using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory.Views
{
    partial class VersionManagementWindow
    {
        public VersionManagementWindow()
        {
            InitializeComponent();

            WindowInfo windowInfo = App.Instance.Settings.VersionManagerWindowInfo;
            if (windowInfo == WindowInfo.Empty)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowState = windowInfo.State;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Width = windowInfo.Width;
                Height = windowInfo.Height;
                Left = windowInfo.PosX;
                Top = windowInfo.PosY;
            }

            Closing += ClosingHandler;
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            var windowInfo = new WindowInfo();
            if (WindowState == WindowState.Normal)
            {
                windowInfo.PosX = (int)Left;
                windowInfo.PosY = (int)Top;
                windowInfo.Width = (int)Width;
                windowInfo.Height = (int)Height;
            }
            else
            {
                windowInfo.PosX = (int)RestoreBounds.Left;
                windowInfo.PosY = (int)RestoreBounds.Top;
                windowInfo.Width = (int)RestoreBounds.Width;
                windowInfo.Height = (int)RestoreBounds.Height;
            }
            windowInfo.State = WindowState == WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;

            App.Instance.Settings.VersionManagerWindowInfo = windowInfo;
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
