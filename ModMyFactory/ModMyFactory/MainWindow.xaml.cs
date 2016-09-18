using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModMyFactory
{
    partial class MainWindow
    {
        Point dragStartPoint;
        bool dragging;
        bool modsListBoxDeselectionOmitted;
        bool modpacksListBoxDeselectionOmitted;
        int modsListBoxDragStartIndex;
        int modpacksListBoxDragStartIndex;

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

            if (e.Data.GetDataPresent(typeof(List<Mod>)))
            {
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

                e.Effects = (index >= 0) ? DragDropEffects.Link : DragDropEffects.None;
            }
            else if (e.Data.GetDataPresent(typeof(List<Modpack>)))
            {
                int index = -1;
                for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
                {
                    var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (item == null)
                    {
                        e.Effects = DragDropEffects.None;
                        return;
                    }

                    Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                    Point pos = e.GetPosition(item);
                    if (bounds.Contains(pos))
                    {
                        bounds.Height /= 2;
                        index = bounds.Contains(pos) ? i : i + 1;
                        break;
                    }
                }

                if (index >= 0)
                {
                    var modpacks = (List<Modpack>)e.Data.GetData(typeof(List<Modpack>));
                    if (modpacks.Count == 1)
                    {
                        e.Effects = DragDropEffects.Move;
                        Modpack modpack = modpacks[0];
                        ObservableCollection<Modpack> modpackCollection = (ObservableCollection<Modpack>)listBox.ItemsSource;

                        int oldIndex = modpackCollection.IndexOf(modpack);
                        int newIndex = Math.Min(Math.Max(index > oldIndex ? index - 1 : index, 0), modpackCollection.Count - 1);
                        if (newIndex != oldIndex)
                            modpackCollection.Move(oldIndex, newIndex);

                        return;
                    }
                }
                e.Effects = DragDropEffects.None;
            }
        }

        void ModpackListBoxDragLeaveHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (VisualTreeHelper.GetDescendantBounds(listBox).Contains(e.GetPosition(listBox)))
                return;

            if (e.Data.GetDataPresent(typeof(List<Modpack>)))
            {
                var modpacks = (List<Modpack>)e.Data.GetData(typeof(List<Modpack>));
                if (modpacks.Count == 1)
                {
                    Modpack modpack = modpacks[0];
                    ObservableCollection<Modpack> modpackCollection = (ObservableCollection<Modpack>)listBox.ItemsSource;

                    modpackCollection.Move(modpackCollection.IndexOf(modpack), modpacksListBoxDragStartIndex);
                }
            }
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

            if (item != null)
            {
                dragStartPoint = e.GetPosition(null);
                dragging = true;

                if (item.IsSelected)
                    modpacksListBoxDeselectionOmitted = true;
            }
            else
            {
                dragging = false;
                listBox.SelectedItems.Clear();
            }
        }

        private void ModpackListBoxPreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            Vector dragDistance = e.GetPosition(null) - dragStartPoint;
            if (e.LeftButton == MouseButtonState.Pressed && dragging &&
                (Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var mods = new List<Modpack>();
                mods.AddRange(listBox.SelectedItems.Cast<Modpack>());
                modpacksListBoxDragStartIndex = listBox.SelectedIndex;
                DragDrop.DoDragDrop(listBox, mods, DragDropEffects.All);
                dragging = false;
                modpacksListBoxDeselectionOmitted = false;
            }
        }

        private void ModpackListBoxPreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (modpacksListBoxDeselectionOmitted)
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
                    Modpack selectedModpack = (Modpack)listBox.ItemContainerGenerator.ItemFromContainer(item);
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        listBox.SelectedItems.Remove(selectedModpack);
                    }
                    else
                    {
                        for (int i = listBox.SelectedItems.Count - 1; i >= 0; i--)
                        {
                            Modpack modpack = (Modpack)listBox.SelectedItems[i];
                            if (modpack != selectedModpack)
                                listBox.SelectedItems.Remove(modpack);
                        }
                    }
                }

                modpacksListBoxDeselectionOmitted = false;
            }
        }

        private void ModsListBoxDragOverHander(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            int index = -1;
            for (int i = 0; i < ((IList)listBox.ItemsSource).Count; i++)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item == null)
                {
                    e.Effects = DragDropEffects.None;
                    return;
                }

                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point pos = e.GetPosition(item);
                if (bounds.Contains(pos))
                {
                    bounds.Height /= 2;
                    index = bounds.Contains(pos) ? i : i + 1;
                    break;
                }
            }

            if (index >= 0 && e.Data.GetDataPresent(typeof(List<Mod>)))
            {
                var mods = (List<Mod>)e.Data.GetData(typeof(List<Mod>));
                if (mods.Count == 1)
                {
                    e.Effects = DragDropEffects.Move;
                    Mod mod = mods[0];
                    ObservableCollection<Mod> modCollection = (ObservableCollection<Mod>)listBox.ItemsSource;

                    int oldIndex = modCollection.IndexOf(mod);
                    int newIndex = Math.Min(Math.Max(index > oldIndex ? index - 1 : index, 0), modCollection.Count - 1);
                    if (newIndex != oldIndex)
                        modCollection.Move(oldIndex, newIndex);

                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void ModsListBoxDragLeaveHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (VisualTreeHelper.GetDescendantBounds(listBox).Contains(e.GetPosition(listBox)))
                return;

            if (e.Data.GetDataPresent(typeof(List<Mod>)))
            {
                var mods = (List<Mod>)e.Data.GetData(typeof(List<Mod>));
                if (mods.Count == 1)
                {
                    Mod mod = mods[0];
                    ObservableCollection<Mod> modCollection = (ObservableCollection<Mod>)listBox.ItemsSource;

                    modCollection.Move(modCollection.IndexOf(mod), modsListBoxDragStartIndex);
                    listBox.SelectedIndex = modsListBoxDragStartIndex;
                }
            }
        }

        private void ModsListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
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

            if (item != null)
            {
                dragStartPoint = e.GetPosition(null);
                dragging = true;

                if (item.IsSelected)
                    modsListBoxDeselectionOmitted = true;
            }
            else
            {
                dragging = false;
                listBox.SelectedItems.Clear();
            }
        }

        private void ModsListBoxPreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            Vector dragDistance = e.GetPosition(null) - dragStartPoint;
            if (e.LeftButton == MouseButtonState.Pressed && dragging &&
                (Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var mods = new List<Mod>();
                mods.AddRange(listBox.SelectedItems.Cast<Mod>());
                modsListBoxDragStartIndex = listBox.SelectedIndex;
                DragDrop.DoDragDrop(listBox, mods, DragDropEffects.All);
                dragging = false;
                modsListBoxDeselectionOmitted = false;
            }
        }

        private void ModsListBoxPreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (modsListBoxDeselectionOmitted)
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
                    Mod selectedMod = (Mod)listBox.ItemContainerGenerator.ItemFromContainer(item);
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        listBox.SelectedItems.Remove(selectedMod);
                    }
                    else
                    {
                        for (int i = listBox.SelectedItems.Count - 1; i >= 0; i--)
                        {
                            Mod mod = (Mod)listBox.SelectedItems[i];
                            if (mod != selectedMod)
                                listBox.SelectedItems.Remove(mod);
                        }
                    }
                }

                modsListBoxDeselectionOmitted = false;
            }
        }
    }
}
