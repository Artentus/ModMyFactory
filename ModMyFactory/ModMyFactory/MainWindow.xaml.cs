using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ModMyFactory.Win32;

namespace ModMyFactory
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point dragStartPoint;
        bool dragging;

        public MainWindow()
        {
            InitializeComponent();

            dragging = false;
        }

        void CanExecuteCommandDefault(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }

        private void ModpackListBoxDropHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            ListBoxItem item = null;
            for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
            {
                item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (VisualTreeHelper.GetDescendantBounds(item).Contains(e.GetPosition(item)))
                    break;
                item = null;
            }

            if (item != null && e.Data.GetDataPresent(typeof(List<Mod>)))
            {
                var mods = (List<Mod>)e.Data.GetData(typeof(List<Mod>));
                Modpack modpack = (Modpack)listBox.ItemContainerGenerator.ItemFromContainer(item);
                foreach (Mod mod in mods)
                {
                    if (!modpack.Mods.Contains(mod))
                        modpack.Mods.Add(mod);
                }
            }
        }

        private void ModpackListBoxDragOverHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            int index = -1;
            for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
            {
                var item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (VisualTreeHelper.GetDescendantBounds(item).Contains(e.GetPosition(item)))
                {
                    index = i;
                    break;
                }
            }

            e.Effects = (index >= 0 && e.Data.GetDataPresent(typeof(List<Mod>))) ? DragDropEffects.Link : DragDropEffects.None;
        }

        private void ModpackListBoxMouseMoveHandler(object sender, MouseEventArgs e)
        {
            
        }

        private void ModsListBoxMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            ListBoxItem item = null;
            for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
            {
                item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (VisualTreeHelper.GetDescendantBounds(item).Contains(e.GetPosition(item)))
                    break;
                item = null;
            }

            if (item != null && item.IsSelected)
            {
                dragStartPoint = e.GetPosition(null);
                dragging = true;
            }
            else
            {
                dragging = false;
            }
        }

        private void ModsListBoxMouseMoveHandler(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            Vector dragDistance = e.GetPosition(null) - dragStartPoint;
            if (e.LeftButton == MouseButtonState.Pressed && dragging &&
                (Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var mods = new List<Mod>();
                mods.AddRange(listBox.SelectedItems.Cast<Mod>());
                DragDrop.DoDragDrop(listBox, mods, DragDropEffects.Link);
                dragging = false;
            }
        }
    }
}
