using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using ModMyFactory.Models;
using ModMyFactory.MVVM;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;

namespace ModMyFactory.ViewModels
{
    sealed class UpdateListViewModel : ViewModelBase<UpdateListWindow>
    {
        ListCollectionView updateTargetsView;
        List<UpdateTarget> updateTargets;
        UpdateTarget selectedTarget;
        bool canUpdate;

        public ListCollectionView UpdateTargetsView
        {
            get { return updateTargetsView; }
            private set
            {
                updateTargetsView = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(UpdateTargetsView)));
            }
        }

        public List<UpdateTarget> UpdateTargets
        {
            get { return updateTargets; }
            set
            {
                if (value != updateTargets)
                {
                    updateTargets = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UpdateTargets)));

                    UpdateTargetsView = (ListCollectionView)CollectionViewSource.GetDefaultView(UpdateTargets);
                    UpdateTargetsView.CustomSort = new UpdateTargetSorter();
                }
            }
        } 

        public UpdateTarget SelectedTarget
        {
            get { return selectedTarget; }
            set
            {
                if (value != selectedTarget)
                {
                    selectedTarget = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedTarget)));
                    CanUpdate = selectedTarget != null;
                }
            }
        }

        public bool CanUpdate
        {
            get { return canUpdate; }
            private set
            {
                if (value != canUpdate)
                {
                    canUpdate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanUpdate)));
                }
            }
        }
    }
}
