using System.Collections.ObjectModel;
using System.Windows.Data;
using ModMyFactory.MVVM;
using ModMyFactory.Views;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.ViewModels
{
    sealed class OnlineModsViewModel : ViewModelBase<OnlineModsWindow>
    {
        public ListCollectionView ModsView { get; }

        public ObservableCollection<ModInfo> Mods { get; }

        public OnlineModsViewModel()
        {
            Mods = new ObservableCollection<ModInfo>();
            ModsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Mods);
        }
    }
}
