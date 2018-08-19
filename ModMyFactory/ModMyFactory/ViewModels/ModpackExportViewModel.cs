using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class ModpackExportViewModel : ViewModelBase
    {
        bool downloadNewer;

        public bool DownloadNewer
        {
            get { return downloadNewer; }
            set
            {
                if (value != downloadNewer)
                {
                    downloadNewer = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DownloadNewer)));
                }
            }
        }

        public ListCollectionView ModpacksView { get; }

        public List<ModpackTemplate> Modpacks { get; }

        public bool CanExport => Modpacks.Any(template => template.Export);

        public ModpackExportViewModel()
        {
            if (!App.IsInDesignMode)
            {
                Modpacks = new List<ModpackTemplate>();
                foreach (var modpack in MainViewModel.Instance.Modpacks) Modpacks.Add(new ModpackTemplate(modpack, Modpacks));
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
