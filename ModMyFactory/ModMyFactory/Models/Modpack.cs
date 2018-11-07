using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ModMyFactory.ViewModels;
using ModMyFactory.MVVM.Sorters;
using WPFCore;
using WPFCore.Commands;
using System.Windows.Input;
using ModMyFactory.Views;
using System.Collections.Generic;
using System.Linq;
using ModMyFactory.Helpers;
using ModMyFactory.ModSettings;

namespace ModMyFactory.Models
{
    /// <summary>
    /// A collection of mods.
    /// </summary>
    class Modpack : NotifyPropertyChangedBase, IEditableObject
    {
        string name;
        string editingName;
        readonly ModpackCollection parentCollection;
        bool editing;
        bool? active;
        bool activeChanging;
        bool isSelected;
        bool contentsExpanded;
        bool hasUnsatisfiedDependencies;
        Dictionary<IModReference, IEnumerable<IHasModSettings>> proxyDict;

        private string GetUniqueName(string baseName)
        {
            int counter = 0;
            string candidateName = baseName;
            
            while (parentCollection.Contains(candidateName))
            {
                counter++;
                candidateName = $"{baseName} ({counter})";
            }

            return candidateName;
        }

        /// <summary>
        /// The name of the modpack.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    string newName = GetUniqueName(value);
                    name = newName;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));

                    EditingName = newName;
                }
            }
        }

        public string EditingName
        {
            get { return editingName; }
            set
            {
                if (value != editingName)
                {
                    editingName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(EditingName)));
                }
            }
        }

        /// <summary>
        /// Indicates whether the modpack is currently active.
        /// </summary>
        public bool? Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    activeChanging = true;
                    if (active.HasValue)
                    {
                        ModManager.BeginUpdateTemplates();

                        foreach (var mod in Mods)
                        {
                            if (mod.Active != active.Value)
                                mod.Active = active.Value;
                        }

                        ModManager.EndUpdateTemplates();
                        ModManager.SaveTemplates();
                    }
                    activeChanging = false;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        /// <summary>
        /// Indicates whether this modpack is selected in the list.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// Indicates whether the user currently edits the name of this modpack.
        /// </summary>
        public bool Editing
        {
            get { return editing; }
            set
            {
                if (value != editing)
                {
                    editing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Editing)));

                    if (editing)
                    {
                        ParentView.EditItem(this);
                    }
                    else
                    {
                        Name = EditingName;
                        ParentView.CommitEdit();

                        MainViewModel.Instance.Window.ModpacksListBox.ScrollIntoView(this);
                        ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                        ModpackTemplateList.Instance.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Indicates whether the contents of this modpack are currently expanded on the UI.
        /// </summary>
        public bool ContentsExpanded
        {
            get { return contentsExpanded; }
            set
            {
                if (value != contentsExpanded)
                {
                    contentsExpanded = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ContentsExpanded)));
                }
            }
        }

        /// <summary>
        /// Indicates whether this modpack has unsatisfied dependencies.
        /// </summary>
        public bool HasUnsatisfiedDependencies
        {
            get => hasUnsatisfiedDependencies;
            private set
            {
                if (value != hasUnsatisfiedDependencies)
                {
                    hasUnsatisfiedDependencies = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsatisfiedDependencies)));
                }
            }
        }

        /// <summary>
        /// The view that presents all contents of this modpack.
        /// </summary>
        public ListCollectionView ModsView { get; }

        /// <summary>
        /// The mods in this modpack.
        /// </summary>
        public ObservableCollection<IModReference> Mods { get; }

        /// <summary>
        /// The view this modpack is presented in.
        /// </summary>
        public IEditableCollectionView ParentView { get; set; }

        public IEnumerable<IHasModSettings> ModProxies => proxyDict.Values.ConcatAll();

        /// <summary>
        /// A command that deletes this modpack from the list.
        /// </summary>
        public RelayCommand<bool?> DeleteCommand { get; }

        /// <summary>
        /// A command that finishes renaming this modpack.
        /// </summary>
        public RelayCommand EndEditCommand { get; }

        /// <summary>
        /// Indicates whether any mods in this modpack have settings;
        /// </summary>
        public bool HasSettings
        {
            get
            {
                foreach (var proxy in ModProxies)
                {
                    if (proxy.HasSettings)
                        return true;
                }
                return false;
            }
        }

        public ICommand ViewSettingsCommand { get; }

        public void ViewSettings()
        {
            var proxyList = ModProxies.ToList();

            var settingsWindow = new ModSettingsWindow() { Owner = App.Instance.MainWindow };
            var settingsViewModel = (ModSettingsViewModel)settingsWindow.ViewModel;
            settingsViewModel.SetMods(proxyList);
            settingsWindow.ShowDialog();

            //ModSettingsManager.SaveSettings(proxyList);
        }

        /// <summary>
        /// Checks if this modpack contains a specified mod.
        /// </summary>
        public bool Contains(Mod mod, out ModReference reference)
        {
            foreach (var @ref in Mods)
            {
                var modReference = @ref as ModReference;
                if (modReference != null)
                {
                    if (modReference.Mod == mod)
                    {
                        reference = modReference;
                        return true;
                    }
                }
            }

            reference = null;
            return false;
        }

        /// <summary>
        /// Checks if this modpack contains a specified mod.
        /// </summary>
        public bool Contains(Mod mod)
        {
            ModReference reference;
            return Contains(mod, out reference);
        }

        /// <summary>
        /// Checks if this modpack contains a specified modpack.
        /// </summary>
        public bool Contains(Modpack modpack, out ModpackReference reference, bool recursive = false)
        {
            foreach (var @ref in Mods)
            {
                var modpackReference = @ref as ModpackReference;
                if (modpackReference != null)
                {
                    if (modpackReference.Modpack == modpack)
                    {
                        reference = modpackReference;
                        return true;
                    }

                    ModpackReference recursiveReference;
                    if (recursive && modpackReference.Modpack.Contains(modpack, out recursiveReference, true))
                    {
                        reference = recursiveReference;
                        return true;
                    }
                }
            }

            reference = null;
            return false;
        }

        /// <summary>
        /// Checks if this modpack contains a specified modpack.
        /// </summary>
        public bool Contains(Modpack modpack, bool recursive = false)
        {
            ModpackReference reference;
            return Contains(modpack, out reference, recursive);
        }

        private void SetActive()
        {
            if (Mods.Count == 0 || activeChanging)
                return;

            bool? newValue = Mods[0].Active;
            for (int i = 1; i < Mods.Count; i++)
            {
                if (Mods[i].Active != newValue)
                {
                    newValue = null;
                    break;
                }
            }

            if (newValue != active)
            {
                active = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
            }
        }

        private void SetHasUnsatisfiedDependencies()
        {
            if (Mods.Count == 0)
            {
                HasUnsatisfiedDependencies = false;
                return;
            }

            bool newValue = false;
            foreach (var mod in Mods)
            {
                if (mod.HasUnsatisfiedDependencies)
                {
                    newValue = true;
                    break;
                }
            }
            HasUnsatisfiedDependencies = newValue;
        }

        private void ModPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IModReference.Active):
                    SetActive();
                    break;
                case nameof(IModReference.HasUnsatisfiedDependencies):
                    SetHasUnsatisfiedDependencies();
                    break;
            }
        }

        private void ModsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IModReference mod in e.NewItems)
                    {
                        mod.PropertyChanged += ModPropertyChanged;
                        proxyDict.Add(mod, mod.ModProxies.Select(proxy => new ModSettingsProxy(proxy, this)));
                    }
                    SetActive();
                    SetHasUnsatisfiedDependencies();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IModReference mod in e.OldItems)
                    {
                        mod.PropertyChanged -= ModPropertyChanged;
                        proxyDict.Remove(mod);
                    }
                    SetActive();
                    SetHasUnsatisfiedDependencies();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (IModReference mod in e.NewItems)
                    {
                        mod.PropertyChanged += ModPropertyChanged;
                        proxyDict.Add(mod, mod.ModProxies.Select(proxy => new ModSettingsProxy(proxy, this)));
                    }
                    foreach (IModReference mod in e.OldItems)
                    {
                        mod.PropertyChanged -= ModPropertyChanged;
                        proxyDict.Remove(mod);
                    }
                    SetActive();
                    SetHasUnsatisfiedDependencies();
                    break;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasSettings)));
        }

        /// <summary>
        /// Deletes this modpack from the list.
        /// </summary>
        /// <param name="showPrompt">Indicates whether a confirmation prompt is shown to the user.</param>
        public void Delete(bool showPrompt)
        {
            if (!showPrompt || MessageBox.Show(
                App.Instance.GetLocalizedMessage("DeleteModpack", MessageType.Question),
                App.Instance.GetLocalizedMessageTitle("DeleteModpack", MessageType.Question),
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var modpack in parentCollection)
                {
                    ModpackReference reference;
                    if (modpack.Contains(this, out reference))
                        modpack.Mods.Remove(reference);

                }
                parentCollection.Remove(this);
            }
        }

        /// <summary>
        /// Creates a modpack.
        /// </summary>
        /// <param name="name">The name of the modpack.</param>
        /// <param name="parentCollection">The collection containing this modpack.</param>
        public Modpack(string name, ModpackCollection parentCollection)
        {
            this.parentCollection = parentCollection;
            Name = name;
            active = false;
            activeChanging = false;
            proxyDict = new Dictionary<IModReference, IEnumerable<IHasModSettings>>();

            Mods = new ObservableCollection<IModReference>();
            Mods.CollectionChanged += ModsChangedHandler;

            ModsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Mods);
            ModsView.CustomSort = new ModReferenceSorter();

            DeleteCommand = new RelayCommand<bool?>(showPrompt => Delete(showPrompt ?? true));
            EndEditCommand = new RelayCommand(EndEdit, () => Editing);
            ViewSettingsCommand = new RelayCommand(ViewSettings);
        }

        public void BeginEdit()
        {
            Editing = true;
        }

        public void EndEdit()
        {
            Editing = false;
        }

        public void CancelEdit()
        {
            throw new NotSupportedException();
        }
    }
}
