using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ModMyFactory.ViewModels;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    class ModReference : NotifyPropertyChangedBase, IModReference
    {
        public Mod Mod { get; }

        public string DisplayName => Mod.FriendlyName;

        public string VersionInfo => $"({Mod.FactorioVersion})";

        public BitmapImage Image { get; }

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
            Image = new BitmapImage(new Uri("../Images/Document.png", UriKind.Relative));

            mod.PropertyChanged += PropertyChangedHandler;
            RemoveFromParentCommand = new RelayCommand(() =>
            {
                parent.Mods.Remove(this);

                ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                ModpackTemplateList.Instance.Save();
            });
        }

        ~ModReference()
        {
            Mod.PropertyChanged -= PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Mod.FriendlyName):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                    break;
                case nameof(Mod.Active):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                    break;
            }
        }
    }
}
