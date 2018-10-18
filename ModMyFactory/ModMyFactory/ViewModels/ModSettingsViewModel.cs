using ModMyFactory.Models;
using ModMyFactory.Models.ModSettings;
using System.Collections.Generic;
using System.ComponentModel;
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

                    SelectedModOverride = selectedMod.Override;
                    SelectedModSettings = selectedMod.Settings;
                    SelectedModSettingsView = selectedMod.SettingsView;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModOverride)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettings)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedModSettingsView)));
                }
            }
        }

        public bool SelectedModOverride { get; private set; }

        public IReadOnlyCollection<IModSetting> SelectedModSettings { get; private set; }

        public ICollectionView SelectedModSettingsView { get; private set; }

        public void SetMod(IHasModSettings mod)
        {
            MultiSelect = false;

            Mods = null;
            ModsView = null;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Mods)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ModsView)));

            SelectedMod = mod;
        }

        public void SetMods(IList<IHasModSettings> mods)
        {
            MultiSelect = true;
        }
    }
}
