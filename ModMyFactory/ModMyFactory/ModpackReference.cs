using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    class ModpackReference : NotifyPropertyChangedBase, IModReference
    {
        public Modpack Modpack { get; }

        public string DisplayName => Modpack.Name;

        public BitmapImage Image { get; }

        public bool? Active
        {
            get { return Modpack.Active; }
            set { Modpack.Active = value; }
        }

        public List<IEditableCollectionView> ParentViews => Modpack.ParentViews;

        public RelayCommand RemoveFromParentCommand { get; }

        public ModpackReference(Modpack modpack, Modpack parent)
        {
            Modpack = modpack;
            Image = new BitmapImage(new Uri("Images/Package.png", UriKind.Relative));

            modpack.PropertyChanged += PropertyChangedHandler;
            RemoveFromParentCommand = new RelayCommand(() => parent.Mods.Remove(this));
        }

        ~ModpackReference()
        {
            Modpack.PropertyChanged -= PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Modpack.Name):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                    break;
                case nameof(Modpack.Active):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                    break;
                //case nameof(Modpack.Editing):
                //    if (Modpack.Editing)
                //    {
                //        foreach (var view in ParentViews)
                //            view.EditItem(this);
                //    }
                //    break;
            }
        }
    }
}
