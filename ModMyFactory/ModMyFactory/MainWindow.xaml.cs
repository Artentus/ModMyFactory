using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public MainWindow()
        {
            InitializeComponent();

            dragging = false;

            if (App.Instance.Settings.Width < 0 && App.Instance.Settings.Height < 0)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                WindowState = App.Instance.Settings.State;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Width = App.Instance.Settings.Width;
                Height = App.Instance.Settings.Height;
                Left = App.Instance.Settings.PosX;
                Top = App.Instance.Settings.PosY;
            }

            Loaded += LoadedHandler;
            Closing += ClosingHandler;
        }

        private void LoadedHandler(object sender, EventArgs e)
        {
            if (!Program.NoUpdateCheck)
                ViewModel.UpdateCommand.Execute(true);
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                App.Instance.Settings.PosX = (int)Left;
                App.Instance.Settings.PosY = (int)Top;
                App.Instance.Settings.Width = (int)Width;
                App.Instance.Settings.Height = (int)Height;
            }
            else
            {
                App.Instance.Settings.PosX = (int)RestoreBounds.Left;
                App.Instance.Settings.PosY = (int)RestoreBounds.Top;
                App.Instance.Settings.Width = (int)RestoreBounds.Width;
                App.Instance.Settings.Height = (int)RestoreBounds.Height;
            }
            App.Instance.Settings.State = WindowState == WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;
            App.Instance.Settings.Save();
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

        private static ListBoxItem GetItem(ListBox listBox, Func<ListBoxItem, Point> positionSelector)
        {
            int itemCount = ((ICollectionView)listBox.ItemsSource).Cast<object>().Count();
            for (int i = 0; i < itemCount; i++)
            {
                ListBoxItem item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                Rect itemBounds = VisualTreeHelper.GetDescendantBounds(item);
                Point relativePosition = positionSelector.Invoke(item);

                if (itemBounds.Contains(relativePosition))
                    return item;
            }

            return null;
        }
        
        private void ModpackListBoxDropHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            ListBoxItem item = GetItem(listBox, e.GetPosition);
            if (item != null)
            {
                if (e.Data.GetDataPresent(typeof(List<Mod>)))
                {
                    var mods = (List<Mod>)e.Data.GetData(typeof(List<Mod>));
                    Modpack parent = (Modpack)listBox.ItemContainerGenerator.ItemFromContainer(item);
                    foreach (Mod mod in mods)
                    {
                        if (!parent.Contains(mod))
                        {
                            var reference = new ModReference(mod, parent);
                            parent.Mods.Add(reference);
                        }
                    }

                    ViewModel.ModpackTemplateList.Update(ViewModel.Modpacks);
                    ViewModel.ModpackTemplateList.Save();
                }
                else if (e.Data.GetDataPresent(typeof(List<Modpack>)))
                {
                    var modpacks = (List<Modpack>)e.Data.GetData(typeof(List<Modpack>));
                    Modpack parent = (Modpack)listBox.ItemContainerGenerator.ItemFromContainer(item);
                    foreach (Modpack modpack in modpacks)
                    {
                        if (modpack != parent && !parent.Contains(modpack) && !modpack.Contains(parent, true))
                        {
                            var reference = new ModpackReference(modpack, parent);
                            reference.ParentViews.Add(parent.ModsView);
                            parent.Mods.Add(reference);
                        }
                    }

                    ViewModel.ModpackTemplateList.Update(ViewModel.Modpacks);
                    ViewModel.ModpackTemplateList.Save();
                }
            }
        }

        private void ModpackListBoxDragOverHandler(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(List<Mod>)) || e.Data.GetDataPresent(typeof(List<Modpack>)))
            {
                ListBoxItem item = GetItem(listBox, e.GetPosition);
                if (item != null) e.Effects = DragDropEffects.Link;
            }
        }
        
        private void ModpackListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (e.ChangedButton != MouseButton.Left)
                return;

            ListBoxItem item = GetItem(listBox, e.GetPosition);
            if (item != null && item.IsMouseOver)
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

        private void ModpackListBoxMouseMoveHandler(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            Vector dragDistance = e.GetPosition(null) - dragStartPoint;
            if (e.LeftButton == MouseButtonState.Pressed && dragging &&
                (Math.Abs(dragDistance.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(dragDistance.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var mods = new List<Modpack>();
                mods.AddRange(listBox.SelectedItems.Cast<Modpack>());
                DragDrop.DoDragDrop(listBox, mods, DragDropEffects.Link);
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

                ListBoxItem item = GetItem(listBox, e.GetPosition);
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
        
        private void ModsListBoxPreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            if (e.ChangedButton != MouseButton.Left)
                return;

            ListBoxItem item = GetItem(listBox, e.GetPosition);
            if (item != null && item.IsMouseOver)
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

                ListBoxItem item = GetItem(listBox, e.GetPosition);
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
