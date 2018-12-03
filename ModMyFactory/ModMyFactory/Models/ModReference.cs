using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ModMyFactory.Helpers;
using ModMyFactory.ViewModels;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    class ModReference : NotifyPropertyChangedBase, IModReference
    {
        public Mod Mod { get; }

        public string DisplayName => Mod.FriendlyName;

        public string VersionInfo => Mod.Version.ToString();

        public string FactorioVersionInfo => $"(Factorio {Mod.FactorioVersion})";

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

        public bool HasUnsatisfiedDependencies => Mod.HasUnsatisfiedDependencies;

        public IEnumerable<IHasModSettings> ModProxies => Mod.EnumerateSingle();

        public RelayCommand RemoveFromParentCommand { get; }

        public ModReference(Mod mod, Modpack parent)
        {
            Mod = mod;
            Image = new BitmapImage(new Uri("../Images/Document.png", UriKind.Relative));

            mod.PropertyChanged += PropertyChangedHandler;
            RemoveFromParentCommand = new RelayCommand(() =>
            {
                if (!parent.IsLocked)
                {
                    parent.Mods.Remove(this);

                    ModpackTemplateList.Instance.Update(MainViewModel.Instance.Modpacks);
                    ModpackTemplateList.Instance.Save();
                }
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
                case nameof(Mod.HasUnsatisfiedDependencies):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsatisfiedDependencies)));
                    break;
            }
        }
    }
}
