using ModMyFactory.Models;
using ModMyFactory.Models.ModSettings;
using ModMyFactory.MVVM.Sorters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class ModSettingsViewModel : ViewModelBase
    {
        bool multiSelect;
        IHasModSettings selectedMod;

        public bool MultiSelect
        {
            get => multiSelect;
            private set
            {
                if (value != multiSelect)
                {
                    multiSelect = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(MultiSelect)));
                }
            }
        }

        public IList<IHasModSettings> Mods { get; private set; }

        public ICollectionView ModsView { get; private set; }

        public IHasModSettings SelectedMod
        {
            get => selectedMod;
            set
            {
                if (value != selectedMod)
                {
                    selectedMod = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMod)));
                    
                    SelectedModSettings = selectedMod.Settings;
                    SelectedModSettingsView = selectedMod.SettingsView;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModOverride)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettings)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettingsView)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettingGroups)));

                    SelectedModSettingGroupIndex = 0;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettingGroupIndex)));
                }
            }
        }

        public bool SelectedModOverride
        {
            get => SelectedMod?.Override ?? false;
            set { if (SelectedMod != null) SelectedMod.Override = value; }
        }

        public IReadOnlyCollection<IModSetting> SelectedModSettings { get; private set; }

        public ICollectionView SelectedModSettingsView { get; private set; }

        public ReadOnlyObservableCollection<object> SelectedModSettingGroups => SelectedModSettingsView?.Groups;

        public object SelectedModSettingGroupIndex { get; set; }

        public void SetMod(IHasModSettings mod)
        {
            MultiSelect = false;

            Mods = new List<IHasModSettings>() { mod };
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));
            ModsView = CollectionViewSource.GetDefaultView(Mods);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsView)));

            SelectedMod = mod;
        }

        public void SetMods(IList<IHasModSettings> mods)
        {
            MultiSelect = true;

            Mods = mods;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));

            var source = new CollectionViewSource() { Source = Mods };
            var view = (ListCollectionView)source.View;
            view.CustomSort = new ModSorter();
            view.Filter = ModFilter;
            ModsView = view;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsView)));

            SelectedMod = Mods.FirstOrDefault(mod => mod.HasSettings);
        }

        private bool ModFilter(object item)
        {
            var mod = item as IHasModSettings;
            if (mod == null) return false;
            return mod.HasSettings;
        }
    }
}
