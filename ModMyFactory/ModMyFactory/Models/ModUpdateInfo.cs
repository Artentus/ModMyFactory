using System;
using System.ComponentModel;
using ModMyFactory.Web.ModApi;
using WPFCore;

namespace ModMyFactory.Models
{
    class ModUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;

        public Mod Mod { get; }

        public ModRelease Update { get; }

        public string Name => Mod.Name;

        public string FriendlyName => Mod.FriendlyName;

        public Version CurrentVersion => Mod.Version;

        public Version UpdateVersion => Update.Version;

        public Version CurrentFactorioVersion => Mod.FactorioVersion;

        public Version UpdateFactorioVersion => Update.InfoFile.FactorioVersion;

        public bool CreateNewMod { get; }

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

        public ModUpdateInfo(Mod mod, ModRelease update, bool createNewMod)
        {
            Mod = mod;
            Update = update;
            CreateNewMod = createNewMod;
            isSelected = true;
        }
    }
}
