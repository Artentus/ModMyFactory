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
        bool activeEditing;
        bool propertyChanged;
        
        bool useNewestVersion;
        bool useSpecificVersion;
        bool useFactorioVersion;
        bool include;

        public bool UseNewestVersion
        {
            get { return useNewestVersion; }
            set
            {
                if (value != useNewestVersion)
                {
                    useNewestVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseNewestVersion)));

                    if (!propertyChanged)
                    {
                        if (useNewestVersion)
                        {
                            UseSpecificVersion = false;
                            UseFactorioVersion = false;
                            
                            activeEditing = true;
                            foreach (var modpackTemplate in Modpacks)
                            {
                                foreach (var modTemplate in modpackTemplate.ModTemplates)
                                    modTemplate.UseNewestVersion = true;
                            }
                            activeEditing = false;
                        }
                    }
                }
            }
        }

        public bool UseSpecificVersion
        {
            get { return useSpecificVersion; }
            set
            {
                if (value != useSpecificVersion)
                {
                    useSpecificVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseSpecificVersion)));

                    if (!propertyChanged)
                    {
                        if (useSpecificVersion)
                        {
                            UseNewestVersion = false;
                            UseFactorioVersion = false;
                            
                            activeEditing = true;
                            foreach (var modpackTemplate in Modpacks)
                            {
                                foreach (var modTemplate in modpackTemplate.ModTemplates)
                                    modTemplate.UseSpecificVersion = true;
                            }
                            activeEditing = false;
                        }
                    }
                }
            }
        }

        public bool UseFactorioVersion
        {
            get { return useFactorioVersion; }
            set
            {
                if (value != useFactorioVersion)
                {
                    useFactorioVersion = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(UseFactorioVersion)));

                    if (!propertyChanged)
                    {
                        if (useFactorioVersion)
                        {
                            UseNewestVersion = false;
                            UseSpecificVersion = false;
                            
                            activeEditing = true;
                            foreach (var modpackTemplate in Modpacks)
                            {
                                foreach (var modTemplate in modpackTemplate.ModTemplates)
                                    modTemplate.UseFactorioVersion = true;
                            }
                            activeEditing = false;
                        }
                    }
                }
            }
        }

        public bool Include
        {
            get { return include; }
            set
            {
                if (value != include)
                {
                    include = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Include)));

                    if (!propertyChanged)
                    {
                        if (include) UseSpecificVersion = true;

                        activeEditing = true;
                        foreach (var modpackTemplate in Modpacks)
                        {
                            foreach (var modTemplate in modpackTemplate.ModTemplates)
                                modTemplate.Include = include;
                        }
                        activeEditing = false;
                    }
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

                useNewestVersion = true;

                CommandManager.RequerySuggested += CanExportChanged;
                foreach (var modpackTemplate in Modpacks)
                {
                    foreach (var modTemplate in modpackTemplate.ModTemplates)
                        modTemplate.PropertyChanged += ModTemplatePropertyChanged;
                }
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

        private void ReevaluateIncludeStatus()
        {
            Include = Modpacks.SelectMany(modpackTemplate => modpackTemplate.ModTemplates).All(modTemplate => modTemplate.Include);
        }

        private void ReevaluateExportModeStatus()
        {
            bool useNewest = true;
            bool useSpecific = true;
            bool useFactorio = true;

            foreach (var modpackTemplate in Modpacks)
            {
                if (!useNewest && !useSpecific && !useFactorio) break;

                foreach (var modTemplate in modpackTemplate.ModTemplates)
                {
                    if (!useNewest && !useSpecific && !useFactorio) break;

                    if (!modTemplate.UseNewestVersion) useNewest = false;
                    if (!modTemplate.UseSpecificVersion) useSpecific = false;
                    if (!modTemplate.UseFactorioVersion) useFactorio = false;
                }
            }

            UseNewestVersion = useNewest;
            UseSpecificVersion = useSpecific;
            UseFactorioVersion = useFactorio;
        }

        private void ModTemplatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!activeEditing)
            {
                propertyChanged = true;
                switch (e.PropertyName)
                {
                    case nameof(ModTemplate.Include):
                        ReevaluateIncludeStatus();
                        break;
                    case nameof(ModTemplate.ExportMode):
                        ReevaluateExportModeStatus();
                        break;
                }
                propertyChanged = false;
            }
        }
    }
}
