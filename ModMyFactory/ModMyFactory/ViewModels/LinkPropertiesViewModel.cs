using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ModMyFactory.MVVM;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;

namespace ModMyFactory.ViewModels
{
    sealed class LinkPropertiesViewModel : ViewModelBase<LinkPropertiesWindow>
    {
        FactorioVersion selectedVersion;
        Modpack selectedModpack;
        bool canCreate;

        public ListCollectionView FactorioVersionsView { get; }

        public ObservableCollection<FactorioVersion> FactorioVersions { get; }

        public FactorioVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));

                    CanCreate = selectedVersion != null;
                }
            }
        }

        public ListCollectionView ModpacksView { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        public Modpack SelectedModpack
        {
            get { return selectedModpack; }
            set
            {
                if (value != selectedModpack)
                {
                    selectedModpack = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModpack)));
                }
            }
        }

        public bool CanCreate
        {
            get { return canCreate; }
            private set
            {
                if (value != canCreate)
                {
                    canCreate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanCreate)));
                }
            }
        }

        public LinkPropertiesViewModel()
        {
            FactorioVersions = MainViewModel.Instance.FactorioVersions;
            FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
            FactorioVersionsView.CustomSort = new FactorioVersionSorter();

            Modpacks = MainViewModel.Instance.Modpacks;
            ModpacksView = (ListCollectionView)(new CollectionViewSource() { Source = Modpacks }).View;
            ModpacksView.CustomSort = new ModpackSorter();
        }
    }
}
