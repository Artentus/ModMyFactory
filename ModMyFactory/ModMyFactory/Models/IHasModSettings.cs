using ModMyFactory.Models.ModSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace ModMyFactory.Models
{
    interface IHasModSettings : INotifyPropertyChanged, IHasLocale
    {
        string Name { get; }

        GameCompatibleVersion Version { get; }

        Version FactorioVersion { get; }

        string DisplayName { get; }

        string UniqueID { get; }

        bool UseBinaryFileOverride { get; }

        bool Override { get; set; }

        bool HasSettings { get; }

        IReadOnlyCollection<IModSetting> Settings { get; }
        
        ICollectionView SettingsView { get; }
        
        ICommand ViewSettingsCommand { get; }

        void ViewSettings();
    }
}
