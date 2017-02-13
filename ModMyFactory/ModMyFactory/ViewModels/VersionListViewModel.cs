using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ModMyFactory.Web;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class VersionListViewModel : ViewModelBase
    {
        bool showExperimentalVersions;
        FactorioOnlineVersion selectedVersion;
        bool canAdd;

        public ObservableCollection<FactorioOnlineVersion> FactorioVersions { get; set; }

        public ListCollectionView FactorioVersionsView { get; }

        public bool ShowExperimentalVersions
        {
            get { return showExperimentalVersions; }
            set
            {
                if (value != showExperimentalVersions)
                {
                    showExperimentalVersions = value;
                    FactorioVersionsView.Refresh();

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShowExperimentalVersions)));

                    App.Instance.Settings.ShowExperimentalDownloads = showExperimentalVersions;
                    App.Instance.Settings.Save();
                }
            }
        }

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

        private bool VersionFilter(object obj)
        {
            FactorioOnlineVersion version = obj as FactorioOnlineVersion;
            if (version != null)
            {
                return !version.IsExperimental || ShowExperimentalVersions;
            }
            else
            {
                return false;
            }
        }

        public VersionListViewModel()
        {
            FactorioVersions = new ObservableCollection<FactorioOnlineVersion>();
            FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
            FactorioVersionsView.Filter = VersionFilter;

            if (!App.IsInDesignMode)
            {
                showExperimentalVersions = App.Instance.Settings.ShowExperimentalDownloads;
            }
        }
    }
}
