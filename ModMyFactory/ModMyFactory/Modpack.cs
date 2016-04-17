using System.ComponentModel;

namespace ModMyFactory
{
    /// <summary>
    /// A collection of mods.
    /// </summary>
    class Modpack : NotifyPropertyChangedBase
    {
        string name;
        bool active;

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
        public bool Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    foreach (var mod in Mods)
                        mod.Active = active;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        /// <summary>
        /// The mods in this modpack.
        /// </summary>
        public BindingList<Mod> Mods { get; }

        private void AddingNewModHandler(object sender, AddingNewEventArgs e)
        {
            Mod mod = (Mod) e.NewObject;
            mod.Active = active;
        }

        /// <summary>
        /// Creates a modpack.
        /// </summary>
        /// <param name="name">The name of the modpack.</param>
        public Modpack(string name)
        {
            this.name = name;
            Mods = new BindingList<Mod>();
            Mods.AddingNew += AddingNewModHandler;
        }
    }
}
