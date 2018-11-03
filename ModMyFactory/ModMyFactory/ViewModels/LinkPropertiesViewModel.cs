using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class LinkPropertiesViewModel : ViewModelBase
    {
        FactorioVersion selectedVersion;

        Modpack selectedModpack;

        bool loadGame;
        FileInfo selectedSavegame;

        bool useArguments;
        string arguments;

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

                    EvaluateCanCreate();
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

        public bool LoadGame
        {
            get { return loadGame; }
            set
            {
                if (value != loadGame)
                {
                    loadGame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(LoadGame)));

                    EvaluateCanCreate();
                }
            }
        }

        public ListCollectionView SavegameView { get; }

        public FileInfo[] Savegames { get; }

        public FileInfo SelectedSavegame
        {
            get { return selectedSavegame; }
            set
            {
                if (value != selectedSavegame)
                {
                    selectedSavegame = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSavegame)));

                    EvaluateCanCreate();
                }
            }
        }

        public bool UseArguments
        {
            get { return useArguments; }
            set
            {
                if (value != useArguments)
                {
                    useArguments = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseArguments)));

                    EvaluateCanCreate();
                }
            }
        }

        public string Arguments
        {
            get { return arguments; }
            set
            {
                if (value != arguments)
                {
                    arguments = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Arguments)));

                    EvaluateCanCreate();
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

        private void EvaluateCanCreate()
        {
            bool value = true;

            if (SelectedVersion == null) value = false;
            if (LoadGame && (SelectedSavegame == null)) value = false;
            if (UseArguments && string.IsNullOrWhiteSpace(Arguments)) value = false;

            CanCreate = value;
        }

        public LinkPropertiesViewModel()
        {
            if (!App.IsInDesignMode)
            {
                FactorioVersions = MainViewModel.Instance.FactorioVersions;
                FactorioVersionsView = (ListCollectionView)(new CollectionViewSource() { Source = FactorioVersions }).View;
                FactorioVersionsView.CustomSort = new FactorioVersionSorter();

                Modpacks = MainViewModel.Instance.Modpacks;
                ModpacksView = (ListCollectionView)(new CollectionViewSource() { Source = Modpacks }).View;
                ModpacksView.CustomSort = new ModpackSorter();

                Savegames = App.Instance.Settings.GetSavegameDirectory().EnumerateFiles().Where(file => !file.Name.StartsWith("_autosave")).ToArray();
                SavegameView = (ListCollectionView)(new CollectionViewSource() { Source = Savegames }).View;
            }
        }
    }
}
