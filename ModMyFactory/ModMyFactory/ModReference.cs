using System.ComponentModel;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    class ModReference : NotifyPropertyChangedBase, IModReference
    {
        public Mod Mod { get; }

        public string DisplayName => Mod.Name;

        public bool? Active
        {
            get { return Mod.Active; }
            set
            {
                if (value.HasValue)
                    Mod.Active = value.Value;
            }
        }

        public RelayCommand RemoveFromParentCommand { get; }

        public ModReference(Mod mod, Modpack parent)
        {
            Mod = mod;

            mod.PropertyChanged += PropertyChangedHandler;
            RemoveFromParentCommand = new RelayCommand(() => parent.Mods.Remove(this));
        }

        ~ModReference()
        {
            Mod.PropertyChanged -= PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ModMyFactory.Mod.Name):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                    break;
                case nameof(ModMyFactory.Mod.Active):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                    break;
            }
        }
    }
}
