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
        public ObservableCollection<Mod> Mods { get; }

        /// <summary>
        /// The mods wrapped as view source.
        /// </summary>
        public CollectionViewSource ViewSource { get; }

        private void SetActive()
        {
            if (Mods.Count == 0 || activeChanging)
                return;

            bool? newValue = Mods[0].Active;
            for (int i = 1; i < Mods.Count; i++)
            {
                if (Mods[i].Active != newValue.Value)
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
                    foreach (Mod mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    SetActive();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Mod mod in e.OldItems)
                        mod.PropertyChanged -= ModPropertyChanged;
                    SetActive();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (Mod mod in e.NewItems)
                        mod.PropertyChanged += ModPropertyChanged;
                    foreach (Mod mod in e.OldItems)
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
            Mods = new ObservableCollection<Mod>();
            Mods.CollectionChanged += ModsChangedHandler;

            ViewSource = new CollectionViewSource();
            ViewSource.Source = Mods;
            ViewSource.SortDescriptions.Add(new SortDescription(nameof(Name), ListSortDirection.Ascending));
        }
    }
}
