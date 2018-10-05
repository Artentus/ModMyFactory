using ModMyFactory.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModMyFactory.Views
{
    partial class VersionManagementWindow
    {
        const int DefaultWidth = 500, DefaultHeight = 400;
        DispatcherTimer dropTimer;
        string droppedFile = null;

        public VersionManagementWindow()
            : base(App.Instance.Settings.VersionManagerWindowInfo, DefaultWidth, DefaultHeight)
        {
            InitializeComponent();

            Closing += ClosingHandler;

            dropTimer = new DispatcherTimer(DispatcherPriority.Input);
            dropTimer.Interval = TimeSpan.FromMilliseconds(1);
            dropTimer.Tick += DropTimerCallback;
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

        private void VersionListBoxDragOverHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && (((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void VersionListBoxDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (File.Exists(path))
                {
                    droppedFile = path;
                    dropTimer.Start();
                }
            }
        }
        
        private async void DropTimerCallback(object sender, EventArgs e)
        {
            dropTimer.Stop();

            if (!string.IsNullOrEmpty(droppedFile))
            {
                await VersionManagementViewModel.Instance.AddZippedVersion(droppedFile);
                droppedFile = null;
            }
        }

        private void RenameTextBoxLostFocusHandler(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Visibility = Visibility.Collapsed;
        }

        private void RenameTextBoxVisibilityChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if ((bool)e.NewValue)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
}
