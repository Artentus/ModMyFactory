using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using ModMyFactory.Models;
using ModMyFactory.MVVM;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;

namespace ModMyFactory.ViewModels
{
    class ModUpdateViewModel : ViewModelBase<ModUpdateWindow>
    {
        ListCollectionView modsView;
        List<ModUpdateInfo> modsToUpdate;

        public ListCollectionView ModsView
        {
            get { return modsView; }
            private set
            {
                if (value != modsView)
                {
                    modsView = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsView)));
                }
            }
        }

        public List<ModUpdateInfo> ModsToUpdate
        {
            get { return modsToUpdate; }
            set
            {
                if (value != modsToUpdate)
                {
                    modsToUpdate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsToUpdate)));

                    ModsView = (ListCollectionView)(new CollectionViewSource() { Source = modsToUpdate }).View;
                    ModsView.CustomSort = new ModUpdateInfoSorter();
                }
            }
        }
    }
}
