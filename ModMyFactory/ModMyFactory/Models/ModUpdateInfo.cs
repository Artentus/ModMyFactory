using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ModMyFactory.Web.ModApi;
using WPFCore;

namespace ModMyFactory.Models
{
    class ModUpdateInfo : NotifyPropertyChangedBase
    {
        bool isSelected;
        
        public ModRelease Update { get; }

        public string ModName { get; }

        public string FriendlyName { get; }

        public Version UpdateVersion => Update.Version;

        public Version FactorioVersion => Update.InfoFile.FactorioVersion;

        public List<ModVersionUpdateInfo> ModVersions { get; }

        public bool Extract => ModVersions.Any(version => version.Mod.ExtractUpdates);
        
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));

                    if (!isSelected)
                    {
                        foreach (var version in ModVersions)
                            version.IsSelected = false;
                    }
                }
            }
        }

        public ModUpdateInfo(string modName, string friendlyName, ModRelease update)
        {
            ModName = modName;
            FriendlyName = friendlyName;
            Update = update;
            isSelected = true;
            ModVersions = new List<ModVersionUpdateInfo>();
        }
    }
}
