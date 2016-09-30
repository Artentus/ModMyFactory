using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ModMyFactory.MVVM;
using ModMyFactory.Models;
using ModMyFactory.ViewModels;

namespace ModMyFactory
{
    class ModReference : NotifyPropertyChangedBase, IModReference
    {
        public Mod Mod { get; }

        public string DisplayName => Mod.Name;

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

        public List<IEditableCollectionView> ParentViews => null;

        public RelayCommand RemoveFromParentCommand { get; }

        public ModReference(Mod mod, Modpack parent)
        {
            Mod = mod;
            Image = new BitmapImage(new Uri("Images/Document.png", UriKind.Relative));

            mod.PropertyChanged += PropertyChangedHandler;
            RemoveFromParentCommand = new RelayCommand(() =>
            {
                parent.Mods.Remove(this);

                MainViewModel.Instance.ModpackTemplateList.Update(MainViewModel.Instance.Modpacks);
                MainViewModel.Instance.ModpackTemplateList.Save();
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
                case nameof(Mod.Name):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                    break;
                case nameof(Mod.Active):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                    break;
            }
        }
    }
}
