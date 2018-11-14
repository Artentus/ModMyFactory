using System;
using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.Models
{
    class ModVersionUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;

        public Mod Mod { get; }

        public Version Version => Mod.Version;

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

        public ModVersionUpdateInfo(Mod mod)
        {
            Mod = mod;
            isSelected = false;
        }
    }
}
