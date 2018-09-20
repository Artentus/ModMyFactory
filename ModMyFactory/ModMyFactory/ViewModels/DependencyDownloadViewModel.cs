using ModMyFactory.Models;
using ModMyFactory.MVVM.Sorters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class DependencyDownloadViewModel : ViewModelBase
    {
        List<ModDependencyInfo> dependencies;
        bool showOptional;

        public List<ModDependencyInfo> Dependencies
        {
            get => dependencies;
            set
            {
                if (value != dependencies)
                {
                    dependencies = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Dependencies)));

                    var source = new CollectionViewSource() { Source = dependencies };
                    DependenciesView = (ListCollectionView)source.View;
                    DependenciesView.Filter = DependencyFilter;
                    DependenciesView.CustomSort = new ModDependencySorter();
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DependenciesView)));
                }
            }
        }

        public ListCollectionView DependenciesView { get; private set; }

        public bool ShowOptional
        {
            get => showOptional;
            set
            {
                if (value != showOptional)
                {
                    showOptional = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShowOptional)));

                    if (!showOptional)
                    {
                        foreach (var dependency in Dependencies)
                        {
                            if (dependency.IsOptional)
                                dependency.IsSelected = false;
                        }
                    }

                    DependenciesView.Refresh();

                    App.Instance.Settings.ShowOptionalDependencies = showOptional;
                    App.Instance.Settings.Save();
                }
            }
        }

        public bool CanDownload => Dependencies?.Any(dependency => dependency.IsSelected) ?? false;

        private bool DependencyFilter(object item)
        {
            var dependency = item as ModDependencyInfo;
            if (dependency == null) return false;

            return ShowOptional || !dependency.IsOptional;
        }
        
        public DependencyDownloadViewModel()
        {
            if (!App.IsInDesignMode)
                showOptional = App.Instance.Settings.ShowOptionalDependencies;

            CommandManager.RequerySuggested += CanDownloadChanged;
        }

        ~DependencyDownloadViewModel()
        {
            CommandManager.RequerySuggested -= CanDownloadChanged;
        }

        private void CanDownloadChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CanDownload)));
        }
    }
}
