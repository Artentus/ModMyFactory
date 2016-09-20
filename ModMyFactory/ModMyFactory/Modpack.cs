using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    /// <summary>
    /// A collection of mods.
    /// </summary>
    class Modpack : NotifyPropertyChangedBase
    {
        string name;
        bool? active;
        bool activeChanging;

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
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
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
                        foreach (var mod in Mods)
                            if (mod.Active != active.Value) mod.Active = active.Value;
                    }
                    activeChanging = false;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        /// <summary>
        /// The mods in this modpack.
        /// </summary>
        public ObservableCollection<IModReference> Mods { get; }

        /// <summary>
        /// The mods wrapped as view source.
        /// </summary>
        public CollectionViewSource ViewSource { get; }

        /// <summary>
        /// Checks if this modpack contains a specified mod.
        /// </summary>
        public bool Contains(Mod mod)
        {
            foreach (var reference in Mods)
            {
                var modReference = reference as ModReference;
                if (modReference != null)
                {
                    if (modReference.Mod == mod)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if this modpack contains a specified modpack.
        /// </summary>
        public bool Contains(Modpack modpack, bool recursive = false)
        {
            foreach (var reference in Mods)
            {
                var modpackReference = reference as ModpackReference;
                if (modpackReference != null)
                {
                    if (modpackReference.Modpack == modpack)
                        return true;

                    if (recursive && modpackReference.Modpack.Contains(modpack, true))
                        return true;
                }
            }

            return false;
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

        private void ModPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Mod.Active))
            {
                SetActive();
            }
        }

        private void ModsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IModReference mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    SetActive();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IModReference mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    SetActive();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (IModReference mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    foreach (IModReference mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    SetActive();
                    break;
            }
        }

        /// <summary>
        /// Creates a modpack.
        /// </summary>
        /// <param name="name">The name of the modpack.</param>
        public Modpack(string name)
        {
            this.name = name;
            active = false;
            activeChanging = false;
            Mods = new ObservableCollection<IModReference>();
            Mods.CollectionChanged += ModsChangedHandler;

            ViewSource = new CollectionViewSource();
            ViewSource.Source = Mods;
            ViewSource.SortDescriptions.Add(new SortDescription(nameof(Name), ListSortDirection.Ascending));
        }
    }
}
