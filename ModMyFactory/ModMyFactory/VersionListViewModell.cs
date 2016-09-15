using System.Collections.Generic;
using System.ComponentModel;
using ModMyFactory.Web;

namespace ModMyFactory
{
    sealed class VersionListViewModell : NotifyPropertyChangedBase
    {
        FactorioOnlineVersion selectedVersion;
        bool canAdd;

        public List<FactorioOnlineVersion> FactorioVersions { get; }

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

        public VersionListViewModell(List<FactorioOnlineVersion> factorioVersions)
        {
            FactorioVersions = factorioVersions;
        }
    }
}
