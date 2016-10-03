using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using ModMyFactory.MVVM;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.ViewModels
{
    sealed class OnlineModsViewModel : ViewModelBase<OnlineModsWindow>
    {
        ListCollectionView modsView;
        List<ModInfo> mods;
        string filter;

        ModInfo selectedMod;
        ExtendedModInfo extendedInfo;
        string selectedModName;
        string selectedModDescription;

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

        public List<ModInfo> Mods
        {
            get { return mods; }
            set
            {
                if (value != mods)
                {
                    mods = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));

                    ModsView = (ListCollectionView)CollectionViewSource.GetDefaultView(Mods);
                }
            }
        }

        public string Filter
        {
            get { return filter; }
            set
            {
                if (value != filter)
                {
                    filter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Filter)));

                    modsView.Refresh();
                }
            }
        }

        public ModInfo SelectedMod
        {
            get { return selectedMod; }
            set
            {
                selectedMod = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMod)));

                SelectedModName = selectedMod.Title;

                new Action(async () => await LoadExtendedModInfoAsync(selectedMod)).Invoke();
            }
        }

        public ExtendedModInfo ExtendedInfo
        {
            get { return extendedInfo; }
            private set
            {
                extendedInfo = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ExtendedInfo)));

                SelectedModDescription = extendedInfo.Description;
                SelectedReleases.Clear();
                foreach (var release in extendedInfo.Releases)
                    SelectedReleases.Add(release);
            }
        }

        public string SelectedModName
        {
            get { return selectedModName; }
            private set
            {
                if (value != selectedModName)
                {
                    selectedModName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModName)));
                }
            }
        }

        public string SelectedModDescription
        {
            get { return string.IsNullOrWhiteSpace(selectedModDescription) ? selectedMod.Summary : selectedModDescription; }
            set
            {
                if (value != selectedModDescription)
                {
                    selectedModDescription = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModDescription)));
                }
            }
        }

        public ObservableCollection<ModRelease> SelectedReleases { get; } 

        private async Task LoadExtendedModInfoAsync(ModInfo mod)
        {
            ExtendedModInfo extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod);
            ExtendedInfo = extendedInfo;
        }

        public OnlineModsViewModel()
        {
            SelectedReleases = new ObservableCollection<ModRelease>();
        }
    }
}
