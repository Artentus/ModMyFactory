using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ModMyFactory.MVVM.Sorters;
using WPFCore;
using ModMyFactory.Helpers;

namespace ModMyFactory.Models
{
    sealed class ModpackTemplate : NotifyPropertyChangedBase
    {
        readonly List<ModpackTemplate> parentCollection; 

        bool export;
        bool forcedExport;

        public bool Export
        {
            get { return export; }
            set
            {
                if (value != export)
                {
                    export = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Export)));

                    foreach (var template in parentCollection)
                    {
                        if (template != this)
                        {
                            if (ModpackTemplates.Contains(template, (innerTemplate, outerTemplate) => (innerTemplate.Modpack == outerTemplate.Modpack)))
                                template.ForcedExport = export;
                        }
                    }
                }
            }
        }

        public bool ForcedExport
        {
            get { return forcedExport; }
            set
            {
                if (value != forcedExport)
                {
                    forcedExport = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForcedExport)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(AllowExportChange)));

                    if (forcedExport) Export = true;
                }
            }
        }

        public bool AllowExportChange => !ForcedExport;




        public Modpack Modpack { get; }

        public string Name => Modpack.Name;

        public ListCollectionView ModTemplateView { get; }

        public List<ModTemplate> ModTemplates { get; }

        public ListCollectionView ModpackTemplateView { get; }

        public List<InnerModpackTemplate> ModpackTemplates { get; }

        public ModpackTemplate(Modpack modpack, List<ModpackTemplate> parentCollection)
        {
            Modpack = modpack;
            this.parentCollection = parentCollection;

            ModTemplates = modpack.Mods.Where(reference => reference is ModReference).Select(reference => new ModTemplate(((ModReference)reference).Mod)).ToList();
            ModTemplateView = (ListCollectionView)(new CollectionViewSource() { Source = ModTemplates }).View;
            ModTemplateView.CustomSort = new ModSorter();

            ModpackTemplates = modpack.Mods.Where(reference => reference is ModpackReference).Select(reference => new InnerModpackTemplate(((ModpackReference)reference).Modpack, parentCollection)).ToList();
            ModpackTemplateView = (ListCollectionView)(new CollectionViewSource() { Source = ModpackTemplates }).View;
            ModpackTemplateView.CustomSort = new ModpackSorter();
        }
    }

    sealed class InnerModpackTemplate
    {
        readonly List<ModpackTemplate> parentCollection;

        public Modpack Modpack { get; }

        public string Name => Modpack.Name;

        public ModpackTemplate OuterTemplate => parentCollection.First(template => template.Modpack == Modpack);

        public InnerModpackTemplate(Modpack modpack, List<ModpackTemplate> parentCollection)
        {
            Modpack = modpack;
            this.parentCollection = parentCollection;
        }
    }
}
