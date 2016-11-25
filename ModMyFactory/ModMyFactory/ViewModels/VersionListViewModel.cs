using System.Collections.ObjectModel;
using System.ComponentModel;
using ModMyFactory.Web;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class VersionListViewModel : ViewModelBase
    {
        FactorioOnlineVersion selectedVersion;
        bool canAdd;

        public ObservableCollection<FactorioOnlineVersion> FactorioVersions { get; set; }

        public FactorioOnlineVersion SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                if (value != selectedVersion)
                {
                    selectedVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedVersion)));
                    CanAdd = selectedVersion != null;
                }
            }
        }

        public bool CanAdd
        {
            get { return canAdd; }
            private set
            {
                if (value != canAdd)
                {
                    canAdd = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanAdd)));
                }
            }
        }

        public VersionListViewModel()
        {
            FactorioVersions = new ObservableCollection<FactorioOnlineVersion>();
        }
    }
}
