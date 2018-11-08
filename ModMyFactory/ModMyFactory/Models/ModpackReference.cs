using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    class ModpackReference : NotifyPropertyChangedBase, IModReference
    {
        public Modpack Modpack { get; }

        public string DisplayName => Modpack.Name;

        public string VersionInfo => string.Empty;

        public string FactorioVersionInfo => string.Empty;

        public BitmapImage Image { get; }

        public bool? Active
        {
            get { return Modpack.Active; }
            set { Modpack.Active = value; }
        }

        public bool HasUnsatisfiedDependencies => Modpack.HasUnsatisfiedDependencies;

        /// <summary>
        /// The view this modpack reference is presented in.
        /// </summary>
        public IEditableCollectionView ParentView { get; set; }

        public IEnumerable<IHasModSettings> ModProxies => Modpack.ModProxies;

        public RelayCommand RemoveFromParentCommand { get; }

        public ModpackReference(Modpack modpack, Modpack parent)
        {
            Modpack = modpack;
            Image = new BitmapImage(new Uri("../Images/Package.png", UriKind.Relative));

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
                case nameof(Modpack.HasUnsatisfiedDependencies):
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasUnsatisfiedDependencies)));
                    break;
                case nameof(Modpack.Editing):
                    if (Modpack.Editing)
                    {
                        ParentView.EditItem(this);
                    }
                    else
                    {
                        ParentView.CommitEdit();
                    }
                    break;
            }
        }
    }
}
