using System;
using System.ComponentModel;
using System.Linq;
using WPFCore;

namespace ModMyFactory.Models
{
    class ModVersionUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;

        public Mod Mod { get; }

        public Version Version => Mod.Version;

        public int ModpackCount { get; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public ModVersionUpdateInfo(Mod mod, ModpackCollection modpacks)
        {
            Mod = mod;
            isSelected = App.Instance.Settings.PreSelectModVersions;
            ModpackCount = modpacks.Count(pack => pack.Contains(mod));
        }
    }
}
