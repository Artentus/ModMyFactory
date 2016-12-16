using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.Views;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class ModpackExportViewModel : ViewModelBase
    {
        bool includeVersionInfo;

        public ListCollectionView ModpacksView { get; }

        public ObservableCollection<Modpack> Modpacks { get; }

        public bool CanExport => ((View as ModpackExportWindow)?.ModpackListBox.SelectedItems?.Count ?? 0) > 0;

        public bool IncludeVersionInfo
        {
            get { return includeVersionInfo; }
            set
            {
                if (value != includeVersionInfo)
                {
                    includeVersionInfo = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IncludeVersionInfo)));
                }
            }
        }

        public ModpackExportViewModel()
        {
            if (!App.IsInDesignMode)
            {
                Modpacks = MainViewModel.Instance.Modpacks;
                ModpacksView = (ListCollectionView)(new CollectionViewSource() { Source = Modpacks }).View;
                ModpacksView.CustomSort = new ModpackSorter();

                CommandManager.RequerySuggested += CanExportChanged;
            }
        }

        ~ModpackExportViewModel()
        {
            CommandManager.RequerySuggested -= CanExportChanged;
        }

        private void CanExportChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanExport)));
        }
    }
}
